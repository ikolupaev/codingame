using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

//physics: https://www.codingame.com/forum/t/pokerchiprace-physics-explanations/268

struct Vector
{
    public double X;
    public double Y;

    public override string ToString()
    {
        return $"{X:n0} {Y:n0}";
    }

    public void Add(Vector o)
    {
        X += o.X;
        Y += o.Y;
    }

    public double GetDistanceTo(Vector v)
    {
        var dx = v.X - X;
        var dy = v.Y - Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double GetLength()
    {
        return GetDistanceTo(new Vector { X = 0, Y = 0 });
    }
}

class Entity
{
    public Entity()
    {
    }

    public Entity(Entity e)
    {
        Id = e.Id;
        PlayerId = e.PlayerId;
        Radius = e.Radius;
        P = e.P;
        V = e.V;
    }

    public int Id; // Unique identifier for this entity
    public int PlayerId; // The owner of this entity (-1 for neutral droplets)
    public float Radius; // the radius of this entity
    public Vector P; // the X coordinate (0 to 799)
    public Vector V; // the speed of this entity along the X axis

    public bool Oil => PlayerId == -1;

    public override string ToString()
    {
        return $"{Id}: {P} {V}";
    }

    public void AdjustToNextPosition()
    {
        P.X += V.X;
        P.Y += V.Y;
    }

    public int WillCollideWith(Entity e)
    {
        var me = new Entity(this);
        var obj = new Entity(e);

        for (int i = 0; i < 5; i++)
        {
            me.AdjustToNextPosition();
            obj.AdjustToNextPosition();
            if (me.IsColided(obj)) return i;
        }

        return 0;
    }

    public bool IsColided(Entity e)
    {
        return e.P.GetDistanceTo(e.P) < e.Radius + Radius;
    }
}

class Player
{
    static void Main(string[] args)
    {
        int playerId = int.Parse(Console.ReadLine()); // your id (0 to 4)
        Entity[] entities;
        // game loop
        while (true)
        {
            int playerChipCount = int.Parse(Console.ReadLine()); // The number of chips under your control
            int entityCount = int.Parse(Console.ReadLine()); // The total number of entities on the table, including your chips
            entities = new Entity[entityCount];

            for (int i = 0; i < entityCount; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                entities[i] = new Entity
                {
                    Id = int.Parse(inputs[0]),
                    PlayerId = int.Parse(inputs[1]),
                    Radius = float.Parse(inputs[2]),
                    P = new Vector
                    {
                        X = float.Parse(inputs[3]),
                        Y = float.Parse(inputs[4])
                    },
                    V = new Vector
                    {
                        X = float.Parse(inputs[5]),
                        Y = float.Parse(inputs[6])
                    }
                };
            }

            foreach (var chip in entities.Where(x => x.PlayerId == playerId))
            {
                D(chip.V.GetLength());

                var target = entities
                    .Where(x => (x.Radius < chip.Radius))
                    .OrderBy(x => chip.P.GetDistanceTo(x.P))
                    .FirstOrDefault();

                if (target.P.GetDistanceTo(chip.P) < target.Radius + chip.Radius + 5 || chip.V.GetLength() < 10)
                {
                    Console.WriteLine($"{target.P.X} {target.P.Y}");
                }
                else
                {
                    Console.WriteLine("WAIT");
                }
            }
        }
    }

    public static void D(params object[] args)
    {
        Console.Error.WriteLine(string.Join(" ", args.Select(x => x.ToString())));
    }
}