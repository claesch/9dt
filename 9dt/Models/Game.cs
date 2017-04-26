using _9dt.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _9dt.Models
{
    public enum GameState
    {
        IN_PROGRESS,
        DONE
    }

    public enum MoveType
    {
        QUIT,
        MOVE
    }

    public struct Move
    {
        public Move(MoveType type, string player, int? column = null, int? row= null)
        {
            Type = type;
            Column = column;
            Player = player;
            Row = row;
        }
        public MoveType Type { get; }
        public int? Column { get; }
        internal int? Row { get; }
        public string Player { get; }
    }


    public class Game
    {
        private readonly string _player1;
        private readonly string _player2;
        private readonly int _rows;
        private readonly int _columns;
        private GameState _state;
        private string _winner = null;

        public string Id { get; }
        public GameState State { get { return _state; } }
        public string Winner { get { return _winner; } }
        public string Player1 { get { return _player1; } }
        public string Player2 { get { return _player2; } }
        public List<Move> Moves { get; }

        public Game(string player1, string player2, int rows = 4, int columns = 4)
        {
            Moves = new List<Move>();
            _player1 = player1;
            _player2 = player2;
            _rows = rows;
            _columns = columns;
            Id = Guid.NewGuid().ToString();
            _state = GameState.IN_PROGRESS;
        }

        [Obsolete]
        public void SetWinner(string player)
        {
            _winner = player;
            _state = GameState.DONE;
        }

        private void SetDraw()
        {
            _state = GameState.DONE;
        }

        internal void Quit(string quitterId)
        {
            VerifyPlayerPartOfGame(quitterId);

            if (_state == GameState.DONE)
                throw new MoveNotAllowedException();

            AddMove(MoveType.QUIT, quitterId);

            var winner = (_player1 == quitterId) ? _player2 : _player1;
            SetWinner(winner);
        }

        internal int AddMove(MoveType type, string player, int? column = null)
        {
            VerifyPlayerPartOfGame(player);
            VerifyItIsPlayerTurn(player);

            switch (type)
            {
                case MoveType.QUIT:
                    Moves.Add(new Move(type, player));
                    break;
                case MoveType.MOVE:
                    VerifyColumnExists(column);
                    var row = VerifyColumnHasRoom((int)column);
                    Moves.Add(new Move(type, player, column, row));
                    if (IsGameWon(player, (int)column, row))
                    {
                        SetWinner(player);
                    }
                    else if (Moves.Count == _rows * _columns)
                    {
                        SetDraw();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return Moves.Count() - 1;
        }

        internal Move GetMove(int moveNumber)
        {
            if (moveNumber >= Moves.Count() || moveNumber < 0)
                throw new MoveNotFoundException();
            return Moves[moveNumber];
        }

        internal List<Move> GetMoves(int? start, int? end)
        {
            VerifyStartAndEndIndex(start, end);

            if (Moves.Count() == 0)
            { return Moves; }

            int startIndex = start ?? 0;
            int endIndex = end ?? Moves.Count() - 1;
            return Moves.GetRange(startIndex, endIndex - startIndex + 1);
        }

        //NOTE: Assumes 4x4
        private bool IsGameWon(string player, int column, int row)
        {
            var colMovesForPlayer = Moves.Where(m => m.Column == column && m.Player == player);
            if (colMovesForPlayer.Count() == 4)
                return true;
            var rowMovesForPlayer = Moves.Where(m => m.Row == row && m.Player == player);
            if (rowMovesForPlayer.Count() == 4)
                return true;
            if (Moves.Count() >= 10)
            {
                if(DescendingDiagonalExists(3, 0, player))
                    return true;
                if (AscendingDiagonalExists(0, 0, player))
                    return true;
            }
            return false;
        }

        private bool DescendingDiagonalExists(int row, int column, string player)
        {
            if (row < 0 || column >= _columns - 1) return true;
            if (Moves.Any(m => m.Row == row && m.Column == column && m.Player == player))
                return DescendingDiagonalExists(row-1, column+1, player);
            return false;
        }

        private bool AscendingDiagonalExists(int row, int column, string player)
        {
            if (row >= _rows - 1 || column >= _columns - 1) return true;
            if (Moves.Any(m => m.Row == row && m.Column == column && m.Player == player))
                return AscendingDiagonalExists(row+1, column+1, player);
            return false;
        }

        private void VerifyColumnExists(int? column)
        {
            if (column == null || column < 0 || column > _columns - 1)
                throw new IllegalMoveException();
        }

        private int VerifyColumnHasRoom(int column)
        {
            var columnRows = Moves.Where(m => m.Column == column).Count();
            if (columnRows >= _rows)
                throw new IllegalMoveException();
            return columnRows;
        }

        private void VerifyPlayerPartOfGame(string player)
        {
            if (_player1 != player && _player2 != player)
                throw new PlayerNotFoundException();
        }

        private void VerifyItIsPlayerTurn(string player)
        {
            if ((player == _player1 && Moves.Count() % 2 != 0) ||
                (player == _player2 && Moves.Count() % 2 != 1))
            {
                throw new PlayerMovedOutOfTurnException();
            }
        }

        private void VerifyStartAndEndIndex(int? start, int? end)
        {
            if (start != null && (start < 0 || start >= Moves.Count()))
                throw new MoveNotFoundException();
            if (end != null && (end < 0 || end >= Moves.Count()))
                throw new MoveNotFoundException();
        }
    }
}