using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Player
{
    static IAgent[] chain = new IAgent[]
    {
        new TrainToKillEnemies(),
        new TrainToExplore(),
        //new DefendHQ(),
        new Explore(),
        new AttackEnemies(),
        new AttackHQ()
    };

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
                if (owner == 0 && buildingType == BuildingType.HQ) World.MyHQ = cell;
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

            DoMoves(world);
        }
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

    public static void DoMoves(World world)
    {
        world.TurnMoves.Clear();

        foreach (var step in chain)
        {
            step.Handle(world);
        }

        Console.WriteLine(GetMovesStr(world.TurnMoves));
    }

    public static string GetMovesStr(IEnumerable<Turn> moves)
    {
        if (!moves.Any()) return "WAIT";
        else return string.Join(';', moves.Select(x => x.ToString()));
    }

    internal class AttackHQ : IAgent
    {
        public void Handle(World world)
        {
            var enemyHQ = world.Buildings[1].Where(x => x.Value == BuildingType.HQ).First().Key;
            var myUnits = world.Units[0].Values
                .Where(x => x.CanMove)
                .OrderByDescending(x => x.Level)
                .ToArray();

            foreach (var unit in myUnits)
            {
                world.MakeMoveTurn(0, unit, enemyHQ);
            }
        }
    }

    static void D(string message)
    {
        Console.Error.WriteLine(message);
    }
}

internal class Explore : IAgent
{
    public void Handle(World world)
    {
        var units = world.Units[0].Values.Where(x => x.Level == 1 && x.CanMove).ToList();
        var cells = world.GetValidCellsToTrain(0)[0];
        cells.ExceptWith(world.Cells[0]);

        var units1 = units.Select(x => (Unit: x, Cells: cells.Where(c => c.GetDistance(x.Cell) == 1).ToArray()))
            .OrderBy(x => x.Cells.Count())
            .ToArray();

        var used = new HashSet<Cell>();
        foreach (var uc in units1)
        {
            foreach (var c in uc.Cells)
            {
                if (!used.Contains(c))
                {
                    used.Add(c);
                    world.MakeMoveTurn(0, uc.Unit, c);
                    units.Remove(uc.Unit);
                    break;
                }
            }
        }
    }
}


internal class AttackEnemies : IAgent
{
    public void Handle(World world)
    {
        var units = world.Units[0].Values.Where(x => x.CanMove).ToArray();
        var enemies = world.Units[1].Values.ToList();

        foreach (var u in units)
        {
            var enemy = enemies.Where(x => x.Cell.IsAdustent(u.Cell)).FirstOrDefault();
            if (enemy != null)
            {
                world.MakeMoveTurn(0, u, enemy.Cell);
                enemies.Remove(enemy);
            }
        }
    }
}

internal class TrainToKillEnemies : IAgent
{
    public void Handle(World world)
    {
        var cells = world.GetValidCellsToTrain(0);

        cells[2].ExceptWith(cells[1]);
        cells[2].ExceptWith(cells[0]);

        cells[1].ExceptWith(cells[0]);

        cells[0].IntersectWith(world.Units[1].Keys);

        world.TrainAllAvailable(cells, 3);
        world.TrainAllAvailable(cells, 2);
        world.TrainAllAvailable(cells, 1);
    }
}
internal class TrainToExplore : IAgent
{
    public void Handle(World world)
    {
        if (world.Units[0].Count() < 15)
        {
            var cells = world.GetValidCellsToTrain(0);
            cells[0].ExceptWith(world.Cells[0]);
            world.TrainAllAvailable(cells, 1);
        }
    }
}

internal class DefendHQ : IAgent
{
    public void Handle(World world)
    {
        var hq = world.Buildings[0].First(x => x.Value == BuildingType.HQ).Key;

        var toProtect = new HashSet<Cell>(hq.GetAdustent());
        toProtect.IntersectWith(World.AllCells);

        var emptyToProtect = toProtect.Except(world.Units[0].Keys).ToArray();
        var availableUnits = new List<Unit>();

        foreach (var unit in world.Units[0].Values.Where(x => x.CanMove).ToArray())
        {
            if (toProtect.Contains(unit.Cell))
            {
                unit.CanMove = false;
            }
            else
            {
                availableUnits.Add(unit);
            }
        }

        foreach (var item in emptyToProtect)
        {
            if (!availableUnits.Any()) break;

            var unit = availableUnits
                .OrderBy(x => x.Cell.GetDistance(item))
                .FirstOrDefault();

            world.MakeMoveTurn(0, unit, item);
            availableUnits.Remove(unit);
        }
    }
}

internal interface IAgent
{
    void Handle(World world);
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

    public static int GetUpkeepCost(int level)
    {
        if (level == 1) return 1;
        if (level == 2) return 4;
        return 20;
    }

    public override string ToString()
    {
        return $"id:{ID} cell:{Cell} team:{Team} level:{Level}";
    }

