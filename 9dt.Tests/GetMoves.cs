using _9dt.Exceptions;
using _9dt.Models;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace _9dt.Tests
{
    /*
   ### GET /drop_token/{gameId}/moves- Get (sub) list of the moves played. ###
   Optional Query parameters: **GET /drop_token/{gameId}/moves?start=0&until=1**.
     * Output:
   ```
   {
     "moves": [{"type": "MOVE", "player": "player1", "column":1}, {"type": "QUIT", "player": "player2"}]
   }
   ```
     * #### Status codes ####
       * 200 - OK. On success
       * 400 - Malformed request
       * 404 - Game/moves not found.
*/

    public class GetMoves : TestFixtureBase
    {
        private string _gameId;
        private string[] _players;
        private List<dynamic> _moves;
        private List<MoveResponse> _response;

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void GetAllMovesSucessfully(int numberOfMoves)
        {
            Given_a_game();
            And_a_number_of_moves(numberOfMoves);
            When_requesting_all_moves();
            Then_the_list_of_moves_is_returned();
        }

        [TestCase(1, 0, 0)] //all moves when only 1
        [TestCase(3, 0, 1)] //first two
        [TestCase(3, 0, 2)] //all moves when multiple
        [TestCase(3, 1, 2)] //last two
        [TestCase(3, 1, null)] // null end
        [TestCase(3, null, 1)] // null start
        [TestCase(8, 2, 4)] //middle subset of multiple
        public void GetSubsetOfMovesSucessfully(int numberOfMoves, int? startIndex, int? endIndex)
        {
            Given_a_game();
            And_a_number_of_moves(numberOfMoves);
            When_requesting_a_subset_of_moves(startIndex, endIndex);
            Then_the_subset_of_moves_is_returned(startIndex, endIndex);
        }


        [TestCase(1, -1, 0)]
        [TestCase(0, 0, 0)]
        [TestCase(1, 0, 1)]
        [TestCase(1, -2, -1)]
        [TestCase(3, 2, 4)]
        public void MovesThatDoesNotExistIsRequested(int numberOfMoves, int? startIndex, int? endIndex)
        {
            var error = false;
            Given_a_game();
            And_a_number_of_moves(numberOfMoves);
            try
            {
                When_requesting_a_subset_of_moves(startIndex, endIndex);
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
        public void MovesWithAStartIndexHigherThanEndThrowsError()
        {
            var error = false;
            Given_a_game();
            And_a_number_of_moves(3);
            try
            {
                When_requesting_a_subset_of_moves(2, 0);
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<StartAndEndIndexMismatchException>(ex);
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
                When_requesting_all_moves();
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

        private void When_requesting_all_moves()
        {
            _response = _controller.GetMoves(_gameId);
        }

        private void When_requesting_a_subset_of_moves(int? startIndex, int? endIndex)
        {
            _response = _controller.GetMoves(_gameId, startIndex, endIndex);
        }

        private void Then_the_list_of_moves_is_returned()
        {
            for (var i = 0; i < _response.Count; i++)
            {
                _response[i].Should().NotBeNull();
                _response[i].Player.Should().Be(_moves[i].Player);
                _response[i].Column.Should().Be(_moves[i].Column);
                _response[i].Type.Should().Be(MoveType.MOVE);
            }
        }

        private void Then_the_subset_of_moves_is_returned(int? start, int? end)
        {
            _response.Should().NotBeNull();
            if (_moves.Count == 0)
            {
                _response.Count.Should().Be(0);
                return;
            }

            int expectedStart = start ?? 0;
            int expectedEnd = end ?? _moves.Count - 1;
            

            var movesSubset = _moves.GetRange(expectedStart, (expectedEnd - expectedStart + 1));

            for (var i = 0; i < _response.Count; i++)
            {
                _response[i].Should().NotBeNull();
                _response[i].Player.Should().Be(movesSubset[i].Player);
                _response[i].Column.Should().Be(movesSubset[i].Column);
                _response[i].Type.Should().Be(MoveType.MOVE);
            }
        }
        #endregion
    }
}
