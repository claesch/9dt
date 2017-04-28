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
        private const int WinningMatches = 4;
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

        public Game(string player1, string player2, int rows, int columns)
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
                if (Moves.Count >= ((WinningMatches * 2) - 1) && IsGameWon(boardMove))
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
            if (FourInARow_Row(move))
                return true;

            if (FourInARow_Column(move))
                return true;

            if (FourInARow_IncreasingDiag(move))
                return true;
            
            if (FourInARow_DescreasingDiag(move))
                return true;

            return false;
        }

        private int GetRowForMove(int column)
        {
            for (var row = 0; row < _rows; row++)
                if (Board[column, row] == null)
                    return row;
            throw new IllegalMoveException("The requested column is full");
        }

        private bool IsPlayerInSpot(string player, int col, int row)
        {
            return Board[col, row] == player;
        }

        #region Validation
        private void VerifyColumnExists(int? column)
        {
            if (column == null || column < 0 || column > _columns - 1)
                throw new IllegalMoveException($"Column requested does not exist in the game. Valid columns are 0 to {_columns - 1}.");
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
        #endregion

        #region Winning Scenarios
        private bool FourInARow_Row(BoardMove move)
        {
            int matchingPlayCount = 0;
            var startingColumn = move.Column - (WinningMatches - 1) < 0 ? 0 : move.Column - (WinningMatches - 1);
            int endingColumn = move.Column + (WinningMatches - 1) >= _columns ? _columns - 1 : move.Column + (WinningMatches - 1);

            for (var i = startingColumn; i <= endingColumn && matchingPlayCount < WinningMatches; i++)
            {
                if (IsPlayerInSpot(move.Player, i, move.Row))
                    matchingPlayCount++;
                else
                    matchingPlayCount = 0;
            }

            return matchingPlayCount >= WinningMatches;
        }

        private bool FourInARow_Column(BoardMove move)
        {
            int matchingPlayCount = 0;
            var startingRow = move.Row - (WinningMatches - 1) < 0 ? 0 : move.Row - (WinningMatches - 1);
            int endingRow = move.Row;

            for (var i = startingRow; i <= endingRow && matchingPlayCount < WinningMatches; i++)
            {
                if (IsPlayerInSpot(move.Player, move.Column, i))
                    matchingPlayCount++;
                else
                    matchingPlayCount = 0;
            }

            return matchingPlayCount >= WinningMatches;
        }

        private bool FourInARow_IncreasingDiag(BoardMove move)
        {
            int matchingPlayCount = 1;
            var currentRow = move.Row - 1;
            var currentCol = move.Column - 1;
            while (currentRow >= 0 && currentCol >= 0 && matchingPlayCount < WinningMatches)
            {
                if (IsPlayerInSpot(move.Player, currentCol, currentRow))
                    matchingPlayCount++;
                else break;
                currentRow--;
                currentCol--;
            }

            currentRow = move.Row + 1;
            currentCol = move.Column + 1;

            while (currentRow < _rows && currentCol < _columns && matchingPlayCount < WinningMatches)
            {
                if (IsPlayerInSpot(move.Player, currentCol, currentRow))
                    matchingPlayCount++;
                else break;
                currentRow++;
                currentCol++;
            }
            return matchingPlayCount >= WinningMatches;
        }

        private bool FourInARow_DescreasingDiag(BoardMove move)
        {
            int matchingPlayCount = 1;
            var currentRow = move.Row + 1;
            var currentCol = move.Column - 1;
            while (currentRow < _rows && currentCol >= 0 && matchingPlayCount < WinningMatches)
            {
                if (IsPlayerInSpot(move.Player, currentCol, currentRow))
                    matchingPlayCount++;
                else break;
                currentRow++;
                currentCol--;
            }

            currentRow = move.Row - 1;
            currentCol = move.Column + 1;

            while (currentRow >= 0 && currentCol < _columns && matchingPlayCount < WinningMatches)
            {
                if (IsPlayerInSpot(move.Player, currentCol, currentRow))
                    matchingPlayCount++;
                else break;
                currentRow--;
                currentCol++;
            }
            return matchingPlayCount >= WinningMatches;
        }
        #endregion
    }
}