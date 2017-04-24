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
 ### DELETE /drop_token/{gameId}/{playerId} - Player quits from game. ###
  * #### Status codes ####
    * 202 - OK. On success
    * 404 - Game not found or player is not a part of it.
    * 410 - Game is already in DONE state.
 */

    public class PlayerQuits : TestFixtureBase
    {
        private string _gameId;
        private GameStatus _gameStatusResponse;
        private string[] _players;
        private object _quitResponse;

        [Test]
        public void PlayerQuitsGameSuccessfully()
        {
            _players = CreatePlayersArray(2);
            var quitter = _players[0];
            var winner = _players[1];

            Given_a_game_in_progress();
            When_a_player_requests_to_quit_the_game(quitter);
            Then_the_state_should_be(GameState.DONE);
            And_the_winner_should_be(winner);
            And_the_moves_indicate_the_player_has_quit(quitter);
        }

        [Test]
        public void PlayerTriesToQuitAlreadyFinishedGame()
        {
            _players = CreatePlayersArray(2);
            bool exception = false;
            var quitter = _players[0];
            var winner = _players[1];

            Given_a_game_that_is_done();
            try
            {
                When_a_player_requests_to_quit_the_game(quitter);
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<MoveNotAllowedException>(ex);
                exception = true;
            }
            if (!exception)
                throw new NotImplementedException();
        }


        [Test]
        public void UnknownGameIdErrors()
        {
            _players = CreatePlayersArray(2);
            bool exception = false;
            Given_a_request_with_an_unknown_game_id();
            try
            {
                When_a_player_requests_to_quit_the_game(_players[0]);
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<GameNotFoundException>(ex);
                exception = true;
            }
            if (!exception)
                throw new NotImplementedException();
        }

        [Test]
        public void UnknownPlayerErrors()
        {
            _players = CreatePlayersArray(2);
            bool exception = false;
            var player = "UnknownPlayer";

            Given_a_game_in_progress();

            try
            {
                When_a_player_requests_to_quit_the_game(player);
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<PlayerNotFoundException>(ex);
                exception = true;
            }
            if (!exception)
                throw new NotImplementedException();
        }

        #region Methods
        private void Given_a_request_with_an_unknown_game_id()
        {
            _gameId = "0000";
        }

        private void Given_a_game_in_progress()
        {
            var createResponse = _controller.CreateGame(new NewGame { Players = _players, Rows = 4, Columns = 4 });
            _gameId = createResponse.Id;
        }
        private void Given_a_game_that_is_done()
        {
            Given_a_game_in_progress();
            AndTheWinnerIs(_gameId, _players[0]);
        }

        private void When_a_player_requests_to_quit_the_game(string quitterId)
        {
            _controller.Quit(_gameId, quitterId);
        }

        private void Then_the_state_should_be(GameState state)
        {
            _gameStatusResponse = _controller.GetStatus(_gameId);
            _gameStatusResponse.State.Should().Be(state);
        }

        private void And_the_winner_should_be(string winner)
        {
            _gameStatusResponse.Winner.Should().Be(winner);
        }

        private void And_the_moves_indicate_the_player_has_quit(string quitter)
        {
            var lastMove = base.GetMoves(_gameId).Last();
            lastMove.Player.Should().Be(quitter);
            lastMove.Type.Should().Be(MoveType.QUIT);
        }

        #endregion
    }
}