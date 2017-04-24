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
 

    Given a game in progress
        And a known player in the game

    And a request to make a move
    And it is the players turn
    And the column has open spaces
When requesting to make a move
Then the move is successful
    And the move number is returned
    And the move is included in the moves list

        //Request for invalid column

Given a game in progress
    And a unknown player in the game

    And a request to make a move
When requesting to make a move
Then an error is returned
    And the error indicates the player is unknown

Given a request for a game status with an unknown id
    And a request to make a move
When requesting to make a move
Then an error is returned
    And the error indicates the game is unknown

Given a game in progress
    And a malformed request to make a move
When requesting to make a move
Then an error is returned
    And the error indicates the request is malformed

Given a game in progress
    And a known player in the game

    And a request to make a move
    And it is not the players turn
When requesting to make a move
Then an error is returned
    And the error indicates it is not the players turn

Given a game in progress
    And a known player in the game

    And a malformed request to make a move

    And it is the players turn
    And the column does not have open spaces
When requesting to make a move
Then an error is returned
    And the error indicates the move is not allowed
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

        [Test]
        public void PlayerThatDoesNotExistRequestsAMove()
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

        #region Methods
        private void Given_a_game_in_progress()
        {
            var createResponse = _controller.CreateGame(new NewGame { Players = _players, Rows = 4, Columns = 4 });
            _gameId = createResponse.Id;
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
