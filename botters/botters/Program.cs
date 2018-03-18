using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Made with love by AntiSquid, Illedan and Wildum.
 * You can help children learn to code while you participate by donating to CoderDojo.
 **/

class GameItem
{
    public string ItemName;// contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
    public int ItemCost; // BRONZE items have lowest cost, the most expensive items are LEGENDARY
    public int Damage;// keyword BLADE is present if the most important item stat is damage
    public int Health;
    public int MaxHealth;
    public int Mana;
    public int MaxMana;
    public int MoveSpeed; // keyword BOOTS is present if the most important item stat is moveSpeed
    public int ManaRegeneration;
    public int IsPotion; // 0 if it's not instantly consumed
}

class GameUnit
{
    public int UnitId;
    public int Team;
    public string UnitType; // UNIT, HERO, TOWER, can also be GROOT from wood1
    public int X;
    public int Y;
    public int AttackRange;
    public int Health;
    public int MaxHealth;
    public int Shield; // useful in bronze
    public int AttackDamage;
    public int MovementSpeed;
    public int StunDuration; // useful in bronze
    public int GoldValue;
    public int CountDown1; // all countDown and mana variables are useful starting in bronze
    public int CountDown2;
    public int CountDown3;
    public int Mana;
    public int MaxMana;
    public int ManaRegeneration;
    public string HeroType; // DEADPOOL, VALKYRIE, DOCTOR_STRANGE, HULK, IRONMAN
    public int IsVisible; // 0 if it isn't
    public int ItemsOwned; // useful from wood1

    public int Dist(GameUnit unit)
    {
        var xx = X - unit.X;
        var yy = Y - unit.Y;
        return (int)Math.Sqrt(xx * xx + yy * yy);
    }
}

class Player
{
    const string UNIT = "UNIT";
    const string HERO = "HERO";
    const string TOWER = "TOWER";
    const string GROOT = "GROOT";

    static GameUnit[] units;
    static int sideFactor = 1;
    static GameUnit hero, e_hero;
    static GameUnit[] enemies;

    static void Main(string[] args)
    {
        var itemsOwned = 0;
        string[] inputs;
        int myTeam = int.Parse(Console.ReadLine());
        if (myTeam == 0) sideFactor = -1;

        int bushAndSpawnPointCount = int.Parse(Console.ReadLine()); // usefrul from wood1, represents the number of bushes and the number of places where neutral units can spawn
        for (int i = 0; i < bushAndSpawnPointCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            string entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);
        }
        int itemCount = int.Parse(Console.ReadLine()); // useful from wood2
        var items = new List<GameItem>();
        for (int i = 0; i < itemCount; i++)
        {
            items.Add(new GameItem());

            var s = Console.ReadLine();
            d(s);
            inputs = s.Split(' ');

            items[i].ItemName = inputs[0]; // contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
            items[i].ItemCost = int.Parse(inputs[1]); // BRONZE items have lowest cost, the most expensive items are LEGENDARY
            items[i].Damage = int.Parse(inputs[2]); // keyword BLADE is present if the most important item stat is damage
            items[i].Health = int.Parse(inputs[3]);
            items[i].MaxHealth = int.Parse(inputs[4]);
            items[i].Mana = int.Parse(inputs[5]);
            items[i].MaxMana = int.Parse(inputs[6]);
            items[i].MoveSpeed = int.Parse(inputs[7]); // keyword BOOTS is present if the most important item stat is moveSpeed
            items[i].ManaRegeneration = int.Parse(inputs[8]);
            items[i].IsPotion = int.Parse(inputs[9]); // 0 if it's not instantly consumed
        }

