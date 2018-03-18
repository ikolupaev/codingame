using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


/**
 * Save humans, destroy zombies!
 **/
public class Player
{
    static Point ash = new Point();
    static Human[] humans = new Human[100];
    static Zombie[] zombies = new Zombie[100];

    static int[][] humanDistances;
    static int[][] zombiesDistances;

    static int humanCount;
    static int zombieCount;
    static Zombie centralZombie;

    public class HumansCollection
    {
        public Human[] Humans = new Human[100];
        public int HumansCount;
        public int[,] Distances = new int[100, 100];

        public void ReadHumans(TextReader reader, TextWriter trace)
        {
            HumansCount = int.Parse(reader.ReadLine());

            for (int i = 0; i < HumansCount; i++)
            {
                Humans[i] = new Human();

                var h = reader.ReadLine();
                trace.WriteLine(h);
                Humans[i].Init(h);
            }

            CalcDistances();
        }

        private void CalcDistances()
        {
            for (int i = 0; i < HumansCount - 1; i++)
            {
                for (int j = i; j < HumansCount; j++)
                {
                    if (j == i)
                    {
                        Distances[i, j] = 0;
                    }
                    else
                    {
                        Distances[i, j] = Humans[i].GetDistanceTo(Humans[j]);
                        Distances[j, i] = Distances[i, j];
                    }
                }
            }
        }
    }

    public class Point
    {
        public int X;
        public int Y;

        public bool AlmostEqual(Point p)
        {
            var distance = GetDistanceTo(p);
            Log("distance", distance);
            return distance < 1000;
        }

        public int GetDistanceTo(Point p)
        {
            return (int)Math.Sqrt(SqSum(this.X, p.X) + SqSum(this.Y, p.Y));
        }

        private int SqSum(int a, int b)
        {
            var d = Math.Abs(a - b);
            return d * d;
        }
    }

    public class Human : Point
    {
        public int Index;

        public Human()
        {
        }

        public void Init(string line)
        {
            var inputs = line.Split(' ');

            Index = int.Parse(inputs[0]);
            X = int.Parse(inputs[1]);
            Y = int.Parse(inputs[2]);
        }
    }

    class Zombie : Human
    {
        public Point Destination;

        public Zombie()
        {
            Destination = new Point();
        }

        public void Init(string input)
        {
            var inputs = input.Split(' ');

            Index = int.Parse(inputs[0]);
            X = int.Parse(inputs[1]);
            Y = int.Parse(inputs[2]);

            Destination.X = int.Parse(inputs[3]);
            Destination.Y = int.Parse(inputs[4]);
        }
    }

    static void Main(string[] args)
    {
        InitArrays();

        // game loop
        while (true)
        {
            ReadAsh();

            ReadHumans();

            ReadZombies();

            humanDistances = CalcDistances(humans, humanCount);
            zombiesDistances = CalcDistances(zombies, zombieCount);

            Point targetPoint;

            if (AllZombiesAreTogether())
            {
                targetPoint = centralZombie;
            }
            else if (ZombiesGoSamePoint())
            {
                targetPoint = centralZombie.Destination;
            }
            else
            {
                var centralHuman = GetPointWithMinDistanceToOthers(humans, humanCount, humanDistances);

                Log("central human:", centralHuman.Index);

                if (AshCloserThanZombie(centralHuman))
                {
                    targetPoint = centralHuman;
                }
                else
                {
                    var h = GetFarerHumanFromZombies();
                    var humansCount1 = humanDistances[h.Index].Count(x => x < 2000);

                    var humansCount2 = GetHumansNumberWithinShootingArea(ash);

                    if (humansCount2 > humansCount1)
                    {
                        targetPoint = ash;
                    }
                    else
                    {
                        targetPoint = h;
                    }
                }
            }

            Console.WriteLine(string.Format("{0} {1}", targetPoint.X, targetPoint.Y)); // Your destination coordinates
        }
    }

    private static int GetHumansNumberWithinShootingArea(Point p)
    {
        return GetAllHumans().Count(x => x.GetDistanceTo(p) < 2000);
    }

    private static IEnumerable<Human> GetAllHumans()
    {
        return humans.Take(humanCount);
    }

    static bool AllZombiesAreTogether()
    {
        centralZombie = GetPointWithMinDistanceToOthers(zombies, zombieCount, zombiesDistances);
        return GetAllZombies().All(z => z.GetDistanceTo(centralZombie) < 2000);
    }

    static IEnumerable<Zombie> GetAllZombies()
    {
        return zombies.Take(zombieCount);
    }

