using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;

using Chessington.UI.Factories;
using Chessington.GameEngine.AI;

namespace Chessington.GameEngine.Tests
{
    [TestFixture]
    class OpeningTests
    {
        [Test]
        public void StartPositionHasCorrectHash()
        {
            var board = new Board();
            StartingPositionFactory.Setup(board);

            ulong hash = Polyglot.HashBoard(board);

            hash.Should().Be(0x463b96181691fc9c);
        }

        [Test]
        public void OpeningBookHasCorrectNumberOfEntries()
        {
            // total number of entries
            Polyglot.CountEntries().Should().Be(1030253);
        }

        [Test]
        public void OpeningBookHasCorrectNumberOfEntriesForStartPosition()
        {
            var board = new Board();
            StartingPositionFactory.Setup(board);

            // 9 initial moves are considered for white
            Polyglot.CountMoves(board).Should().Be(9);
        }
    }
}
