using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;

namespace LegendsOfCodeAndMagic
{
    public class RandomActionSolver
    {
        static Random rnd = new Random(777);

        public static IEnumerable<GameAction> GetBestActions(Game game)
        {
            IEnumerable<GameAction> bestActions = GameAction.PassActions;
            var bestScore = double.MinValue;
            var batchActions = new LinkedList<GameAction>();
            while (!CgTimer.IsTimeout())
            {
                var g = new Game(game);
                batchActions.Clear();

                while (!CgTimer.IsTimeout())
                {
                    CgTimer.Tick();

                    var actions = MovesEnumerator.GetAvailableActions(g, 0).ToArray();
                    if (actions.Length == 0) break;

                    var action = actions[rnd.Next(actions.Length)];
                    Simulator.ApplyMove(0, action, g);
                    batchActions.AddLast(action);
                }

                //var score = CalcScoreWithOpponentAction(g);
                var score = CalcScore(g);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestActions = batchActions.ToArray();
                }
            }
            return bestActions;
        }

        private static double CalcScoreWithOpponentAction(Game game)
        {
            var actions = MovesEnumerator.GetAvailableActions(game, 1);
            var bestScore = double.MinValue;
            foreach (var a in actions)
            {
                if (CgTimer.IsTimeout()) break;

                var g = new Game(game);
                Simulator.ApplyMove(1, a, g);
                var score = CalcScore(g);
                if( score > bestScore )
                {
                    bestScore = score;
                }
            }

            return bestScore;
        }

        static private double CalcScore(Game g)
        {
            if (g.Me.Health <= 0) return double.MinValue;
            if (g.Opponent.Health <= 0) return double.MaxValue;

            return
                g.Me.Health
                + g.GetMyBoardCards().Sum(x => x.Attack + x.Defense)
                + g.GetEnemyBoardCards().Sum(x => x.Attack + x.Defense) * -1
            //+ g.GetMyBoardCards().Count * 0.2
            //+ g.GetMyHandCards().Count * 0.2
            //+ g.GetEnemyBoardCards().Count * 0.1
            ;
        }
    }
}