    private static bool AshCloserThanZombie(Point p)
    {
        var zombieWithDistance = GetClosest(p, zombies, zombieCount);
        var timeToEat = zombieWithDistance.Item2 / 400;
        var ashDistance = ash.GetDistanceTo(p);
        var timeToShoot = (ashDistance - 2000) / 1000;
        return timeToShoot < timeToEat;
    }

    static void Log(string message, Point p)
    {
        Console.Error.WriteLine(string.Format("{0}: {1},{2}", message, p.X, p.Y));
    }

    static void Log(string v, int distance)
    {
        Console.Error.WriteLine(v + ": " + distance.ToString());
    }

    private static bool ZombiesGoSamePoint()
    {
        var firstZombieDestination = zombies[0].Destination;

        for (int i = 0; i < zombieCount; i++)
        {
            var z = zombies[i];

            Log("zombie goal", z.Destination);

            if (!z.Destination.AlmostEqual(firstZombieDestination))
            {
                return false;
            }

        }

        Console.Error.WriteLine("All zombies go same place");

        return true;
    }

    private static Human GetFarerHumanFromZombies()
    {
        var max = 0;
        var maxIndex = 0;

        for (int i = 0; i < humanCount; i++)
        {
            var l = GetClosest(humans[i], zombies, zombieCount);

            Console.Error.WriteLine(string.Format("human[{0}].MinZombieDist = {1}", i, l.Item2));

            if (l.Item2 > max)
            {
                max = l.Item2;
                maxIndex = i;
            }
        }

        humans[maxIndex].Index = maxIndex;
        return humans[maxIndex];
    }

    private static void InitArrays()
    {
        for (int i = 0; i < 100; i++)
        {
            zombies[i] = new Zombie();
            humans[i] = new Human();
        }
    }

    private static void ReadZombies()
    {
        zombieCount = int.Parse(Console.ReadLine());

        for (int i = 0; i < zombieCount; i++)
        {
            zombies[i].Init(Console.ReadLine());
        }
    }

    private static void ReadAsh()
    {
        string[] inputs = Console.ReadLine().Split(' ');
        ash.X = int.Parse(inputs[0]);
        ash.Y = int.Parse(inputs[1]);
    }

    private static T GetPointWithMinDistanceToOthers<T>(T[] points, int pointCount, int[][] distances) where T : Point
    {
        if (pointCount == 1)
        {
            return points[0];
        }

        var minIndex = 0;
        var minSum = int.MaxValue;

        for (int i = 0; i < pointCount; i++)
        {
            var sum = distances[i].Sum();

            if (sum < minSum)
            {
                minIndex = i;
                minSum = sum;
            }
        }

        return points[minIndex];
    }

    private static Tuple<T, int> GetClosest<T>(Point p, T[] points, int count) where T : Point
    {
        var minIndex = 0;
        var minDist = int.MaxValue;

        for (var i = 0; i < count; i++)
        {
            var dist = p.GetDistanceTo(points[i]);

            if (dist < minDist)
            {
                minIndex = i;
                minDist = dist;
            }
        }

        return Tuple.Create(points[minIndex], minDist);
    }

    private static T GetFarest<T>(Point p, T[] points, int count) where T : Point
    {
        var maxIndex = 0;
        var maxSum = int.MinValue;

        for (var i = 0; i < count; i++)
        {
            var sum = p.GetDistanceTo(points[i]);

            if (sum > maxSum)
            {
                maxIndex = i;
                maxSum = sum;
            }
        }

        return points[maxIndex];
    }

    private static void ReadHumans()
    {
        humanCount = int.Parse(Console.ReadLine());


        for (int i = 0; i < humanCount; i++)
        {
            var h = Console.ReadLine();
            Console.Error.WriteLine(h);
            humans[i].Init(h);
        }
    }

    private static int[][] CalcDistances(Point[] points, int pointCount)
    {
        var pointDist = new int[pointCount][];

        for (int i = 0; i < pointCount - 1; i++)
        {
            pointDist[i] = new int[pointCount];

            for (int j = i; j < pointCount; j++)
            {
                if (j == i)
                {
                    pointDist[i][j] = 0;
                }
                else
                {
                    pointDist[i][j] = points[i].GetDistanceTo(points[j]);

                    if (pointDist[j] == null)
                    {
                        pointDist[j] = new int[pointCount];
                    }

                    pointDist[j][i] = pointDist[i][j];
                }
            }
        }

        return pointDist;
    }
}