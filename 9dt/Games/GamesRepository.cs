using _9dt.Models;
using System.Collections.Generic;

namespace _9dt.Games
{
    public class GamesRepository
    {
        private static List<Game> _games;
        private GamesRepository() { }
        public static List<Game> Games {
            get
            {
                if (_games == null)
                {
                    _games = new List<Game>();
                }
                return _games;
            }
        }
        public static void Add(Game game)
        {
            _games.Add(game);
        }
    }
}