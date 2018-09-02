using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsOfCodeAndMagic
{
    public class PermutateActionSolver
    {
        static private void ResolveActions(int playerIndex, Game game, LinkedList<GameAction> moves, List<GameActionsBatch> allActions)
        {
            if (CgTimer.IsTimeout())
            {
                return;
            }

            CgTimer.Tick();

            game = new Game(game);

            if (moves != null)
            {
                Simulator.ApplyMove(playerIndex, moves.Last(), game);
            }
            else
            {
                moves = new LinkedList<GameAction>();
            }

            var availableMoves = MovesEnumerator.GetAvailableActions(game, playerIndex);

            if (!availableMoves.Any())
            {
                if (moves.Any())
                {
                    allActions.Add(new GameActionsBatch { Game = game, Actions = moves.ToArray(), Score = CalcScore(game) });
                }
            }
            else
            {
                foreach (var m in availableMoves)
                {
                    if (CgTimer.IsTimeout()) return;

                    moves.AddLast(m);
                    ResolveActions(playerIndex, game, moves, allActions);
                    moves.RemoveLast();
                }
            }
        }

        public static IEnumerable<GameAction> GetBestActions(Game game)
        {
            var allActions = new List<GameActionsBatch>();
            ResolveActions(0, game, null, allActions);

            CgPlayer.D($"actions: {allActions.Count}");

            allActions = allActions.OrderByDescending(x => x.Score).ToList();

            CgTimer.Log("my finished");

            IEnumerable<GameAction> actions = GameAction.PassActions;

            //if (allActions.Count > 0)
            //{
            //    var top = allActions.First();
            //    actions = top.Actions;
            //}

            foreach (var a in allActions)
            {
                if (CgTimer.IsTimeout()) break;
                var oppActions = new List<GameActionsBatch>();
                ResolveActions(1, a.Game, null, oppActions);
                var best = oppActions.OrderByDescending(x => x.Score).FirstOrDefault();
                if (best != null)
                {
                    a.Score1 = best.Score;
                    a.Actions1 = best.Actions;
                }
                else
                {
                    a.Score1 = double.MaxValue;
                    a.Actions1 = GameAction.PassActions;
                }
            }

            if (allActions.Count > 0)
            {
                var top = allActions.First();
                actions = top.Actions;
                CgPlayer.D($"top: {top.Score}/{top.Score1} {string.Join(";", actions)}");

                var top1 = allActions.Where(x => x.Score1 > double.MinValue).OrderByDescending(x => x.Score1).FirstOrDefault();
                if (top1 != null)
                {
                    CgPlayer.D($"top1: {top1?.Score}/{top1?.Score1} {string.Join(";", top1?.Actions)} / {string.Join(";", top1?.Actions1)}");
                    actions = top1.Actions;
                }
            }

            CgTimer.Log("op finished");

            return actions;
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
                if (score > bestScore)
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
