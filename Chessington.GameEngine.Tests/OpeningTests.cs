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
    }
}
