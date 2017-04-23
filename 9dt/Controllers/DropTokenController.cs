using _9dt.Games;
using _9dt.Models;
using System.Linq;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using System.Net;
using _9dt.Exceptions;
using System.Collections.Generic;

namespace _9dt.Controllers
{
    public class DropTokenController : ApiController
    {
        private List<Game> _games;

        public DropTokenController()
        {
            _games = GamesRepository.Games;
        }

        private Game GetGame(string gameId)
        {
            var game = _games.FirstOrDefault(g => g.Id == gameId);
            if (game == null)
                throw new GameNotFoundException();
            return game;
        }

        // GET: drop_token
        public GameIdList Get()
        {
            return new GameIdList { Games = _games.Select(g => g.Id).ToList() };
        }

        // GET: drop_token/598929-238428jfjklf...
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public GameStatus Get(string gameId)
        {
            var game = GetGame(gameId);
            var status = new GameStatus
            {
                Players = new[] { game.Player1, game.Player2 },
                State = game.State,
                Winner = game.Winner
            };
            return status;
        }

        // POST: drop_token
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public GameId Post(NewGame createGame)
        {
            if (createGame?.Players == null)
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            if (createGame.Players.Count() != 2)
                throw new NumberOfPlayersMustBeTwoException();
            if (createGame.Players[0] == createGame.Players[1])
                throw new PlayersCannotHaveSameNameException();
            if (createGame.Rows < 4 || createGame.Columns < 4)
                throw new RowsColumnsCannotBeLessThanFourException();

            var game = new Game(createGame.Players[0], createGame.Players[1]);
            _games.Add(game);
            return new GameId { Id = game.Id };
        }

        // DELETE: drop_token/37292-28372fa-234/player_1
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Gone)]
        [Route("drop_token/{gameId}/{playerId}")]
        public void Delete(string gameId, string playerId)
        {
            var game = GetGame(gameId);
            game.Quit(playerId);
        }
    }
}
