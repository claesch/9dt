using _9dt.Exceptions;
using FluentAssertions;
using NUnit.Framework;
using System;
using _9dt.Models;

namespace _9dt.Tests
{

    /*
    ### POST /drop_token - Create a new game. ###
      * Input:
    ```
    { "players": ["player1", "player2"],
      "columns": 4,
      "rows": 4
    }
    ```
      * Output:
     ```
     { "gameId": "some_string_token"}
     ```
      * #### Status codes ####
        * 200 - OK. On success
        * 400 - Malformed request

    */
    public class CreateGame : TestFixtureBase
    {
        private NewGame _request;
        private GameId _createResponse;

        [Test]
        public void CreatingGameSucessfully()
        {
            Given_a_request_for_a_new_game(CreatePlayersArray(2), 4, 4);
            When_creating_a_new_game();
            Then_a_new_game_is_created();
            And_a_game_id_is_returned();
            
        }

        [Test]
        public void CreatingGameWithSameNamePlayersThrowsError()
        {
            bool exception = false;
            Given_a_request_for_a_new_game(new[] { "Player", "Player" }, 4, 4);
            try
            {
                When_creating_a_new_game();
            }
            catch (Exception ex)
            {
                exception = true;
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<PlayersCannotHaveSameNameException>(ex);
            }
            if (!exception)
                throw new Exception("Expected exception");
        }

        [TestCase(1)]
        [TestCase(3)]
        public void CreatingGameWithWrongNumberOfPlayersThrowsError(int numberOfPlayers)
        {
            bool exception = false;
            Given_a_request_for_a_new_game(CreatePlayersArray(numberOfPlayers), 4, 4);

            try
            {
                When_creating_a_new_game();
            }
            catch (Exception ex)
            {
                exception = true;
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<NumberOfPlayersMustBeTwoException>(ex);
            }
            if (!exception)
                throw new Exception("Expected exception");
        }

        [TestCase(0, 0)]
        [TestCase(3, 4)]
        [TestCase(4, 3)]
        public void CreatingGameWithLessThanFourRowsOrColumnsThrowsError(int numberOfRows, int numberOfColumns)
        {
            bool exception = false;
            Given_a_request_for_a_new_game(CreatePlayersArray(2), numberOfRows, numberOfColumns);
            try
            {
                When_creating_a_new_game();
            }
            catch (Exception ex)
            {
                exception = true;
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<RowsColumnsCannotBeLessThanFourException>(ex);
            }
            if (!exception)
                throw new Exception("Expected exception");
        }

        #region Methods
        private void Given_a_request_for_a_new_game(string[] players, int rows, int columns)
        {
            _request = new NewGame { Players = players, Rows = rows, Columns = columns };
        }

        private void When_creating_a_new_game()
        {
            _createResponse = _controller.CreateGame(_request);
        }

        private void Then_a_new_game_is_created()
        {
            _createResponse.Should().NotBeNull();
        }

        private void And_a_game_id_is_returned()
        {
            _createResponse.Id.Should().NotBeNullOrWhiteSpace();
            _createResponse.Id.Length.Should().Be(36);
            _createResponse.Id.Should().NotBe(new Guid().ToString());
        }

        #endregion
    }
}
