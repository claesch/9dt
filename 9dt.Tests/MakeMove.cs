using _9dt.Exceptions;
using _9dt.Models;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;

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
        [TearDown]
        public void ResetSharedValues() {
            _gameId = null;
            _players = null;
            _response = null;
            _status = null;
        }

        private string _gameId;
        private string[] _players;
        private MakeMoveResponse _response;
        private GameStatus _status;

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

        [Test]
        public void GameEndsInDraw()
        {
            _players = new[] { "player0", "player1" };

            Given_a_game_in_progress();
            And_there_will_be_no_winner();
            When_the_last_move_is_made();
            Then_the_game_is_done();
            And_there_is_no_winner();
        }

        [TestCase(0, "player1")]
        [TestCase(1, "player0")]
        [TestCase(2, "player1")]
        [TestCase(3, "player0")]
        public void PlayerWinsWithFourInColumn(int column, string winner)
        {
            _players = new[] { "player0", "player1" };
            Given_a_game_in_progress();
            And_a_player_has_three_moves_in_succession_in_a_column(column, winner);
            When_the_player_places_the_fourth_move_in_the_same_column(column, winner);
            Then_the_game_is_done();
            And_the_player_is_the_winner(winner);
        }

        [TestCase(0, "player0")] 
        [TestCase(1, "player1")]
        [TestCase(3, "player1")]
        public void PlayerWinsWithFourInRow(int row, string winner)
        {
            _players = new[] { "player0", "player1" };
            Given_a_game_in_progress();
            And_a_player_has_three_moves_in_succession_in_a_row(row, winner);
            When_the_player_places_the_fourth_move_in_the_same_row(winner);
            Then_the_game_is_done();
            And_the_player_is_the_winner(winner);
        }
        [TestCase("Ascending")]
        [TestCase("Descending")]
        public void PlayerWinsOnADiagonal(string direction)
        {
            _players = new[] { "player0", "player1" };
            Given_a_game_in_progress();
            And_a_player_has_three_moves_in_succession_in_a_diagonal(direction);
            When_the_player_places_the_fourth_move_in_the_same_diagonal();
            Then_the_game_is_done();
            And_the_player_is_the_winner("player0");
        }

        [TestCase("Ascending")]
        [TestCase("Descending")]
        public void PlayerWinsOnADiagonalOnALargeBoard(string direction)
        {
            _players = new[] { "player0", "player1" };
            Given_a_game_in_progress(6, 6);
            And_a_player_has_three_moves_in_succession_in_a_diagonal(direction, true);
            When_the_player_places_the_fourth_move_in_the_same_diagonal(true);
            Then_the_game_is_done();
            And_the_player_is_the_winner("player0");
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
                And_the_player_can_make_another_move(_players[0], 3);
            }
            exception.Should().BeTrue();
        }

        [TestCase(0, "player1")]
        [TestCase(1, "player0")]
        [TestCase(2, "player1")]
        public void PlayerGoesOutOfTurn(int moveNumber, string outOfTurnPlayer)
        {
            bool exception = false;
            _players = new[] { "player0", "player1" };
            Given_a_game_in_progress();
            var currentMove = 0;
            while (currentMove < moveNumber)
            {
                When_requesting_to_make_a_move($"player{currentMove % 2}", currentMove % 4);
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
                And_the_player_can_make_another_move(_players[0], 1);
            }
            exception.Should().BeTrue();
        }

        private void And_a_column_is_full(int gamesMovesSoFar, int column)
        {
            var currentMove = gamesMovesSoFar;
            while (currentMove < 4)
            {
                When_requesting_to_make_a_move(_players[currentMove % 2], 0);
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
        private void Given_a_game_in_progress(int rows = 4, int cols = 4)
        {
            var createResponse = _controller.CreateGame(new NewGame { Players = _players, Rows = rows, Columns = cols });
            _gameId = createResponse.Id;
        }

        private void Given_an_unknown_game()
        {
            _gameId = "XXXX";
        }

        private void When_requesting_to_make_a_move(string player, int column)
        {
            _response = _controller.CreateMove(_gameId, player, new Models.MakeMove { Column = column });
        }

        private void And_the_player_can_make_another_move(string player, int column)
        {
            _response = _controller.CreateMove(_gameId, player, new Models.MakeMove { Column = column });
        }

        private void Then_the_move_number_is_returned(int expectedMoveNumber)
        {
            _response.Should().NotBeNull();
            _response.Move.Should().NotBeNull();
            _response.Move.Should().Be($"{_gameId}/moves/{expectedMoveNumber}");
        }

        private void And_the_move_is_included_in_the_moves_list(string player, int col, int expectedMoveNumber)
        {
            var moves = _controller.GetMoves(_gameId);
            var lastMove = moves.Last();
            lastMove.Should().NotBeNull();
            lastMove.Player.Should().Be(player);
            lastMove.Column.Should().Be(col);
            lastMove.Type.Should().Be(MoveType.MOVE);
            (moves.Count - 1).Should().Be(expectedMoveNumber);
        }


        private void And_there_will_be_no_winner()
        {
            //First row //1  2  2  1
            When_requesting_to_make_a_move(_players[0], 0);
            When_requesting_to_make_a_move(_players[1], 1);
            When_requesting_to_make_a_move(_players[0], 3);
            When_requesting_to_make_a_move(_players[1], 2);

            //Second row //2  1  1  2
            When_requesting_to_make_a_move(_players[0], 1);
            When_requesting_to_make_a_move(_players[1], 0);
            When_requesting_to_make_a_move(_players[0], 2);
            When_requesting_to_make_a_move(_players[1], 3);

            //Third row //1  2  2  1
            When_requesting_to_make_a_move(_players[0], 0);
            When_requesting_to_make_a_move(_players[1], 1);
            When_requesting_to_make_a_move(_players[0], 3);
            When_requesting_to_make_a_move(_players[1], 2);

            //Fourth row //2  1  1  2
            When_requesting_to_make_a_move(_players[0], 1);
            When_requesting_to_make_a_move(_players[1], 0);
            When_requesting_to_make_a_move(_players[0], 2);
        }
        private void And_a_player_has_three_moves_in_succession_in_a_diagonal(string direction, bool largeboard = false)
        {
            if (direction == "Ascending")
            {
                if (largeboard)
                {

                    When_requesting_to_make_a_move(_players[0], 1);
                    When_requesting_to_make_a_move(_players[1], 2);
                    When_requesting_to_make_a_move(_players[0], 3);
                    When_requesting_to_make_a_move(_players[1], 4);
                    When_requesting_to_make_a_move(_players[0], 1);
                    When_requesting_to_make_a_move(_players[1], 2);
                    When_requesting_to_make_a_move(_players[0], 2);
                    When_requesting_to_make_a_move(_players[1], 3);
                    When_requesting_to_make_a_move(_players[0], 3);
                    When_requesting_to_make_a_move(_players[1], 4);
                    When_requesting_to_make_a_move(_players[0], 3);
                    When_requesting_to_make_a_move(_players[1], 4);
                    When_requesting_to_make_a_move(_players[0], 4);
                    When_requesting_to_make_a_move(_players[1], 3);
                }
                else
                {
                    When_requesting_to_make_a_move(_players[0], 0);
                    When_requesting_to_make_a_move(_players[1], 1);
                    When_requesting_to_make_a_move(_players[0], 1);
                    When_requesting_to_make_a_move(_players[1], 2);
                    When_requesting_to_make_a_move(_players[0], 2);
                    When_requesting_to_make_a_move(_players[1], 3);
                    When_requesting_to_make_a_move(_players[0], 2);
                    When_requesting_to_make_a_move(_players[1], 3);
                    When_requesting_to_make_a_move(_players[0], 3);
                    When_requesting_to_make_a_move(_players[1], 2);
                }
            }
            else if (direction == "Descending")
            {
                if (largeboard)
                {
                    When_requesting_to_make_a_move(_players[0], 1);
                    When_requesting_to_make_a_move(_players[1], 2);
                    When_requesting_to_make_a_move(_players[0], 3);
                    When_requesting_to_make_a_move(_players[1], 4);
                    When_requesting_to_make_a_move(_players[0], 1);
                    When_requesting_to_make_a_move(_players[1], 1);
                    When_requesting_to_make_a_move(_players[0], 1);
                    When_requesting_to_make_a_move(_players[1], 2);
                    When_requesting_to_make_a_move(_players[0], 1);
                    When_requesting_to_make_a_move(_players[1], 2);
                    When_requesting_to_make_a_move(_players[0], 2);
                    When_requesting_to_make_a_move(_players[1], 3);
                    When_requesting_to_make_a_move(_players[0], 3);
                    When_requesting_to_make_a_move(_players[1], 3);
                }
                else
                {
                    When_requesting_to_make_a_move(_players[0], 0);
                    When_requesting_to_make_a_move(_players[1], 0);
                    When_requesting_to_make_a_move(_players[0], 0);
                    When_requesting_to_make_a_move(_players[1], 1);
                    When_requesting_to_make_a_move(_players[0], 0);
                    When_requesting_to_make_a_move(_players[1], 1);
                    When_requesting_to_make_a_move(_players[0], 1);
                    When_requesting_to_make_a_move(_players[1], 2);
                    When_requesting_to_make_a_move(_players[0], 2);
                    When_requesting_to_make_a_move(_players[1], 2);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void When_the_last_move_is_made()
        {
            When_requesting_to_make_a_move(_players[1], 3);
            _status = _controller.GetStatus(_gameId);
        }
        private void Then_the_game_is_done()
        {
            _status.State.Should().Be(GameState.DONE);
        }
        private void And_there_is_no_winner()
        {
            _status.Winner.Should().BeNull();
        }

        private void And_a_player_has_three_moves_in_succession_in_a_column(int winnerColumn, string winner)
        {
            var loser = _players.Except(new[] { winner }).Single();
            if (loser == _players[0]) //loser plays first
            {
                var playOffCol = winnerColumn < 2 ? winnerColumn + 2 : winnerColumn - 2;
                When_requesting_to_make_a_move(loser, playOffCol);
            }

            var loserColumn = winnerColumn < 3 ? winnerColumn + 1 : 0;
            When_requesting_to_make_a_move(winner, winnerColumn);
            When_requesting_to_make_a_move(loser, loserColumn);
            When_requesting_to_make_a_move(winner, winnerColumn);
            When_requesting_to_make_a_move(loser, loserColumn);
            When_requesting_to_make_a_move(winner, winnerColumn);
            When_requesting_to_make_a_move(loser, loserColumn);
        }
        private void And_a_player_has_three_moves_in_succession_in_a_row(int winnerRow, string winner)
        {
            var loser = _players.Except(new[] { winner }).Single();
            switch(winnerRow)
            {
                case 0: // winner has to be first player
                    When_requesting_to_make_a_move("player0", 0);
                    When_requesting_to_make_a_move("player1", 0);
                    When_requesting_to_make_a_move("player0", 1);
                    When_requesting_to_make_a_move("player1", 1);
                    When_requesting_to_make_a_move("player0", 2);
                    When_requesting_to_make_a_move("player1", 2);
                    break;
                case 1: // second player wins
                    When_requesting_to_make_a_move(loser, 0);
                    When_requesting_to_make_a_move(winner, 0);
                    When_requesting_to_make_a_move(loser, 1);
                    When_requesting_to_make_a_move(winner, 1);
                    When_requesting_to_make_a_move(loser, 2);
                    When_requesting_to_make_a_move(winner, 2);
                    When_requesting_to_make_a_move(loser, 0);
                    When_requesting_to_make_a_move(winner, 3);
                    When_requesting_to_make_a_move(loser, 1);
                    break;
                case 3: //winner has to be second player
                    When_requesting_to_make_a_move("player0", 0); // 0 0
                    When_requesting_to_make_a_move("player1", 0); // 0 1
                    When_requesting_to_make_a_move("player0", 0); // 0 2
                    When_requesting_to_make_a_move("player1", 0); // 0 3 ---- done
                    When_requesting_to_make_a_move("player0", 1); // 1 0
                    When_requesting_to_make_a_move("player1", 1); // 1 1
                    When_requesting_to_make_a_move("player0", 1); // 1 2
                    When_requesting_to_make_a_move("player1", 2); // 2 0
                    When_requesting_to_make_a_move("player0", 2); // 2 1
                    When_requesting_to_make_a_move("player1", 1); // 1 3 ---- done
                    When_requesting_to_make_a_move("player0", 3); // 3 0
                    When_requesting_to_make_a_move("player1", 2); // 2 2
                    When_requesting_to_make_a_move("player0", 3); // 3 1
                    When_requesting_to_make_a_move("player1", 2); // 2 3
                    When_requesting_to_make_a_move("player0", 3); // 3 2
                    break;
            }
        }
        private void When_the_player_places_the_fourth_move_in_the_same_column(int column, string winner)
        {
            When_requesting_to_make_a_move(winner, column);
            _status = _controller.GetStatus(_gameId);
        }

        private void When_the_player_places_the_fourth_move_in_the_same_row(string winner)
        {
            When_requesting_to_make_a_move(winner, 3);
            _status = _controller.GetStatus(_gameId);
        }

        private void When_the_player_places_the_fourth_move_in_the_same_diagonal(bool largeboard = false)
        {
            When_requesting_to_make_a_move("player0", largeboard ? 4 : 3);
            _status = _controller.GetStatus(_gameId);
        }

        private void And_the_player_is_the_winner(string player)
        {
            _status.Winner.Should().Be(player);
        }

        #endregion
    }
}
