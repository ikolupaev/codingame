using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Send your busters out into the fog to trap ghosts and bring them home!
 **/

class Player
{
    private static int bustersPerPlayer;
    private static int ghostCount;
    private static int myTeamId;

    static Random rnd = new Random();

    struct Point : IEqualityComparer<Point>
    {
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x;
        public int y; // position of this buster / ghost

        public int GetDistanceTo(Point p)
        {
            var xx = p.x - x;
            var yy = p.y - y;
            return (int)Math.Sqrt(xx * xx + yy * yy);
        }

        public bool Equals(Point x, Point y)
        {
            return x.x == y.x && x.y == y.y;
        }

        public int GetHashCode(Point obj)
        {
            return obj.x * 10000 + y;
        }

        public bool HasValue()
        {
            return x >= 0 && y >= 0;
        }
    }

    class Entity
    {
        public int entityId; // buster id or ghost id
        public Point Point = new Point();
        public int entityType; // the team id if it is a buster, -1 if it is a ghost.
        public int state; // For busters: 0=idle, 1=carrying a ghost.
        public int value; // For busters: Ghost id being carried. For ghosts: number of busters attempting to trap this ghost.
        public Point Goal = new Point { x = -1, y = -1 };
        public int StunStepCounter;

        public void Init(string line)
        {
            string[] inputs = line.Split(' ');

            entityId = int.Parse(inputs[0]); // buster id or ghost id
            Point.x = int.Parse(inputs[1]);
            Point.y = int.Parse(inputs[2]); // position of this buster / ghost
            entityType = int.Parse(inputs[3]); // the team id if it is a buster, -1 if it is a ghost.
            state = int.Parse(inputs[4]); // For busters: 0=idle, 1=carrying a ghost.
            value = int.Parse(inputs[5]); // For busters: Ghost id being carried. For ghosts: number of busters attempting to trap this ghost.
        }

        public int GhostStamina => value;
        public int TrappingGhostId => value;
    }

    static Entity[] entitiesBuffer = new Entity[5 * 2 + 28];
    static int entitiesNumber;
    static List<Point> goals = new List<Point>();
    static int nextGoalIndex = 0;

    static IEnumerable<Entity> GetEntities()
    {
        return entitiesBuffer.Take(entitiesNumber);
    }

    static Point trap = new Point();

    class BusterStates
    {
        public const int Idle = 0;
        public const int CarryingGhost = 1;
        public const int Stunned = 2;
        public const int Trapping = 2;
    }

    static void Main(string[] args)
    {
        bustersPerPlayer = int.Parse(Console.ReadLine()); // the amount of busters you control
        ghostCount = int.Parse(Console.ReadLine()); // the amount of ghosts on the map
        myTeamId = int.Parse(Console.ReadLine()); // if this is 0, your base is on the top left of the map, if it is one, on the bottom right

        SetTrap();

        InitGoals();

        // game loop
        while (true)
        {
            UpdateEntities();
            var busters = GetBustersOrdered().ToArray();

            Console.Error.WriteLine($"handling busters: {busters.Length}");
            foreach (var buster in busters)
            {
                Console.Error.Write($"{buster.entityId} ");

                /*
                if (buster.entityId != busters[0].entityId)
                {
                    MoveTo(buster.Point);
                    continue;
                }
                */

                switch (buster.state)
                {
                    case BusterStates.Trapping:
                    case BusterStates.CarryingGhost:
                        BringGhostToTrap(buster);
                        break;
                    default:
                        SeekAndCatch(buster);
                        break;
                }
            }
        }
    }

    private static void BringGhostToTrap(Entity buster)
    {
        if (buster.Point.GetDistanceTo(trap) <= 1600)
        {
            Console.WriteLine("RELEASE");
        }
        else
        {
            MoveToOrStun(buster, trap);
        }
    }

    private static void SeekAndCatch(Entity buster)
    {
        var closestGhost = GetClosestGhostTo(buster);
        if (closestGhost != null)
        {
            Console.Error.WriteLine($"DealWithGhost");
            DealWithGhost(buster, closestGhost);
        }
        else
        {
            KeepLooking(buster);
        }
    }

    private static void InitGoals()
    {
        goals.Add(new Point(1350, 1350)); //left top trap
        goals.Add(new Point(16000 - 1350, 9000 - 1350));
        goals.Add(new Point(16000 / 2, 9000 / 2));
        goals.Add(new Point(1350, 9000 - 1350));
        goals.Add(new Point(16000 - 1350, 1350));

        goals = goals.OrderByDescending(x => x.GetDistanceTo(trap)).ToList();
        goals.RemoveAt(goals.Count - 1);
    }

    private static void MoveToOrStun(Entity buster, Point p)
    {
        var enemyToStun = GetEnemyToStun(buster);
        if (enemyToStun != null)
        {
            Stun(buster, enemyToStun);
        }
        else
        {
            Console.WriteLine($"MOVE {p.x} {p.y} {buster.StunStepCounter}");
        }
    }

    private static void Stun(Entity buster, Entity enemyToStun)
    {
        buster.StunStepCounter = 0;
        Console.WriteLine($"STUN {enemyToStun.entityId}");
    }

