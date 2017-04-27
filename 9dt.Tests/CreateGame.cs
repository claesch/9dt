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
        private string[] _players;
        private string _gameId;
        int _playCount;

        [TestCase("player1", "player2", 4 , 4)]
        [TestCase("player 1", "player 2", 4, 4)]
        [TestCase("player1", "player2", 5, 5)]
        [TestCase("player1", "player2", 9, 7)]
        [TestCase("player1", "player2", 32, 45)]
        public void CreatingGameSucessfully(string player1, string player2, int columns, int rows)
        {
            _playCount = 0;
            _players = new[] { player1, player2 };
            Given_a_request_for_a_new_game(_players, rows, columns);
            When_creating_a_new_game();
            Then_a_new_game_is_created();
            And_a_game_id_is_returned();
            And_a_play_can_be_made_in_every_column(columns);
            And_a_play_can_be_made_in_every_row(rows);
        }

        [TestCase("player", "player")]
        [TestCase(null, "player")]
        [TestCase("player", null)]
        [TestCase("player", "")]
        public void CreatingGameWithSameNamePlayersThrowsError(string player1, string player2)
        {
            bool exception = false;
            Given_a_request_for_a_new_game(new[] { player1, player2 }, 4, 4);
            try
            {
                When_creating_a_new_game();
            }
            catch (Exception ex)
            {
                exception = true;
                Then_an_error_is_thrown(ex);
                And_the_error_indicates<PlayerNameException>(ex);
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
            _gameId = _createResponse.Id;
            _createResponse.Id.Should().NotBeNullOrWhiteSpace();
            _createResponse.Id.Length.Should().Be(36);
            _createResponse.Id.Should().NotBe(new Guid().ToString());
        }

        private void And_a_play_can_be_made_in_every_column(int totalColumns)
        {
            for (var i = 0; i < totalColumns; i++)
            {
                _controller.CreateMove(_gameId, _players[_playCount % 2], new Models.MakeMove { Column = i });
                _playCount++;
            }
        }
        private void And_a_play_can_be_made_in_every_row(int totalRows)
        {
            for (var i = 1; i < totalRows; i++)
            {
                _controller.CreateMove(_gameId, _players[_playCount % 2], new Models.MakeMove { Column = 0 });
                _playCount++;
            }
        }
        #endregion
    }
}
