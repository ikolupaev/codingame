using System;

namespace LegendsOfCodeAndMagic
{
    public class Simulator
    {
        public class ActionResult
        {
            public bool IsValid;
            public Card Attacker;
            public Card Defender;
            public int AttackerHealthChange;
            public int DefenderHealthChange;
            public int AttackerAttackChange;
            public int DefenderAttackChange;
            public int AttackerDefenseChange;
            public int DefenderDefenseChange;
            public bool AttackerDied;
            public bool DefenderDied;

            public ActionResult(bool isValid)
            {
                this.IsValid = isValid;
                this.Attacker = null;
                this.Defender = null;
                this.AttackerHealthChange = 0;
                this.DefenderHealthChange = 0;
            }

            public ActionResult(Card attacker, Card defender, bool attackerDied, bool defenderDied, int attackerHealthChange, int defenderHealthChange)
            {
                this.IsValid = true;
                this.AttackerDied = attackerDied;
                this.DefenderDied = defenderDied;
                this.Attacker = attacker;
                this.Defender = defender;
                this.AttackerHealthChange = attackerHealthChange;
                this.DefenderHealthChange = defenderHealthChange;
            }

            public ActionResult(Card attacker, Card defender, int healthGain, int healthTaken)
                : this(attacker, defender, false, false, healthGain, healthTaken)
            {
            }
        }

        internal static void ApplyMove(int playerIndex, GameAction action, Game game)
        {
            switch (action.Action)
            {
                case GameActions.Summon:
                    ApplySummon(playerIndex, action, game);
                    break;
                case GameActions.Attack:
                    ApplyAttack(playerIndex, action, game);
                    break;
                case GameActions.Use:
                    ApplyUse(playerIndex, action, game);
                    break;
            }
        }

        private static void ApplySummon(int playerIndex, GameAction action, Game g)
        {
            g.GetHandCards(playerIndex).Remove(action.Card);
            g.Players[playerIndex].Mana -= action.Card.Cost;

            action.Card.Location = Locations.Board;
            action.Card.CanAttack = action.Card.Has(Abilities.Charge);
            g.GetBoardCards(playerIndex).Add(action.Card);

            g.Players[playerIndex].ModifyHealth(action.Card.OwnerHealthChange);
            g.Players[1 - playerIndex].ModifyHealth(action.Card.OpponentHealthChange);
            g.Players[playerIndex].NextTurnDraw += action.Card.CardDraw;
        }

        private static void ApplyUse(int playerIndex, GameAction action, Game g)
        {
            g.GetHandCards(playerIndex).Remove(action.Card);
            g.Players[playerIndex].Mana -= action.Card.Cost;

            if (action.Card.Green) // here we assume that green cards never remove friendly creatures!
            {
                var result = ResolveUse(action.Card, action.Target);

                g.GetBoardCards(playerIndex).Remove(action.Target);
                g.GetBoardCards(playerIndex).Add(result.Defender);

                g.Players[playerIndex].ModifyHealth(result.AttackerHealthChange);
                g.Players[1 - playerIndex].ModifyHealth(result.DefenderHealthChange);
                g.Players[playerIndex].NextTurnDraw += action.Card.CardDraw;
            }
            else // red and blue cards
            {
                ActionResult result = null;

                if (action.Target == null) // using on player
                {
                    result = ResolveUse(action.Card);
                }
                else // using on creature
                {
                    result = ResolveUse(action.Card, action.Target);

                    var p = playerIndex;
                    if (action.Target.Location == Locations.Opponent)
                        p = 1 - playerIndex;

                    g.GetBoardCards(p).Remove(action.Target);

                    if (!result.DefenderDied)
                        g.GetBoardCards(p).Add(result.Defender);
                }

                g.Players[playerIndex].ModifyHealth(result.AttackerHealthChange);
                g.Players[1 - playerIndex].ModifyHealth(result.DefenderHealthChange);
                g.Players[playerIndex].NextTurnDraw += action.Card.CardDraw;
            }
        }

        private static void ApplyAttack(int playerIndex, GameAction action, Game g)
        {
            ActionResult result;
            if (action.Target == null) // attacking player
            {
                result = ResolveAttack(action.Card);
            }
            else
            {
                result = ResolveAttack(action.Card, action.Target);
                g.GetBoardCards(1 - playerIndex).Remove(action.Target);
                if (!result.DefenderDied)
                {
                    g.GetBoardCards(1 - playerIndex).Add(result.Defender);
                }
            }

            g.GetBoardCards(playerIndex).Remove(action.Card);
            if (!result.AttackerDied)
            {
                g.GetBoardCards(playerIndex).Add(result.Attacker);
            }

            g.Players[playerIndex].ModifyHealth(result.AttackerHealthChange);
            g.Players[1 - playerIndex].ModifyHealth(result.DefenderHealthChange);
        }

