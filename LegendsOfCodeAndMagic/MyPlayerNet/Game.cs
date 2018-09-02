using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace LegendsOfCodeAndMagic
{
    public class Game
    {
        public PlayerInfo[] Players;
        public List<Card>[] CardsByLocations;

        public Game()
        {
            Players = new[] { new PlayerInfo { Index = 0 }, new PlayerInfo { Index = 1 } };
            CardsByLocations = new[] { new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
        }

        public Game(Game src)
        {
            Players = new[]
            {
                new PlayerInfo(src.Players[0]),
                new PlayerInfo(src.Players[1])
            };

            CardsByLocations = new[]
            {
                new List<Card>(src.CardsByLocations[0]),
                new List<Card>(src.CardsByLocations[1]),
                new List<Card>(src.CardsByLocations[2]),
                new List<Card>(src.CardsByLocations[2])
            };
        }

        public bool Draw => Players[0].Mana == 0;
        public PlayerInfo Me => Players[0];
        public PlayerInfo Opponent => Players[1];


        internal void AddAction(GameAction a)
        {
            turn += a.ToString() + ";";
        }

        public List<Card> GetMyBoardCards() => CardsByLocations[2];
        public List<Card> GetEnemyBoardCards() => CardsByLocations[0];

        public List<Card> GetMyHandCards() => CardsByLocations[1];
        public List<Card> GetEnemyHandCards() => CardsByLocations[3];

        public string Serialize()
        {
            using (var stream = new MemoryStream())
            {
                //using (var gz = new GZipStream(stream, CompressionMode.Compress, true))
                var gz = stream;
                {
                    Players[0].Save(gz);
                    Players[1].Save(gz);

                    var allCards = CardsByLocations.SelectMany(x => x).ToArray();
                    gz.WriteByte((byte)allCards.Length);
                    foreach (var c in allCards)
                    {
                        c.Save(gz);
                    }

                }

                return Convert.ToBase64String(stream.ToArray());
            }
        }

        public void Load(string base64)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(base64)))
            {
                //using (var gz = new GZipStream(stream, CompressionMode.Decompress, false))
                var gz = stream;
                {
                    Players[0].Load(gz);
                    Players[1].Load(gz);

                    var cardsNumber = gz.ReadByte();

                    ClearCards();

                    for (int i = 0; i < cardsNumber; i++)
                    {
                        var card = new Card();
                        card.Load(gz);
                        RouteCard(card);
                    }
                }
            }
        }

        internal List<Card> GetBoardCards(int playerIndex)
        {
            if (playerIndex == 0)
                return GetMyBoardCards();
            else
                return GetEnemyBoardCards();
        }

        internal List<Card> GetHandCards(int playerIndex)
        {
            if (playerIndex == 0)
                return GetMyHandCards();
            else
                return GetEnemyHandCards();
        }

        private void ClearCards()
        {
            CardsByLocations[0].Clear();
            CardsByLocations[1].Clear();
            CardsByLocations[2].Clear();
            CardsByLocations[3].Clear();
        }

        private void RouteCard(Card card)
        {
            CardsByLocations[((int)card.Location) + 1].Add(card);
        }

        public void UpdatePlayers()
        {
            for (int i = 0; i < 2; i++)
            {
                var inputs = ReadLine().Split(' ');

                Players[i].Health = int.Parse(inputs[0]);
                Players[i].Mana = int.Parse(inputs[1]);
                Players[i].Deck = int.Parse(inputs[2]);
                Players[i].Rune = int.Parse(inputs[3]);
            }
        }

        private static string ReadLine()
        {
            var s = Console.ReadLine();
            return s;
        }

        public void UpdateCards()
        {
            int opponentHandCardsCount = int.Parse(ReadLine());
            int cardsCount = int.Parse(ReadLine());

            ClearCards();
            for (int i = 0; i < cardsCount; i++)
            {
                var inputs = ReadLine().Split(' ');

                var card = new Card
                {
                    Index = i,
                    CardNumber = int.Parse(inputs[0]),
                    InstanceId = int.Parse(inputs[1]),
                    Location = (Locations)int.Parse(inputs[2]),
                    CardType = (CardTypes)int.Parse(inputs[3]),
                    Cost = int.Parse(inputs[4]),
                    Attack = int.Parse(inputs[5]),
                    Defense = int.Parse(inputs[6]),
                    Abilities = CreateAbilities(inputs[7]),
                    OwnerHealthChange = int.Parse(inputs[8]),
                    OpponentHealthChange = int.Parse(inputs[9]),
                    CardDraw = int.Parse(inputs[10]),
                    CanAttack = true,
                    HasAttacked = true
                };

                RouteCard(card);
            };
        }

        private Abilities CreateAbilities(string a)
        {

            return (a.Contains('G') ? Abilities.Guard : Abilities.None) |
                   (a.Contains('B') ? Abilities.Breakthrough : Abilities.None) |
                   (a.Contains('C') ? Abilities.Charge : Abilities.None) |
                   (a.Contains('D') ? Abilities.Drain : Abilities.None) |
                   (a.Contains('L') ? Abilities.Lethal : Abilities.None) |
                   (a.Contains('W') ? Abilities.Ward : Abilities.None);
        }

        string turn;

        internal void Take(Card card)
        {
            turn += $"PICK {card.Index}";
        }

        public void Flush()
        {
            if (string.IsNullOrEmpty(turn))
            {
                turn = "PASS";
            }

            Console.WriteLine(turn.Trim(';'));
            turn = "";
        }

        public override string ToString()
        {
            return $"me: {Me.Health}; op: {Opponent.Health}\n{DiagCards(CardsByLocations.SelectMany(x => x))}";
        }

        string DiagCards(IEnumerable<Card> cards)
        {
            return string.Join("\n", cards.Select(x => x.ToString()));
        }
    }
}