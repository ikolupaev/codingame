using System;
using System.IO;
using System.Text;

namespace LegendsOfCodeAndMagic
{
    public enum Locations
    {
        Hand = 0,
        Board = 1,
        Opponent = -1
    }

    public enum CardTypes
    {
        Creature = 0,
        Green = 1,
        Red = 2,
        Blue = 3
    }

    [Flags]
    public enum Abilities
    {
        None = 0,
        Guard = 1,
        Breakthrough = 2,
        Charge = 4,
        Drain = 8,
        Lethal = 16,
        Ward = 32
    }

    [Serializable]
    public class Card
    {
        public int Index;
        public int CardNumber;
        public int InstanceId;
        public Locations Location;
        public CardTypes CardType;
        public int Cost;
        public int Attack;
        public int Defense;
        public Abilities Abilities;
        public int OwnerHealthChange;
        public int OpponentHealthChange;
        public int CardDraw;

        public bool CanAttack;
        public bool HasAttacked;

        public bool Creature => CardType == CardTypes.Creature;
        public bool Green => CardType == CardTypes.Green;
        public bool Red => CardType == CardTypes.Red;
        public bool Blue => CardType == CardTypes.Blue;

        public Card()
        {
            CanAttack = true;
            HasAttacked = false;
        }

        public Card(Card c)
        {
            Index = c.Index;
            CardNumber = c.CardNumber;
            InstanceId = c.InstanceId;
            Location = c.Location;
            CardType = c.CardType;
            Cost = c.Cost;
            Attack = c.Attack;
            Defense = c.Defense;
            Abilities = c.Abilities;
            OwnerHealthChange = c.OwnerHealthChange;
            OpponentHealthChange = c.OpponentHealthChange;
            CardDraw = c.CardDraw;

            CanAttack = c.CanAttack;
            HasAttacked = c.HasAttacked;
        }

        public bool Has(Abilities a)
        {
            return (Abilities & a) == a;
        }

        public override string ToString()
        {
            return $"id:{InstanceId} a:{Attack} d:{Defense} {Location} {Abilities}";
        }

        internal void Set(Abilities a, bool v)
        {
            if (v)
                Abilities |= a;
            else
                Abilities &= ~a;
        }

        public void Save(Stream stream)
        {
            using (var bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                bw.Write(Index);
                bw.Write(CardNumber);
                bw.Write(InstanceId);
                bw.Write((int)Location);
                bw.Write((int)CardType);
                bw.Write(Cost);
                bw.Write(Attack);
                bw.Write(Defense);
                bw.Write((int)Abilities);
                bw.Write(OwnerHealthChange);
                bw.Write(OpponentHealthChange);
                bw.Write(CardDraw);
            }
        }

        public void Load(Stream stream)
        {
            using (var bw = new BinaryReader(stream, Encoding.ASCII, true))
            {
                Index = bw.ReadInt32();
                CardNumber = bw.ReadInt32();
                InstanceId = bw.ReadInt32();
                Location = (Locations)bw.ReadInt32();
                CardType = (CardTypes)bw.ReadInt32();
                Cost = bw.ReadInt32();
                Attack = bw.ReadInt32();
                Defense = bw.ReadInt32();
                Abilities = (Abilities)bw.ReadInt32();
                OwnerHealthChange = bw.ReadInt32();
                OpponentHealthChange = bw.ReadInt32();
                CardDraw = bw.ReadInt32();
            }
        }
    }
}