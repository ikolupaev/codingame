using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace LegendsOfCodeAndMagic
{
    public class CgPlayer
    {
        static Dictionary<int, int> cards;

        static void Main(string[] args)
        {
            cards = Enumerable.Range(0, 20).ToDictionary(x => x, x => 0);
            var game = new Game();
            // game loop
            while (true)
            {
                game.UpdatePlayers();
                game.UpdateCards();

                CgTimer.Reset(90);

                if (game.Draw)
                {
                    Pick(game);
                    continue;
                }

                var s = game.Serialize();
                //D(s);

                DoActions(game);
            }
        }

        private static void DoActions(Game game)
        {
            var actions = PermutateActionSolver.GetBestActions(game);

            foreach (var a in actions)
            {
                game.AddAction(a);
            }

            game.Flush();
        }

        private static void Pick(Game game)
        {
            var card = game.CardsByLocations.SelectMany(x => x).OrderByDescending(DrawRanker).First();
            cards[card.Cost]++;
            game.Take(card);
            //game.AddAction(GameAction.Pass());
            game.Flush();
        }

        private static double DrawRanker(Card c)
        {
            var rank =
                //cards[c.Cost] * -1;
                 c.Cost * -1
                 + c.Attack
                + (c.Creature ? 1 : 0)
                //+ (c.Has(Abilities.Guard) ? 5 : 0)
                //+ c.OwnerHealthChange
                //+ c.Index * 0.1
                //+ (((int)c.Abilities) > 0 ? 1 : 0)
                ;
            //(c.Has(Abilities.Guard) ? 1000 : 0) +
            //       (c.Has(Abilities.Breakthrough) ? 500 : 0) +
            //       (c.OpponentHealthChange > 0 ? -1000 : 0) +
            //       (c.Creature ? 20 : 0) +
            //       c.MyHealthChange * 100 +
            //       c.OpponentHealthChange * -90 +
            //       c.CardNumber;

            D("card rank:", c, rank.ToString(), c.Abilities);

            return rank;
        }

        public static void D(params object[] args)
        {
            Console.Error.WriteLine(string.Join(" ", args.Select(x => x.ToString())));
        }
    }
}