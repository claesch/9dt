using _9dt.Models;
using FluentAssertions;
using NUnit.Framework;

namespace _9dt.Tests
{

    /*
### GET /drop_token - Return all in-progress games. ###
  * Output
```
 { "games" : ["gameid1", "gameid2"] }
```
  *  #### Status codes ####
    * 200 - OK. On success
*/

    public class GetDropTokens : TestFixtureBase
    {
        private GameIdList _response;
        private int _numberOfGames;

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void GetGames(int numberOfGames)
        {
            _numberOfGames = numberOfGames;

            Given_a_number_of_games_in_progress(_numberOfGames);
            When_requesting_games();
            Then_all_games_are_returned();
        }

        private void Then_all_games_are_returned()
        {
            _response.Should().NotBeNull();
            _response.Games.Should().NotBeNull();
            _response.Games.Count.ShouldBeEquivalentTo(_numberOfGames);
        }

        private void When_requesting_games()
        {
            _response = (_controller.Get());
        }

        private void Given_a_number_of_games_in_progress(int numberOfGames)
        {
            for (var i = 0; i < numberOfGames; i++)
                _controller.Post(new NewGame { Players = CreatePlayersArray(2), Rows = 4, Columns = 4});
        }
    }
}
