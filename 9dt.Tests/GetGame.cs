using _9dt.Exceptions;
using _9dt.Models;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace _9dt.Tests
{
    /*
### GET /drop_token/{gameId} - Get the state of the game. ###
  * output:
```
{ "players" : ["player1", "player2"], # Initial list of players.
  "state": "DONE/IN_PROGRESS",
  "winner": "player1", # in case of draw, winner will be null, state will be DONE.
                       # in case game is still in progess, key should not exist.
}
```
  * #### Status codes ####
    * 200 - OK.On success
    //TODO: NOT POSSIBLE? * 400 - Malformed request
    * 404 - Game/moves not found.
*/

    public class GetGame : TestFixtureBase
    {
        private string _gameId;
        private GameStatus _response;
        private string[] _players;
        private string _winner;

        [Test]
        public void GetInProgressGameSucessfully()
        {
            Given_a_game();
            When_requesting_the_status_of_a_game();
            Then_the_players_should_be_correct();
            And_the_state_should_be(GameState.IN_PROGRESS);
            And_the_winner_should_be(null);
        }

        [Test]
        public void GetFinishedGameWithWinnerSuccessfully()
        {
            Given_a_game();
            And_the_game_has_a_winner();
            When_requesting_the_status_of_a_game();
            Then_the_players_should_be_correct();
            And_the_state_should_be(GameState.DONE);
            And_the_winner_should_be(_winner);
        }

        [Test]
        public void GetFinishedGameWithDrawSuccessfully()
        {
            Given_a_game();
            And_the_game_is_a_draw();
            When_requesting_the_status_of_a_game();
            Then_the_players_should_be_correct();
            And_the_state_should_be(GameState.DONE);
            And_the_winner_should_be(null);
        }

        [Test]
        public void UnknownIdErrors()
        {
            Given_a_request_for_an_unknown_id();
            try
            {
                When_requesting_the_status_of_a_game();
            }
            catch (Exception ex)
            {
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<GameNotFoundException>(ex);
            }
        }

        #region Methods
        private void Given_a_request_for_an_unknown_id()
        {
            _gameId = "0000";
        }

        private void Given_a_game()
        {
            _players = CreatePlayersArray(2);
            var createResponse = _controller.CreateGame(new NewGame { Players = _players, Rows = 4, Columns = 4 });
            _gameId = createResponse.Id;
        }

        private void And_the_game_has_a_winner()
        {
            _winner = _players[1];
            AndTheWinnerIs(_gameId, _winner);
        }
        private void And_the_game_is_a_draw()
        {
            AndTheWinnerIs(_gameId, null);
        }

        private void When_requesting_the_status_of_a_game()
        {
            _response = _controller.GetStatus(_gameId);
        }

        private void Then_the_players_should_be_correct()
        {
            _response.Should().NotBeNull();
            _response.Players[0].Should().Be(_players[0]);
            _response.Players[1].Should().Be(_players[1]);
        }

        private void And_the_state_should_be(GameState state)
        {
            _response.State.Should().Be(state);
        }

        private void And_the_winner_should_be(string winner)
        {
            _response.Winner.Should().Be(winner);
        }

        #endregion
    }
}