        // game loop
        while (true)
        {
            int gold = int.Parse(Console.ReadLine());
            int enemyGold = int.Parse(Console.ReadLine());
            int roundType = int.Parse(Console.ReadLine()); // a positive value will show the number of heroes that await a command
            int entityCount = int.Parse(Console.ReadLine());

            units = new GameUnit[entityCount];

            for (int i = 0; i < entityCount; i++)
            {
                units[i] = new GameUnit();

                inputs = Console.ReadLine().Split(' ');

                units[i].UnitId = int.Parse(inputs[0]);
                units[i].Team = int.Parse(inputs[1]);
                units[i].UnitType = inputs[2]; // UNIT, HERO, TOWER, can also be GROOT from wood1
                units[i].X = int.Parse(inputs[3]);
                units[i].Y = int.Parse(inputs[4]);
                units[i].AttackRange = int.Parse(inputs[5]);
                units[i].Health = int.Parse(inputs[6]);
                units[i].MaxHealth = int.Parse(inputs[7]);
                units[i].Shield = int.Parse(inputs[8]); // useful in bronze
                units[i].AttackDamage = int.Parse(inputs[9]);
                units[i].MovementSpeed = int.Parse(inputs[10]);
                units[i].StunDuration = int.Parse(inputs[11]); // useful in bronze
                units[i].GoldValue = int.Parse(inputs[12]);
                units[i].CountDown1 = int.Parse(inputs[13]); // all countDown and mana variables are useful starting in bronze
                units[i].CountDown2 = int.Parse(inputs[14]);
                units[i].CountDown3 = int.Parse(inputs[15]);
                units[i].Mana = int.Parse(inputs[16]);
                units[i].MaxMana = int.Parse(inputs[17]);
                units[i].ManaRegeneration = int.Parse(inputs[18]);
                units[i].HeroType = inputs[19]; // DEADPOOL, VALKYRIE, DOCTOR_STRANGE, HULK, IRONMAN
                units[i].IsVisible = int.Parse(inputs[20]); // 0 if it isn't
                units[i].ItemsOwned = int.Parse(inputs[21]); // useful from wood1

                if (units[i].Team == myTeam && units[i].UnitType == HERO) hero = units[i];
            }

            if (roundType == -2 )
            {
                    Console.WriteLine("DOCTOR_STRANGE");
                    continue;
            }

            if (roundType == -1)
            {
                Console.WriteLine("IRONMAN");
                continue;
            }

            d($"hero health: {hero.Health}");
            d($"hero range: {hero.AttackRange}");

            var allignedX = units
                .Where(x => x.Team == myTeam && x.UnitType == UNIT)
                .GroupBy(x => x.X)
                .Select(x => new { X = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault()?.X;

            var tower = units.First(x => x.Team == myTeam && x.UnitType == TOWER);

            if (myTeam == 0)
                enemies = units.Where(x => x.Team == 1).ToArray();
            else
                enemies = units.Where(x => x.Team == 0).ToArray();

            e_hero = enemies.FirstOrDefault(x => x.UnitType == HERO);

            var closestEnemy = enemies
                .OrderBy(x => tower.Dist(x))
                .First();

            var farestUnit = units
                .Where(x => x.Team == myTeam)
                .OrderByDescending(x => tower.Dist(x))
                .First();

            var ex = closestEnemy.X + (hero.AttackRange) * sideFactor;
            if (tower.Dist(farestUnit) < tower.Dist(closestEnemy))
            {
                ex = farestUnit.X + 100 * sideFactor;
            }

            if (Math.Abs(tower.X - ex) > 1000)
            {
                ex = tower.X - 1000 * sideFactor;
            }

            if (hero.Health < 200)
            {
                ex = tower.X;
            }
            else if ( e_hero != null && e_hero.Health < 200 && e_hero.Health < hero.Health * 2)
            {
                Console.WriteLine($"ATTACK {e_hero.UnitId}");
                Console.WriteLine($"ATTACK {e_hero.UnitId}");
                continue;
            }

            if (ex == hero.X)
            {
                if (itemsOwned < 4 && hero.Health < hero.MaxHealth)
                {
                    var itemToBuy = items.Where(x => x.ItemCost <= gold && x.IsPotion == 1 && x.Health > 0)
                        .OrderByDescending(x => x.Health).FirstOrDefault();

                    //if (itemToBuy != null)
                    //{
                    //    itemToBuy = items.Where(x => x.ItemCost <= gold).OrderByDescending(x => x.MaxHealth).FirstOrDefault();
                    //}

                    if (itemToBuy != null)
                    {
                        if (itemToBuy.IsPotion == 0)
                            itemsOwned++;

                        d($"item {itemToBuy.ItemName} health: {itemToBuy.Health}");

                        Console.WriteLine($"BUY {itemToBuy.ItemName}");
                        Console.WriteLine($"BUY {itemToBuy.ItemName}");

                        //items.Remove(itemToBuy);

                        continue;
                    }
                }
            }

            Console.WriteLine($"MOVE_ATTACK {ex} {closestEnemy.Y} {closestEnemy.UnitId}");
            Console.WriteLine($"MOVE_ATTACK {ex} {closestEnemy.Y} {closestEnemy.UnitId}");
        }
    }

    static void d(object message)
    {
        Console.Error.WriteLine(message);
    }
}