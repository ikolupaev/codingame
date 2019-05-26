using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class Player
{
    static Stopwatch timer = new Stopwatch();
    static int ticks = 0;

    static void Main(string[] args)
    {
        var world = new World();

        string[] inputs;
        int numberMineSpots = int.Parse(Console.ReadLine());

        for (int i = 0; i < numberMineSpots; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            world.MineSpots.Add(new Cell(x, y));
        }

        // game loop
        while (true)
        {
            World.Seed = World.Rnd.Next();
            World.Rnd = new Random(World.Seed);

            world.Gold[0] = int.Parse(Console.ReadLine());
            world.Income[0] = int.Parse(Console.ReadLine());
            world.Gold[1] = int.Parse(Console.ReadLine());
            world.Income[1] = int.Parse(Console.ReadLine());

            World.AllCells.Clear();
            for (int i = 0; i < 4; i++) world.Cells[i].Clear();
            for (int i = 0; i < 12; i++)
            {
                string line = Console.ReadLine();
                for (var j = 0; j < 12; j++)
                {
                    var cellType = GetCellType(line[j]);
                    if (cellType != CellType.Void)
                    {
                        var cell = new Cell(j, i);
                        World.AllCells.Add(cell);
                        world.Cells[(int)cellType].Add(cell);
                        //world.CellTypes[cell] = cellType;
                    }
                }
            }

            int buildingCount = int.Parse(Console.ReadLine());
            world.Buildings[0].Clear();
            world.Buildings[1].Clear();
            for (int i = 0; i < buildingCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int owner = int.Parse(inputs[0]);
                var buildingType = (BuildingType)int.Parse(inputs[1]);
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                var cell = new Cell(x, y);

                if (owner == 1 && buildingType == BuildingType.HQ) World.EnemyHQ = cell;
                world.Buildings[owner][cell] = buildingType;
            }

            int unitCount = int.Parse(Console.ReadLine());
            world.Units[0].Clear();
            world.Units[1].Clear();
            for (int i = 0; i < unitCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int owner = int.Parse(inputs[0]);
                int unitId = int.Parse(inputs[1]);
                int level = int.Parse(inputs[2]);
                int x = int.Parse(inputs[3]);
                int y = int.Parse(inputs[4]);
                var cell = new Cell(x, y);
                world.Units[owner][cell] = new Unit
                {
                    Team = owner,
                    Cell = cell,
                    ID = unitId,
                    Level = level
                };
            }

            var ss = world.Serialize();
            while (ss.Length >= 80)
            {
                D(ss.Substring(0, 80));
                ss = ss.Substring(80);
            }
            D(ss);
            D("");

            var moves = Solve(world).ToArray();
            Console.WriteLine(GetMovesStr(moves));
        }
    }

    public static string GetMovesStr(IEnumerable<Turn> moves)
    {
        if (!moves.Any()) return "WAIT";
        else return string.Join(';', moves.Select(x => x.ToString()));
    }

    private static CellType GetCellType(char v)
    {
        switch (v)
        {
            case '.': return CellType.Neutral;
            case 'O': return CellType.MyActive;
            case 'o': return CellType.MyInactive;
            case 'X': return CellType.OpActive;
            case 'x': return CellType.OpInactive;
            default: return CellType.Void;
        }
    }

    public static List<Turn> Solve(World world)
    {
        List<Turn> bestMoves = null;
        var bestScore = double.MinValue;

        ticks = 0;
#if !DEBUG
        timer.Restart();
#endif

        while (true)
        {
            ticks++;
            var world1 = new World(world);
            world1.MakeNextTurn(0);
            var score = CalcScore(world1, 0, 4);
            if (timer.ElapsedMilliseconds >= 50) break;
            if (bestScore < score)
            {
                bestScore = score;
                bestMoves = world1.TurnMoves;

                var s = Player.GetMovesStr(bestMoves);
                D($"{bestScore} {s}");
            }
        }

        D($"ticks: {ticks}");

        return bestMoves;
    }

    private static double CalcScore(World world, int team, int level)
    {
        if (timer.ElapsedMilliseconds > 50) return double.MinValue;

        if (level == 0)
        {
            var score = world.CalcScore(team);
            if (team == 1) score *= 1;
            return score;
        }

        team = 1 - team;
        var world1 = new World(world);
        world1.MakeNextTurn(team);
        return CalcScore(world1, team, level - 1);
    }

    static void D(string message)
    {
        Console.Error.WriteLine(message);
    }
}

public enum CellType
{
    Neutral = 4,
    MyActive = 0,
    MyInactive = 2,
    OpActive = 1,
    OpInactive = 3,
    Void = -1
}

