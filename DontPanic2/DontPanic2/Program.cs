using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Elevator
{
    public int Floor;
    public int Pos;
}

class Area
{
    public int From;
    public int To;
}

class Player
{
    static List<Elevator> elevators = new List<Elevator>();
    static int cloneFloor;
    static int clonePos;
    static String direction;
    static int exitFloor;
    static int exitPos;
    static int nbFloors;
    static Area[] areas;

    static void Main(String[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        nbFloors = int.Parse(inputs[0]); // number of floors
        int width = int.Parse(inputs[1]); // width of the area
        int nbRounds = int.Parse(inputs[2]); // maximum number of rounds
        exitFloor = int.Parse(inputs[3]); // floor on which the exit is found
        exitPos = int.Parse(inputs[4]); // position of the exit on its floor
        int nbTotalClones = int.Parse(inputs[5]); // number of generated clones
        int nbAdditionalElevators = int.Parse(inputs[6]); // ignore (always zero)
        int nbElevators = int.Parse(inputs[7]); // number of elevators

        for (int i = 0; i < nbElevators; i++)
        {
            var s = Console.ReadLine();
            inputs = s.Split(' ');
            int elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
            int elevatorPos = int.Parse(inputs[1]); // position of the elevator on its floor

            elevators.Add(new Elevator { Floor = elevatorFloor, Pos = elevatorPos });
            Console.Error.WriteLine(s);
        }

        areas = new Area[exitFloor + 2];
        areas[exitFloor + 1] = new Area { From = exitPos, To = exitPos };
        for (var f = exitFloor; f >= 0; f--)
        {
            var floorElevators = elevators.Where(x => x.Floor == f);
            var leftElevator = floorElevators.Where(x => x.Pos <= areas[f + 1].From).OrderBy(x => x.Pos).LastOrDefault();
            var rightElevator = floorElevators.Where(x => x.Pos >= areas[f + 1].To).OrderBy(x => x.Pos).FirstOrDefault();

            areas[f] = new Area { From = leftElevator?.Pos + 1 ?? 0, To = rightElevator?.Pos - 1 ?? width - 1 };

            D("valid area for floor:", f, areas[f].From, areas[f].To);
        }

        elevators.Add(new Elevator { Floor = exitFloor, Pos = exitPos });

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');

            cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
            clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
            direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT

            if (cloneFloor < 0)
            {
                Console.WriteLine("WAIT");
                continue;
            }

            var dirToArea = GetDirectionTo(areas[cloneFloor + 1].From, areas[cloneFloor + 1].To);
            if (dirToArea != null && dirToArea != direction)
            {
                Console.WriteLine("BLOCK"); // action: WAIT or BLOCK
                continue;
            }

            var epos = GetElevatorPos().ToArray();

            if (!epos.Any())
            {
                if (ValidArea())
                {
                    Console.WriteLine("ELEVATOR");
                    elevators.Add(new Elevator { Floor = cloneFloor, Pos = clonePos });
                    nbAdditionalElevators--;
                    continue;
                }
            }

            if (nbAdditionalElevators > 0 && ValidArea() && Math.Abs(GetClosestElevator() - clonePos) > width / 2)
            {
                Console.WriteLine("ELEVATOR");
                elevators.Add(new Elevator { Floor = cloneFloor, Pos = clonePos });
                nbAdditionalElevators--;
                continue;
            }

            if (!epos.Contains(clonePos) && WrongDirection())
            {
                Console.WriteLine("BLOCK"); // action: WAIT or BLOCK
                continue;
            }

            Console.WriteLine("WAIT");
        }
    }

    private static bool ValidArea()
    {
        return clonePos >= areas[cloneFloor + 1].From && clonePos <= areas[cloneFloor + 1].To;
    }

    private static string GetDirectionTo(int from, int to)
    {
        if (clonePos < from)
        {
            return "RIGHT";
        }
        else if (clonePos > to)
        {
            return "LEFT";
        }
        else
        {
            return null;
        }
    }

    static IEnumerable<int> GetElevatorPos()
    {
        return elevators
            .Where(x => x.Floor == cloneFloor)
            .Where(x => x.Pos >= areas[cloneFloor + 1].From && x.Pos <= areas[cloneFloor + 1].To)
            .Select(x => x.Pos);
    }

    private static bool WrongDirection()
    {
        var dir = GetDirectionToElevator();
        Console.Error.WriteLine(dir);
        return dir != direction;
    }

    static int GetClosestElevator()
    {
        return GetElevatorPos().OrderBy(x => Math.Abs(x - clonePos)).First();
    }

    private static string GetDirectionToElevator()
    {
        var pos = GetElevatorPos().OrderBy(x => Math.Abs(x - clonePos)).First();

        if (pos > clonePos)
        {
            return "RIGHT";
        }
        else
        {
            return "LEFT";
        }
    }

    static IEnumerable<int> GetFloorsWithoutElevators()
    {
        for (int i = 0; i < nbFloors; i++)
        {
            if (!elevators.Any(x => x.Floor == i))
            {
                yield return i;
            }
        }
    }

    public static void D(params object[] args)
    {
        Console.Error.WriteLine(string.Join(" ", args.Select(x => x.ToString())));
    }
}