    public bool CanMove
    {
        get => ID > 0;
        set => ID = Math.Abs(ID) * (value ? 1 : -1);
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
    public static Cell MyHQ;

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

    public void MakeMoveTurn(int team, Unit unit, Cell cell)
    {
        if (!unit.CanMove) throw new Exception("unit cannot move" + unit.ID.ToString());

        Units[1 - team].Remove(cell);
        Buildings[1 - team].Remove(cell);

        Units[team].Remove(unit.Cell);

        unit.Cell = cell;
        Units[team][unit.Cell] = unit;
        Cells[team].Add(unit.Cell);
        unit.CanMove = false;

        TurnMoves.Add(Turn.Move(unit, cell));
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

        var protectedCells = new HashSet<Cell>();
        foreach (var c in Buildings[1 - team])
        {
            if (c.Value == BuildingType.Tower)
            {
                protectedCells.UnionWith(c.Key.GetAdustent());
            }
        }

        foreach (var x in cell.GetAdustent())
        {
            if (x.Equals(World.EnemyHQ))
            {
                cells[0].Add(x);
                cells[1].Add(x);
                cells[2].Add(x);
                continue;
            }

            if (!AllCells.Contains(x)) continue;
            if (Cells[team].Contains(x)) continue;

            cells[2].Add(x);

            if (protectedCells.Contains(x)) continue;
            if (Buildings[1 - team].Keys.Contains(x)) continue;

            if (Units[1 - team].TryGetValue(x, out Unit unit))
            {
                if (unit.Level < 2)
                {
                    cells[1].Add(x);
                }
            }
        }
    }

    public IEnumerable<Cell> GetValidCellsToMove(Unit unit)
    {
        var protectedCells = new HashSet<Cell>();
        foreach (var c in Buildings[1 - unit.Team])
        {
            if (c.Value == BuildingType.Tower)
            {
                protectedCells.UnionWith(c.Key.GetAdustent());
            }
        }

        foreach (var cell in unit.Cell.GetAdustent())
        {
            if (!AllCells.Contains(cell)) continue;
            if (Buildings[unit.Team].ContainsKey(cell)) continue;
            if (Units[unit.Team].ContainsKey(cell)) continue;

            if (unit.Level == 3) yield return cell;

            if (protectedCells.Contains(cell)) continue;

            var opUnitExists = Units[1 - unit.Team].TryGetValue(cell, out Unit opUnit);
            if (opUnitExists && unit.Level < opUnit.Level) continue;

            var opBuildingsExists = Buildings[1 - unit.Team].TryGetValue(cell, out BuildingType opBuilding);
            if (opBuildingsExists && opBuilding != BuildingType.HQ) continue;

            yield return cell;
        }
    }

    public HashSet<Cell>[] GetValidCellsToTrain(int team)
    {
        var cells = new HashSet<Cell>(Cells[team]);
        foreach (var item in Cells[team])
        {
            cells.UnionWith(item.GetAdustent());
        }

        var cellsToTrain = new HashSet<Cell>(AllCells);
        cellsToTrain.IntersectWith(cells);
        cellsToTrain.ExceptWith(Units[team].Keys);
        cellsToTrain.ExceptWith(Buildings[team].Keys);

        var ret = new[] { new HashSet<Cell>(), new HashSet<Cell>(), new HashSet<Cell>() };

        var protectedCells = new HashSet<Cell>();
        foreach (var c in Buildings[1])
        {
            if (c.Value == BuildingType.Tower)
            {
                protectedCells.UnionWith(c.Key.GetAdustent());
            }
        }

        var enemyTeam = 1 - team;
        foreach (var x in cellsToTrain)
        {
            ret[2].Add(x);

            if (protectedCells.Contains(x)) continue;

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

    public int CalcUnitsPower(int team)
    {
        return Units[team].Values.Sum(x => x.Level * 10);
    }

    public int CalcUpkeepCost(int team)
    {
        return Units[team].Values.Sum(x => Unit.GetUpkeepCost(x.Level));
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
                World.MyHQ = Buildings[0].First(x => x.Value == BuildingType.HQ).Key;
            }
        }
    }

    public void TrainAllAvailable(HashSet<Cell>[] cells, int level)
    {
        while (cells[level - 1].Any() && Gold[0] >= level * 10 && Income[0] >= CalcUpkeepCost(0) + Unit.GetUpkeepCost(level))
        {
            var cc = cells[level - 1].OrderBy(x => x.GetDistance(World.MyHQ)).ToArray();
            var c = cc[World.Rnd.Next(cc.Length)];
            //var c = cc.First();
            AddTrainMove(0, cells, level, c);
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
                return $"MOVE {Math.Abs(Unit.ID)} {Cell}";
            case TurnType.TRAIN:
                return $"TRAIN {Unit.Level} {Cell}";
            case TurnType.WAIT:
                return "WAIT";
            default:
                throw new NotImplementedException();
        }
    }
}