public enum BuildingType
{
    HQ = 0,
    Mine = 1,
    Tower = 2
}

public class Cell : IEquatable<Cell>
{
    public Cell(Cell cell)
    {
        X = cell.X;
        Y = cell.Y;
    }

    public Cell(int x, int y)
    {
        X = x;
        Y = y;
    }

    public readonly int X;
    public readonly int Y;

    public bool IsAdustent(Cell other)
    {
        var dx = Math.Abs(other.X - X);
        var dy = Math.Abs(other.Y - Y);
        return dx + dy == 1;
    }

    public int GetDistance(Cell other)
    {
        if (other == null) return int.MaxValue;
        return Math.Abs(other.X - X) + Math.Abs(other.Y - Y);
    }

    public bool Equals(Cell other)
    {
        return other.X == X && other.Y == Y;
    }

    public override int GetHashCode()
    {
        return (Y << 4) | X;
    }

    internal void Serialize(BinaryWriter writer)
    {
        writer.Write((byte)((Y << 4) | X));
    }

    public static Cell Deserialize(BinaryReader reader)
    {
        var a = reader.ReadByte();
        return new Cell(a & 0xf, a >> 4);
    }

    internal Cell[] GetAdustent()
    {
        return new[]
        {
            new Cell(X, Y + 1),
            new Cell(X, Y - 1),
            new Cell(X + 1, Y),
            new Cell(X - 1, Y)
        };
    }

    public override string ToString()
    {
        return $"{X} {Y}";
    }
}

public class FieldCell
{
    public Cell Cell;
    public CellType CellType;

    internal void Serialize(BinaryWriter writer)
    {
        Cell.Serialize(writer);
        writer.Write((byte)CellType);
    }

    public static FieldCell Deserialize(BinaryReader reader)
    {
        var cell = Cell.Deserialize(reader);
        return new FieldCell
        {
            Cell = cell,
            CellType = (CellType)reader.ReadByte()
        };
    }
}

public class Building
{
    public int Team;
    public BuildingType BuildingType;

    public void Serialize(BinaryWriter writer)
    {
        writer.Write((byte)((((byte)BuildingType) << 1) | Team));
    }

    public static Building Deserialize(BinaryReader reader)
    {
        var a = reader.ReadByte();

        return new Building
        {
            Team = (a & 1),
            BuildingType = (BuildingType)(a >> 1)
        };
    }
}

public class Unit
{
    public Cell Cell;
    public int Team;
    public int Level;
    public int ID;

    public Unit() { }
    public Unit(Unit unit)
    {
        ID = unit.ID;
        Cell = unit.Cell;
        Team = unit.Team;
        Level = unit.Level;
    }

    public void Serialize(BinaryWriter writer)
    {
        Cell.Serialize(writer);
        writer.Write((byte)ID);
        writer.Write((byte)Team);
        writer.Write((byte)Level);
    }

    public static Unit Deserialize(BinaryReader reader)
    {
        var cell = Cell.Deserialize(reader);
        var id = reader.ReadByte();
        var team = reader.ReadByte();
        var level = reader.ReadByte();

        return new Unit
        {
            ID = id,
            Team = team,
            Level = level,
            Cell = cell
        };
    }

    public int GetUpkeepCost()
    {
        if (Level == 1) return 1;
        if (Level == 2) return 4;
        return 20;
    }

    public override string ToString()
    {
        return $"id:{ID} cell:{Cell} team:{Team} level:{Level}";
    }
}

public class World
{
    public List<Turn> TurnMoves = new List<Turn>();
    public int[] Gold = new int[2];
    public int[] Income = new int[2];
    public static HashSet<Cell> AllCells = new HashSet<Cell>();
    //public Dictionary<Cell,CellType> CellTypes = new Dictionary<Cell,CellType>();
    public HashSet<Cell>[] Cells = new[] { new HashSet<Cell>(), new HashSet<Cell>(), new HashSet<Cell>(), new HashSet<Cell>(), new HashSet<Cell>() };
    public Dictionary<Cell, BuildingType>[] Buildings = new[] { new Dictionary<Cell, BuildingType>(), new Dictionary<Cell, BuildingType>() };
    public Dictionary<Cell, Unit>[] Units = new[] { new Dictionary<Cell, Unit>(), new Dictionary<Cell, Unit>() };
    public HashSet<Cell> MineSpots = new HashSet<Cell>();

    public World()
    {
    }

    static World()
    {
        Seed = Environment.TickCount;
        Rnd = new Random(Seed);
    }

    static public int Seed;
    static public Random Rnd;

    public static Cell EnemyHQ;