        static ActionResult ResolveUse(Card card, Card target)
        {
            var targetAfter = new Card(target);

            if (card.Green) // add keywords
            {
                targetAfter.Abilities |= card.Abilities;

                if ((card.Abilities & Abilities.Charge) == Abilities.Charge)
                    targetAfter.CanAttack = !targetAfter.HasAttacked; // No Swift Strike hack
            }
            else // Assumming ITEM_BLUE or ITEM_RED - remove keywords
            {
                targetAfter.Abilities &= ~card.Abilities;
            }

            targetAfter.Attack = Math.Max(0, target.Attack + card.Attack);

            if (targetAfter.Has(Abilities.Ward) && card.Defense < 0)
                targetAfter.Abilities &= ~Abilities.Ward;
            else
                targetAfter.Defense += card.Defense;

            if (targetAfter.Defense <= 0) targetAfter = null;

            int itemgiverHealthChange = card.OwnerHealthChange;
            int targetHealthChange = card.OpponentHealthChange;

            var result = new ActionResult(card, targetAfter == null ? target : targetAfter, false, targetAfter == null, itemgiverHealthChange, targetHealthChange);
            result.DefenderAttackChange = card.Attack;
            result.DefenderDefenseChange = card.Defense;
            return result;
        }

        static ActionResult ResolveUse(Card item)
        {
            int itemgiverHealthChange = item.OwnerHealthChange;
            int targetHealthChange = item.Defense + item.OpponentHealthChange;

            return new ActionResult(null, null, itemgiverHealthChange, targetHealthChange);
        }

        static ActionResult ResolveAttack(Card attacker)
        {
            var attackerAfter = new Card(attacker)
            {
                CanAttack = false,
                HasAttacked = true
            };

            int healthGain = attacker.Has(Abilities.Drain) ? attacker.Attack : 0;
            int healthTaken = -attacker.Attack;

            var result = new ActionResult(attackerAfter, null, healthGain, healthTaken);
            result.DefenderDefenseChange = healthTaken;
            return result;
        }

        // when creature attacks creatures // run it ONLY on legal actions
        public static ActionResult ResolveAttack(Card attacker, Card defender)
        {
            if (!attacker.CanAttack)
                return new ActionResult(false);

            var attackerAfter = new Card(attacker)
            {
                CanAttack = false,
                HasAttacked = true
            };

            var defenderAfter = new Card(defender);

            if (defender.Has(Abilities.Ward))
                defenderAfter.Set(Abilities.Ward, attacker.Attack == 0);

            if (attacker.Has(Abilities.Ward))
                attackerAfter.Set(Abilities.Ward, defender.Attack == 0);

            int damageGiven = defender.Has(Abilities.Ward) ? 0 : attacker.Attack;
            int damageTaken = attacker.Has(Abilities.Ward) ? 0 : defender.Attack;
            int healthGain = 0;
            int healthTaken = 0;

            // attacking
            if (damageGiven >= defender.Defense) defenderAfter = null;
            if (attacker.Has(Abilities.Breakthrough) && defenderAfter == null) healthTaken = defender.Defense - damageGiven;
            if (attacker.Has(Abilities.Lethal) && damageGiven > 0) defenderAfter = null;
            if (attacker.Has(Abilities.Drain) && damageGiven > 0) healthGain = attacker.Attack;
            if (defenderAfter != null) defenderAfter.Defense -= damageGiven;

            // defending
            if (damageTaken >= attacker.Defense) attackerAfter = null;
            if (defender.Has(Abilities.Lethal) && damageTaken > 0) attackerAfter = null;
            if (attackerAfter != null) attackerAfter.Defense -= damageTaken;
            var result = new ActionResult(attackerAfter == null ? attacker : attackerAfter, defenderAfter == null ? defender : defenderAfter, attackerAfter == null, defenderAfter == null, healthGain, healthTaken);
            result.AttackerDefenseChange = -damageTaken;
            result.DefenderDefenseChange = -damageGiven;
            return result;
        }
    }
}