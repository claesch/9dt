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

    public struct Move
    {
        public enum MoveType
        {
            QUIT,
            MOVE
        }

        public Move(MoveType type, string player, int? column = null)
        {
            Type = type;
            Column = column;
            Player = player;
        }
        public MoveType Type { get; }
        public int? Column { get; }
        public string Player { get; }
    }


    public class Game
    {
        private string _player1;
        private string _player2;
        private int _rows;
        private int _columns;
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

        internal void Quit(string quitterId)
        {
            VerifyPlayerPartOfGame(quitterId);

            if (_state == GameState.DONE)
                throw new MoveNotAllowedException();

            _winner = (_player1 == quitterId) ? _player2 : _player1;
            _state = GameState.DONE;

            AddMove(Move.MoveType.QUIT, quitterId);
        }

        internal int AddMove(Move.MoveType type, string player, int? column = null)
        {
            VerifyPlayerPartOfGame(player);

            if (type == Move.MoveType.MOVE)
            {
                if (column == null || column < 0 || column > 3)
                    throw new IllegalMoveException();
            }

            Moves.Add(new Move(type, player, column));
            return Moves.Count() - 1;
        }

        private void VerifyPlayerPartOfGame(string player)
        {
            if (_player1 != player && _player2 != player)
                throw new PlayerNotFoundException();
        }

        [Obsolete]
        public Move GetLastMove()
        {
            return Moves.Last();
        }
    }
}