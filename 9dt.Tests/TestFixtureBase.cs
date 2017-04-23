using System;
using _9dt.Controllers;
using _9dt.Games;
using _9dt.Models;
using NUnit.Framework;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;

namespace _9dt.Tests
{
    public abstract class TestFixtureBase
    {
        public DropTokenController _controller;
        private List<Game> _gamesRepo;

        /// <summary>
        /// Initial setup for tests
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _gamesRepo = GamesRepository.Games;
            _controller = new DropTokenController();

        }
        [TearDown]
        public void TestTearDown()
        {
            _gamesRepo.Clear();
        }

        protected void Then_an_error_is_thrown(Exception exception)
        {
            exception.Should().NotBeNull();

        }

        protected void And_the_error_indicates<T>(Exception exception)
        {
            exception.Should().BeOfType<T>();
        }

        [Obsolete]
        protected void AndTheWinnerIs(string gameId, string player)
        {
            var game = _gamesRepo.First(g => g.Id == gameId);
            game.SetWinner(player);
        }
        [Obsolete]
        protected Move GetLastMove(string gameId)
        {
            var game = _gamesRepo.First(g => g.Id == gameId);
            return game.GetLastMove();
        }

        protected string[] CreatePlayersArray(int numberOfPlayers)
        {
            var players = new string[numberOfPlayers];
            for (var i = 0; i < numberOfPlayers; i++)
                players[i] = "Thing_" + i;
            return players;
        }
    }
}
