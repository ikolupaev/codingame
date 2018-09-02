using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsOfCodeAndMagic
{
    public class MovesEnumerator
    {
        public static IEnumerable<GameAction> GetAvailableActions(Game game, int playerIndex)
        {
            if (game.Me.Health <= 0 || game.Opponent.Health <= 0)
                yield break;

            if (playerIndex == 0)
            {
                foreach (var a in GetMyHandCardActions(game))
                {
                    yield return a;
                }
            }

            foreach (var c in game.GetBoardCards(playerIndex).Where(x => x.CanAttack))
            {
                foreach (var a in GetCardAttacks(c, game, game.GetBoardCards(1-playerIndex)))
                {
                    yield return a;
                }
            }
        }

        static IEnumerable<GameAction> GetMyHandCardActions(Game game)
        {
            foreach (var c in game.GetMyHandCards())
            {
                if (c.Cost > game.Me.Mana) continue;

                if (c.Creature && game.GetMyBoardCards().Count < 6)
                {
                    yield return GameAction.Summon(c);
                }

                if (c.Green)
                {
                    foreach (var t in game.GetMyBoardCards())
                    {
                        yield return GameAction.Use(c, t);
                    }
                }

                if (c.Red || (c.Blue && c.Defense < 0))
                {
                    foreach (var t in game.GetEnemyBoardCards())
                    {
                        yield return GameAction.Use(c, t);
                    }
                }

                if (c.Blue)
                {
                    yield return GameAction.UseOnOpponent(c);
                }
            }
        }

        static IEnumerable<GameAction> GetCardAttacks(Card card, Game state, IEnumerable<Card> enemyCards)
        {
            var guards = false;
            var guardCards = enemyCards.Where(x => x.Has(Abilities.Guard))
                .OrderByDescending(x => card.Attack - x.Defense);

            foreach (var c in guardCards)
            {
                guards = true;
                yield return GameAction.Attack(card, c);
            }

            if (guards) yield break;

            yield return GameAction.AttackOpponent(card);

            var enemiesToAttack = enemyCards
                .OrderByDescending(x => card.Attack > x.Defense ? 1 : 0)
                .ThenBy(x => card.Attack - x.Defense);

            foreach (var c in enemiesToAttack)
            {
                yield return GameAction.Attack(card, c);
            }
        }
    }
}
