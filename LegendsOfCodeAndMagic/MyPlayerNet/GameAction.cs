using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsOfCodeAndMagic
{
    public enum GameActions
    {
        Summon,
        Attack,
        Use,
        Pass,
        Pick
    }

    public class GameActionsBatch
    {
        public IEnumerable<GameAction> Actions;
        public IEnumerable<GameAction> Actions1;
        public Game Game;
        public double Score;
        public double Score1 = Double.MinValue;

        public override string ToString()
        {
            return Game.ToString() + "; "
                + string.Join("; ", Actions.Select(x => x.ToString()));
        }
    }

    public class GameAction
    {
        public Card Card;
        public Card Target;
        public GameActions Action;

        public static GameAction[] PassActions = new[] { new GameAction { Card = null, Target = null, Action = GameActions.Pass } };

        internal static GameAction Attack(Card c, Card t)
        {
            return new GameAction { Card = c, Target = t, Action = GameActions.Attack };
        }

        internal static GameAction AttackOpponent(Card c)
        {
            return new GameAction { Card = c, Target = null, Action = GameActions.Attack };
        }

        internal static GameAction Summon(Card c)
        {
            return new GameAction { Card = c, Action = GameActions.Summon };
        }

        internal static GameAction Use(Card c, Card t)
        {
            return new GameAction { Card = c, Target = t, Action = GameActions.Use };
        }

        internal static GameAction UseOnOpponent(Card c)
        {
            return new GameAction { Card = c, Target = null, Action = GameActions.Use };
        }

        internal static GameAction Pass()
        {
            return PassActions[0];
        }

        internal static GameAction Pick(Card c)
        {
            return new GameAction { Card = c, Target = null, Action = GameActions.Pass };
        }

        public override string ToString()
        {
            switch (Action)
            {
                case GameActions.Summon:
                    return $"SUMMON {Card.InstanceId}";
                case GameActions.Attack:
                    return $"ATTACK {Card.InstanceId} {GetTargetId()}";
                case GameActions.Use:
                    return $"USE {Card.InstanceId} {GetTargetId()}";
                case GameActions.Pick:
                    return $"PICK {Card.Index}";
                case GameActions.Pass:
                    return "PASS";
            }

            throw new ArgumentException();
        }

        private int GetTargetId()
        {
            if (Target == null) return -1;
            else return Target.InstanceId;
        }
    }
}
