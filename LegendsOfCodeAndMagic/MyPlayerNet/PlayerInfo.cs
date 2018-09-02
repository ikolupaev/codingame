using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LegendsOfCodeAndMagic
{
    public class PlayerInfo
    {
        public int Index;
        public int Health;
        public int Mana;
        public int Deck;
        public int Rune;

        public List<int> Runes = new List<int>() { 5, 10, 15, 20, 25 };

        public int NextTurnDraw;

        public PlayerInfo()
        {
        }

        public PlayerInfo(PlayerInfo p)
        {
            this.Health = p.Health;
            this.Mana = p.Mana;
            this.Deck = p.Deck;
            this.Rune = p.Rune;
            this.NextTurnDraw = p.NextTurnDraw;
        }

        internal void ModifyHealth(int mod)
        {
            Health += mod;

            if (mod >= 0)
                return;

            for (int r = Runes.Count - 1; r >= 0; r--) // rune checking;
            {
                if (Health <= Runes[r])
                {
                    NextTurnDraw += 1;
                    Runes.RemoveAt(r);
                }
            }
        }

        public void Save(Stream stream)
        {
            using (var bw = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                bw.Write(Health);
                bw.Write(Mana);
                bw.Write(Deck);
                bw.Write(Rune);
                bw.Write(NextTurnDraw);
            }
        }

        public void Load(Stream stream)
        {
            using (var bw = new BinaryReader(stream, Encoding.ASCII, true))
            {
                Health = bw.ReadInt32();
                Mana = bw.ReadInt32();
                Deck = bw.ReadInt32();
                Rune = bw.ReadInt32();
                NextTurnDraw = bw.ReadInt32();
            }
        }
    }
}
