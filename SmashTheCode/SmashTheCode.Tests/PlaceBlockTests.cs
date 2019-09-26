using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SmashTheCode.Tests
{
    [TestClass]
    public class PlaceBlockTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var game = new GameState();
            for (int i = 0; i < 12; i++)
            {
                game.MyBlocks[i] = "......".ToCharArray().Select(x => x - '0').ToArray();
            }

            game.MyBlocks[04] = "2.....".ToCharArray().Select(x => x - '0').ToArray();
            game.MyBlocks[05] = "2.....".ToCharArray().Select(x => x - '0').ToArray();
            game.MyBlocks[06] = "4.....".ToCharArray().Select(x => x - '0').ToArray();
            game.MyBlocks[07] = "4.....".ToCharArray().Select(x => x - '0').ToArray();
            game.MyBlocks[08] = "14....".ToCharArray().Select(x => x - '0').ToArray();
            game.MyBlocks[09] = "14....".ToCharArray().Select(x => x - '0').ToArray();
            game.MyBlocks[10] = "41....".ToCharArray().Select(x => x - '0').ToArray();
            game.MyBlocks[11] = "41....".ToCharArray().Select(x => x - '0').ToArray();

            Player.D(game);

            Player.PlaceBlock(game, new[] { 1, 1 }, 2, 0);
            var d = Player.Compress(game);
            Player.D(d);
            Player.D(game);
        }

        [TestMethod]
        public void Test()
        {
            var game = new GameState();
            for (int i = 0; i < 12; i++)
            {
                game.MyBlocks[i] = "......".ToCharArray().Select(x => x - '0').ToArray();
            }

            var gg = new GameState(game);

            gg.MyBlocks[1][1] = 5;
            Player.D(game);
        }
    }
}