    private static Entity GetEnemyToStun(Entity buster)
    {
        if (buster.StunStepCounter < 20)
            return null;

        if (buster.state == BusterStates.CarryingGhost)
            return null;

        var enemies = GetEnemies().Where(x => x.state != BusterStates.Stunned);
        var enemy = GetClosest(enemies, buster.Point);

        if (enemy != null && enemy.state == BusterStates.CarryingGhost && enemy.Point.GetDistanceTo(buster.Point) <= 1760)
        {
            return enemy;
        }

        return null;
    }

    private static void KeepLooking(Entity buster)
    {

        if (!buster.Goal.HasValue())
        {
            buster.Goal = GetNextGoal(buster);
        }

        if (buster.Goal.GetDistanceTo(buster.Point) < 500)
        {
            buster.Goal = GetNextGoal(buster);
        }

        MoveToOrStun(buster, buster.Goal);
    }

    private static void MoveRandom(Entity buster)
    {
        var p = buster.Point;
        p.x += rnd.Next(-16000, 16000);
        p.y += rnd.Next(-9000, 9000);

        if (p.x < 0) p.x = 0;
        if (p.y < 0) p.y = 0;
        if (p.x > 16000) p.x = 16000;
        if (p.y > 9000) p.y = 9000;

        MoveToOrStun(buster, p);
    }

    private static void DealWithGhost(Entity buster, Entity closestGhost)
    {
        var distance = buster.Point.GetDistanceTo(closestGhost.Point);
        if (distance <= 1760)
        {
            if (distance > 900)
            {
                BustOrStun(buster, closestGhost);
            }
            else
            {
                MoveAwayFromGhost(buster, closestGhost);
            }
        }
        else
        {
            MoveToOrStun(buster, closestGhost.Point);
        }
    }

    private static void BustOrStun(Entity buster, Entity closestGhost)
    {
        var enemy = GetEnemyToStun(buster);
        if (enemy != null)
        {
            Stun(buster, enemy);
        }
        else
        {
            Console.WriteLine("BUST " + closestGhost.entityId);
        }
    }

    private static void MoveAwayFromGhost(Entity buster, Entity ghost)
    {
        var p = buster.Point;
        p.x += buster.Point.x - ghost.Point.x;
        p.y += buster.Point.y - ghost.Point.y;

        MoveToOrStun(buster, p);
    }

    private static Entity GetClosestGhostTo(Entity buster)
    {
        var ghosts = GetEntities().Where(x => x.entityType == -1);

        if (ghosts.Any())
        {
            return GetClosest(ghosts, buster.Point);
        }
        else
        {
            return null;
        }
    }

    private static Entity GetClosest(IEnumerable<Entity> entities, Point point)
    {
        var minDist = int.MaxValue;
        Entity entity = null;

        foreach (var e in entities)
        {
            var d = e.Point.GetDistanceTo(point);
            if (d < minDist)
            {
                minDist = d;
                entity = e;
            }
        }

        return entity;
    }

    private static IEnumerable<Entity> GetBustersOrdered()
    {
        return GetEntities().Where(x => x.entityType == myTeamId).OrderBy(x => x.entityId);
    }

    private static IEnumerable<Entity> GetEnemies()
    {
        return GetEntities().Where(x => x.entityType != myTeamId && x.entityType != -1);
    }

    private static void SetTrap()
    {
        if (myTeamId == 0)
        {
            trap.x = 0;
            trap.y = 0;
        }
        else
        {
            trap.x = 16000;
            trap.y = 9000;
        }
    }

    private static void UpdateEntities()
    {
        entitiesNumber = int.Parse(Console.ReadLine()); // the number of busters and ghosts visible to you
        Console.Error.Write($"Loading {entitiesNumber} entities...");

        for (int i = 0; i < entitiesNumber; i++)
        {
            if (entitiesBuffer[i] == null)
            {
                entitiesBuffer[i] = new Entity();
                entitiesBuffer[i].StunStepCounter = 20;
            }

            entitiesBuffer[i].Init(Console.ReadLine());
            entitiesBuffer[i].StunStepCounter++;
        }
    }

    private static Point GetNextGoal(Entity buster)
    {
        var ghost = FindClosestBustingGhost(buster);

        if (ghost != null)
        {
            return ghost.Point;
        }
        else
        {
            var ret = goals[nextGoalIndex];
            nextGoalIndex = (++nextGoalIndex) % goals.Count;
            return ret;
        }
    }

    private static Entity FindClosestBustingGhost(Entity buster)
    {
        var ghostsIds = GetBustersOrdered().Where(x => x.state == BusterStates.Trapping).Select(x => x.value).Distinct();

        if (!ghostsIds.Any())
        {
            return null;
        }

        var ghosts = GetEntities().Where(x => x.entityType == -1 && ghostsIds.Contains(x.entityId));

        if (!ghosts.Any())
        {
            return null;
        }

        var closest = GetClosest(ghosts, buster.Point);

        if (closest.Point.GetDistanceTo(buster.Point) > 4000)
            return null;

        return closest;
    }
}