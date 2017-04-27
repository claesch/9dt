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
        public List<BaseMove> Moves { get; }
        public string[,] Board { get; private set; }

        public Game(string player1, string player2, int rows = 4, int columns = 4)
        {
            Id = Guid.NewGuid().ToString();
            _player1 = player1;
            _player2 = player2;
            _rows = rows;
            _columns = columns;
            _state = GameState.IN_PROGRESS;
            Moves = new List<BaseMove>();
            Board = new string[columns, rows];
        }

        internal void Quit(string quitterId)
        {
            VerifyPlayerPartOfGame(quitterId);

            if (_state == GameState.DONE)
                throw new MoveNotAllowedException("The game is already complete and cannot be quit");

            AddMove(new QuitMove(quitterId));

            var winner = (_player1 == quitterId) ? _player2 : _player1;
            SetWinner(winner);
        }

        internal int AddMove(string player, int column)
        {
            VerifyColumnExists(column);
            var row = GetRowForMove(column);
            var move = new BoardMove(player, column, row);
            return AddMove(move);
        }

        private int AddMove(BaseMove move)
        {
            VerifyPlayerPartOfGame(move.Player);
            VerifyItIsPlayerTurn(move.Player);
            Moves.Add(move);

            if (move.GetType() == typeof(BoardMove))
            {
                var boardMove = (BoardMove)move;
                Board[boardMove.Column, boardMove.Row] = boardMove.Player;
                if (Moves.Count >= 7 && IsGameWon(boardMove))
                {
                    SetWinner(boardMove.Player);
                }
                else if (Moves.Count == _rows * _columns)
                {
                    SetDraw();
                }
            }

            return Moves.Count() - 1;
        }

        //TODO Check edge cases on this one
        internal BaseMove GetMove(int moveNumber)
        {
            if (moveNumber >= Moves.Count() || moveNumber < 0)
                throw new MoveNotFoundException();
            return Moves[moveNumber];
        }

        internal List<BaseMove> GetMoves(int? start, int? end)
        {
            VerifyStartAndEndIndex(start, end);

            if (Moves.Count() == 0)
            { return Moves; }

            int startIndex = start ?? 0;
            int endIndex = end ?? Moves.Count() - 1;
            return Moves.GetRange(startIndex, endIndex - startIndex + 1);
        }

        private void SetWinner(string player)
        {
            _winner = player;
            _state = GameState.DONE;
        }

        private void SetDraw()
        {
            _state = GameState.DONE;
        }

        private bool IsGameWon(BoardMove move)
        {
            var rowCount = 1;
            var colCount = 1;
            var increasingDiag = 1;
            var decreasingDiag = 1;

            //check row
            var currentCol = move.Column - 1;
            while (currentCol >= 0 && rowCount <= 4)
            {
                if (IsPlayerInSpot(move.Player, currentCol, move.Row))
                    rowCount++;
                else
                    break;
                currentCol--;
            }

            currentCol = move.Column + 1;
            while (currentCol < _columns && rowCount < 4)
            {
                if (IsPlayerInSpot(move.Player, currentCol, move.Row))
                    rowCount++;
                else break;
                currentCol++;
            }
            if (rowCount >= 4)
                return true;

            //Column check
            var currentRow = move.Row - 1;
            while (currentRow >= 0 && rowCount < 4)
            {
                if (IsPlayerInSpot(move.Player, move.Column, currentRow))
                    colCount++;
                else break;
                currentRow--;
            }
            currentRow = move.Row + 1;
            while (currentRow < _rows && rowCount < 4)
            {
                if (IsPlayerInSpot(move.Player, move.Column, currentRow))
                    colCount++;
                else break;
                currentRow++;
            }
            if (colCount >= 4)
                return true;

            //check increasing diagonal
            currentRow = move.Row - 1;
            currentCol = move.Column - 1;
            while (currentRow >= 0 && currentCol >=0 && increasingDiag < 4)
            {
                if (IsPlayerInSpot(move.Player, currentCol, currentRow))
                    increasingDiag++;
                else break;
                currentRow--;
                currentCol--;
            }

            currentRow = move.Row + 1;
            currentCol = move.Column + 1;

            while (currentRow < _rows && currentCol < _columns && increasingDiag < 4)
            {
                if (IsPlayerInSpot(move.Player, currentCol, currentRow))
                    increasingDiag++;
                else break;
                currentRow++;
                currentCol++;
            }
            if (increasingDiag >= 4)
                return true;

            //check decreasing diagonal
            currentRow = move.Row + 1;
            currentCol = move.Column - 1;
            while (currentRow < _rows && currentCol >= 0 && decreasingDiag < 4)
            {
                if (IsPlayerInSpot(move.Player, currentCol, currentRow))
                    decreasingDiag++;
                else break;
                currentRow++;
                currentCol--;
            }

            currentRow = move.Row - 1;
            currentCol = move.Column + 1;

            while (currentRow >= 0 && currentCol < _columns && decreasingDiag < 4)
            {
                if (IsPlayerInSpot(move.Player, currentCol, currentRow))
                    decreasingDiag++;
                else break;
                currentRow--;
                currentCol++;
            }
            if (decreasingDiag >= 4)
                return true;

            return false;
        }

        private bool IsPlayerInSpot(string player, int col, int row)
        {
            return Board[col, row] == player;
        }

        private void VerifyColumnExists(int? column)
        {
            if (column == null || column < 0 || column > _columns - 1)
                throw new IllegalMoveException($"Column requested does not exist in the game. Valid column are 0 to {_columns - 1}.");
        }

        private int GetRowForMove(int column)
        {
            for (var row = 0; row < _rows; row++)
                if (Board[column, row] == null)
                    return row;
            throw new IllegalMoveException("The requested column is full");
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