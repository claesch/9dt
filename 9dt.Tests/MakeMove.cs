using _9dt.Exceptions;
using _9dt.Models;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using static _9dt.Models.Move;

namespace _9dt.Tests
{

    /*
 ### POST /drop_token/{gameId}/{playerId} - Post a move. ###
   * Input:
 ```
 {
  "column" : 2
 }
 ```
   * Output:
 ```
 {
   "move": "{gameId}/moves/{move_number}"
 }
 ```
   * #### Status codes ####
     * 200 - OK. On success
     * 400 - Malformed input. Illegal move
     * 404 - Game not found or player is not a part of it.
     * 409 - Player tried to post when it's not their turn.
*/

    public class MakeMove : TestFixtureBase
    {
        private string _gameId;
        private string[] _players;
        private MakeMoveResponse _response;

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void PlayerMovesSuccessfully(int columnNumber)
        {
            _players = CreatePlayersArray(2);
            Given_a_game_in_progress();
            When_requesting_to_make_a_move(_players[0], columnNumber);
            Then_the_move_number_is_returned(0);
            And_the_move_is_included_in_the_moves_list(_players[0], columnNumber, 0);
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void PlayerRequestsColumnThatDoesNotExist(int columnNumber)
        {
            bool exception = false;
            _players = CreatePlayersArray(2);
            Given_a_game_in_progress();
            try
            {
                When_requesting_to_make_a_move(_players[0], columnNumber);
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<IllegalMoveException>(ex);
                exception = true;
            }
            exception.Should().BeTrue();
        }

        [TestCase(0, "player1")]
        [TestCase(1, "player0")]
        [TestCase(2, "player1")]
        public void PlayerGoesOutOfTurn(int moveNumber, string outOfTurnPlayer)
        {
            bool exception = false;
            _players = new[] { "player0", "player1"};
            Given_a_game_in_progress();
            var currentMove = 0;
            while (currentMove < moveNumber)
            {
                When_requesting_to_make_a_move($"player{currentMove%2}", currentMove%4);
                currentMove++;
            }
            try
            {
                When_requesting_to_make_a_move(outOfTurnPlayer, 0);
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<PlayerMovedOutOfTurnException>(ex);
                exception = true;
            }
            exception.Should().BeTrue();
        }
        [Test]
        public void ColumnOutOfSpace()
        {
            bool exception = false;
            _players = CreatePlayersArray(2);
            Given_a_game_in_progress();
            And_a_column_is_full(0, 0);
            try
            {
                When_requesting_to_make_a_move(_players[0], 0);
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<IllegalMoveException>(ex);
                exception = true;
            }
            exception.Should().BeTrue();
        }

        private void And_a_column_is_full(int gamesMovesSoFar, int column)
        {
            var currentMove = gamesMovesSoFar;
            while (currentMove < 4)
            {
                When_requesting_to_make_a_move(_players[currentMove%2], 0);
                currentMove++;
            }
        }

        [Test]
        public void UnknownPlayerRequestsAMove()
        {
            bool exception = false;
            _players = CreatePlayersArray(2);
            Given_a_game_in_progress();
            try
            {
                When_requesting_to_make_a_move("unknown_player", 0);
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<PlayerNotFoundException>(ex);
                exception = true;
            }
            exception.Should().BeTrue();
        }

        [Test]
        public void RequestMoveForUnknownGame()
        {
            bool exception = false;
            _players = CreatePlayersArray(2);
            Given_an_unknown_game();
            try
            {
                When_requesting_to_make_a_move(_players[0], 0);
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<GameNotFoundException>(ex);
                exception = true;
            }
            exception.Should().BeTrue();
        }

        #region Methods
        private void Given_a_game_in_progress()
        {
            var createResponse = _controller.CreateGame(new NewGame { Players = _players, Rows = 4, Columns = 4 });
            _gameId = createResponse.Id;
        }

        private void Given_an_unknown_game()
        {
            _gameId = "XXXX";
        }

        private void When_requesting_to_make_a_move(string player, int column)
        {
            _response = _controller.RequestMove(_gameId, player, new Models.MakeMove { Column = column });
        }

        private void Then_the_move_number_is_returned(int expectedMoveNumber)
        {
            _response.Should().NotBeNull();
            _response.Move.Should().NotBeNull();
            _response.Move.Should().Be($"{_gameId}/moves/{expectedMoveNumber}");
        }

        private void And_the_move_is_included_in_the_moves_list(string player, int col, int expectedMoveNumber)
        {
            var moves = GetMoves(_gameId);
            var lastMove = moves.Last();
            lastMove.Should().NotBeNull();
            lastMove.Player.Should().Be(player);
            lastMove.Column.Should().Be(col);
            lastMove.Type.Should().Be(MoveType.MOVE);
            (moves.Count -1).Should().Be(expectedMoveNumber);
        }

        #endregion
    }
}
