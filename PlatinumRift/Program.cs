using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

class Zone
{
    public int Id;
    public int Platinum;
    public int OwnerId;
    public int[] PlayersPods;
    public List<Zone> Links;
    public int Visits;
    public int AreaId;
    public int CountExcept(int id)
    {
        var n = 0;
        for (int i = 0; i < PlayersPods.Length; i++)
        {
            if (i != id)
            {
                n += PlayersPods[i];
            }
        }
        return n;
    }
}

class Player
{
    static void Main(string[] args)
    {
        var rnd = new Random();
        
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int playerCount = int.Parse(inputs[0]); // the amount of players (2 to 4)
        int myId = int.Parse(inputs[1]); // my player ID (0, 1, 2 or 3)
        int zoneCount = int.Parse(inputs[2]); // the amount of zones on the map
        int linkCount = int.Parse(inputs[3]); // the amount of links between all zones
        var zones = new Zone[zoneCount];

        for (int i = 0; i < zoneCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int zoneId = int.Parse(inputs[0]); // this zone's ID (between 0 and zoneCount-1)
            int platinumSource = int.Parse(inputs[1]); // the amount of Platinum this zone can provide per game turn

            zones[i] = new Zone
            {
                Id = zoneId,
                Platinum = platinumSource,
                Links = new List<Zone>(),
                PlayersPods = new int[4]
            };
        }

        for (int i = 0; i < linkCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int zone1 = int.Parse(inputs[0]);
            int zone2 = int.Parse(inputs[1]);

            zones[zone1].Links.Add(zones[zone2]);
            zones[zone2].Links.Add(zones[zone1]);
        }

        MarkAreas(zones);

        // game loop
        while (true)
        {
            int platinum = int.Parse(Console.ReadLine()); // my available Platinum
            for (int i = 0; i < zoneCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int zId = int.Parse(inputs[0]); // this zone's ID
                int ownerId = int.Parse(inputs[1]); // the player who owns this zone (-1 otherwise)
                int podsP0 = int.Parse(inputs[2]); // player 0's PODs on this zone
                int podsP1 = int.Parse(inputs[3]); // player 1's PODs on this zone
                int podsP2 = int.Parse(inputs[4]); // player 2's PODs on this zone (always 0 for a two player game)
                int podsP3 = int.Parse(inputs[5]); // player 3's PODs on this zone (always 0 for a two or three player game)

                zones[zId].OwnerId = ownerId;
                zones[zId].PlayersPods[0] = podsP0;
                zones[zId].PlayersPods[1] = podsP1;
                zones[zId].PlayersPods[2] = podsP2;
                zones[zId].PlayersPods[3] = podsP3;
            }

            var myzones = zones
                .Where(x => x.OwnerId == myId && x.PlayersPods[myId] > 0)
                .Select(x => new
                {
                    Id = x.Id,
                    Players = x.PlayersPods[myId],
                    Target = x.Links
                    //.Where(l => l.OwnerId != myId )
                    .OrderByDescending(o => RankTarget(x,o,myId)).FirstOrDefault()
                })
                .Where(x=> x.Target != null).ToArray();
                
            if (myzones.Any())
            {
                foreach( var v in myzones )
                {
                    zones[v.Id].Visits++;
                }
                
                var zzz = myzones
                    .Select(x => $"{x.Players} {x.Id} {x.Target.Id}")
                    .ToArray();
    
                Console.WriteLine(string.Join(" ", zzz));
            }
            else
            {
                Console.WriteLine("WAIT");
            }

            var zz = zones
                .Where(x => x.OwnerId == myId || x.OwnerId == -1)
                .OrderByDescending(x => x.Platinum * rnd.Next(10) + x.OwnerId * -10)
                .Take(platinum / 60)
                .Select(x => $"{3} {x.Id}")
                .ToArray();

            if (zz.Any())
            {
                Console.WriteLine(string.Join(" ", zz));
            }
            else
            {
                Console.WriteLine("WAIT");
            }
        }
    }

    public static void MarkAreas(Zone[] zones)
    {

    }

    private static int RankTarget(Zone src, Zone dst, int myId)
    {
        return 
            (dst.OwnerId != myId && dst.CountExcept(myId) == 0 ? 200 : 0) +
            (dst.OwnerId != myId ? 100 : 0) +
            (dst.OwnerId == myId && dst.PlayersPods[myId] == 0 ? 50 : 0) +
            (dst.Visits * -100 ) +
            dst.Platinum;
    }
}