    public World(World world)
    {
        for (int i = 0; i < 2; i++)
        {
            Gold[i] = world.Gold[i];
            Income[i] = world.Income[i];
            Cells[i] = new HashSet<Cell>(world.Cells[i]);
            Buildings[i] = new Dictionary<Cell, BuildingType>(world.Buildings[i]);
            Units[i] = world.Units[i].Values.Select(x => new Unit(x)).ToDictionary(x => x.Cell);
            MineSpots = new HashSet<Cell>(world.MineSpots);
        }
    }

    internal void MakeNextTurn(int team)
    {
        var cellsToTrain = GetValidCellsToTrain(team).ToArray();

        if (Units[team].Values.Count(x => x.Level == 1) < 12)
        {
            while (Gold[team] >= 10 && CalcUpkeepCost(team) + 1 <= Income[team])
            {
                AddRandomTrain(team, cellsToTrain, 1);
            }
        }
        else
        {
            while (Gold[team] >= 30 && CalcUpkeepCost(team) + 4 <= Income[team])
            {
                AddRandomTrain(team, cellsToTrain, 3);
            }

            while (Gold[team] >= 20 && CalcUpkeepCost(team) + 20 <= Income[team])
            {
                AddRandomTrain(team, cellsToTrain, 2);
            }
        }

        foreach (var unit in Units[team].Values.ToArray())
        {
            if (Rnd.NextDouble() > 0.6) continue;

            if (unit.ID < 0) continue;

            var cells = GetValidCellsToMove(unit).ToArray();
            if (!cells.Any()) continue;

            var cell = cells[Rnd.Next(cells.Length)];

            Units[1 - team].Remove(cell);
            Buildings[1 - team].Remove(cell);

            Units[team].Remove(unit.Cell);

            unit.Cell = cell;
            Units[team][unit.Cell] = unit;
            Cells[team].Add(unit.Cell);

            TurnMoves.Add(Turn.Move(unit, cell));
        }

        Gold[team] += Cells[team].Count;

        foreach (var x in Units[team].Values)
        {
            if (x.Level == 1) Gold[team] -= 1;
            else if (x.Level == 2) Gold[team] -= 4;
            else if (x.Level == 3) Gold[team] -= 20;
        }
    }

    private void AddRandomTrain(int team, HashSet<Cell>[] cells, int level)
    {
        var levelCells = cells[level - 1].ToArray();
        if (!levelCells.Any()) return;

        var cell = levelCells[Rnd.Next(levelCells.Length)];
        AddTrainMove(team, cells, level, cell);
    }

    public void AddTrainMove(int team, HashSet<Cell>[] cells, int level, Cell cell)
    {
        var turn = Turn.Train(cell, team, level);

        TurnMoves.Add(turn);

        Cells[team].Add(cell);
        Units[team].Add(cell, turn.Unit);
        Cells[1 - team].Remove(cell);
        Units[1 - team].Remove(cell);

        Gold[team] -= level * 10;

        for (int i = 0; i < 3; i++)
        {
            cells[i].Remove(cell);
        }
    }

    public IEnumerable<Cell> GetValidCellsToMove(Unit unit)
    {
        foreach (var cell in unit.Cell.GetAdustent())
        {
            if (!AllCells.Contains(cell)) continue;
            if (Buildings[unit.Team].ContainsKey(cell)) continue;
            if (Units[unit.Team].ContainsKey(cell)) continue;

            if (unit.Level == 3) yield return cell;

            var opUnitExists = Units[1 - unit.Team].TryGetValue(cell, out Unit opUnit);
            if (opUnitExists && unit.Level < opUnit.Level) continue;

            var opBuildingsExists = Buildings[1 - unit.Team].TryGetValue(cell, out BuildingType opBuilding);
            if (opBuildingsExists && opBuilding != BuildingType.HQ) continue;

            yield return cell;
        }
    }

    public HashSet<Cell>[] GetValidCellsToTrain(int team)
    {
        var cells = new HashSet<Cell>();
        foreach (var item in Cells[team])
        {
            cells.UnionWith(item.GetAdustent());
        }

        var cellsToTrain = new HashSet<Cell>(AllCells);
        cellsToTrain.IntersectWith(cells);
        cellsToTrain.ExceptWith(Cells[team]);
        cellsToTrain.ExceptWith(Units[team].Keys);
        cellsToTrain.ExceptWith(Buildings[team].Keys);

        var ret = new[] { new HashSet<Cell>(), new HashSet<Cell>(), new HashSet<Cell>() };

        var enemyTeam = 1 - team;
        foreach (var x in cellsToTrain)
        {
            ret[2].Add(x);

            if (Units[enemyTeam].TryGetValue(x, out Unit unit))
            {
                if (unit.Level < 2)
                {
                    if (!Buildings[enemyTeam].ContainsKey(x))
                    {
                        ret[1].Add(x);
                    }
                }
            }
            else
            {
                if (!Buildings[enemyTeam].ContainsKey(x))
                {
                    ret[0].Add(x);
                    ret[1].Add(x);
                }
            }
        }

        return ret;
    }

