using _9dt.Exceptions;
using _9dt.Models;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace _9dt.Tests
{
 /*
### GET /drop_token/{gameId}/moves/{move_number} - Return the move. ###
 * Output:
```
{
  "type" : "MOVE",
  "player": "player1",
  "column": 2
}
```
 * #### Status codes ####
    * 200 - OK. On success
    * 400 - Malformed request
    * 404 - Game/moves not found.
        */

    public class GetMove : TestFixtureBase
    {
        private string _gameId;
        private string[] _players;
        private List<dynamic> _moves;
        private MoveResponse _response;

        [TestCase(1, 0)]
        [TestCase(2, 1)]
        [TestCase(4, 3)]
        public void GetAMoveSucessfully(int numberOfMoves, int requestedMove)
        {
            Given_a_game();
            And_a_number_of_moves(numberOfMoves);
            When_requesting_move_number(requestedMove);
            Then_the_move_is_returned(requestedMove);
        }

        [TestCase(1, -1)]
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(3, 4)]
        public void MoveThatDoesNotExistIsRequested(int numberOfMoves, int requestedMove)
        {
            var error = false;
            Given_a_game();
            And_a_number_of_moves(numberOfMoves);
            try
            {
                When_requesting_move_number(requestedMove);
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<MoveNotFoundException>(ex);
                error = true;
            }
            error.Should().BeTrue();
        }

        [Test]
        public void RequestMoveForUnknownGame()
        {
            bool exception = false;
            _players = CreatePlayersArray(2);
            Given_an_unknown_game();
            try
            {
                When_requesting_move_number(0);
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
        private void Given_an_unknown_game()
        {
            _gameId = "0000";
        }

        private void Given_a_game()
        {
            _players = CreatePlayersArray(2);
            var createResponse = _controller.CreateGame(new NewGame { Players = _players, Rows = 4, Columns = 4 });
            _gameId = createResponse.Id;
        }

        private void And_a_number_of_moves(int moves)
        {
            var currentMove = 0;
            _moves = new List<dynamic>();
            while (currentMove < moves)
            {
                var player = _players[currentMove % 2];
                var column = currentMove % 4;
                var move = _controller.CreateMove(_gameId, player, new Models.MakeMove { Column = column });
                _moves.Add(new { Number = currentMove, Player= player, Column = column, Response = move });
                currentMove++;
            }
        }

        private void When_requesting_move_number(int move)
        {
            _response = _controller.GetMove(_gameId, move);
        }

        private void Then_the_move_is_returned(int move)
        {
            var moveRequest = _moves[move];
            _response.Should().NotBeNull();
            _response.Player.Should().Be(moveRequest.Player);
            _response.Column.Should().Be(moveRequest.Column);
            _response.Type.Should().Be(MoveType.MOVE);
        }
        #endregion
    }
}