    public double CalcScore(int team)
    {
        return Cells[team].Count
            + Cells[team].Count * -0.5
            + Gold[team] * 0.01
            + CalcUnitsPower(team)
            + CalcUnitsPower(1 - team) * -0.9
            + (Units[0].ContainsKey(World.EnemyHQ) ? 1000 : 0);
    }

    private int CalcUnitsPower(int team)
    {
        return Units[team].Values.Sum(x => x.Level * 10);
    }

    private int CalcUpkeepCost(int team)
    {
        return Units[team].Values.Sum(x => x.GetUpkeepCost());
    }

    public string Serialize()
    {
        using (var stream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Seed);

                writer.Write((byte)AllCells.Count);
                foreach (var x in AllCells) x.Serialize(writer);

                for (int team = 0; team < 2; team++)
                {
                    writer.Write((Int16)Gold[team]);
                    writer.Write((Int16)Income[team]);

                    writer.Write((byte)Cells[team].Count);
                    foreach (var c in Cells[team]) c.Serialize(writer);

                    writer.Write((byte)Buildings[team].Count);
                    foreach (var item in Buildings[team])
                    {
                        item.Key.Serialize(writer);
                        writer.Write((byte)item.Value);
                    }

                    writer.Write((byte)Units[team].Count);
                    foreach (var item in Units[team])
                    {
                        item.Key.Serialize(writer);
                        item.Value.Serialize(writer);
                    }
                }

                writer.Write((byte)MineSpots.Count);
                foreach (var item in MineSpots)
                {
                    item.Serialize(writer);
                }

                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }

    public void Load(string str)
    {
        using (var stream = new MemoryStream(Convert.FromBase64String(str)))
        {
            using (var reader = new BinaryReader(stream))
            {
                Seed = reader.ReadInt32();
                Rnd = new Random(Seed);

                AllCells.Clear();
                var n = reader.ReadByte();
                for (int i = 0; i < n; i++) AllCells.Add(Cell.Deserialize(reader));

                for (int team = 0; team < 2; team++)
                {
                    Gold[team] = reader.ReadInt16();
                    Income[team] = reader.ReadInt16();

                    Cells[team].Clear();
                    n = reader.ReadByte();
                    for (int j = 0; j < n; j++)
                    {
                        Cells[team].Add(Cell.Deserialize(reader));
                    }

                    Buildings[team].Clear();
                    n = reader.ReadByte();
                    for (int j = 0; j < n; j++)
                    {
                        var cell = Cell.Deserialize(reader);
                        var b = (BuildingType)reader.ReadByte();
                        Buildings[team][cell] = b;
                    }

                    Units[team].Clear();
                    n = reader.ReadByte();
                    for (int j = 0; j < n; j++)
                    {
                        var cell = Cell.Deserialize(reader);
                        var unit = Unit.Deserialize(reader);
                        Units[team][cell] = unit;
                    }
                }

                MineSpots.Clear();
                n = reader.ReadByte();
                for (int j = 0; j < n; j++)
                {
                    MineSpots.Add(Cell.Deserialize(reader));
                }

                World.EnemyHQ = Buildings[1].First(x => x.Value == BuildingType.HQ).Key;
            }
        }
    }
}

public enum TurnType
{
    BUILD = 0,
    MOVE = 1,
    TRAIN = 2,
    WAIT = 3
}

public class Turn
{
    public TurnType TurnType;
    public Unit Unit;
    public Cell Cell;
    public Building Building;

    public static Turn Train(Cell cell, int team, int level)
    {
        var unit = new Unit
        {
            Team = team,
            ID = cell.GetHashCode() * -1,
            Cell = cell,
            Level = level
        };

        return new Turn
        {
            TurnType = TurnType.TRAIN,
            Unit = unit,
            Cell = cell
        };
    }

    public static Turn Move(Unit unit, Cell cell)
    {
        return new Turn
        {
            TurnType = TurnType.MOVE,
            Unit = unit,
            Cell = cell
        };
    }

    public override string ToString()
    {
        switch (TurnType)
        {
            case TurnType.MOVE:
                return $"MOVE {Unit.ID} {Cell}";
            case TurnType.TRAIN:
                return $"TRAIN {Unit.Level} {Cell}";
            case TurnType.WAIT:
                return "WAIT";
            default:
                throw new NotImplementedException();
        }
    }
}