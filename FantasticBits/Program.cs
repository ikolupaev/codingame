using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Entity
{
    public int id;
    public string type;
    public int x;
    public int y;
    public int vx;
    public int vy;
    public int state;
    public string Move;
}

class WizardAttributes
{
    public int nextThrowTurn = 0;
    public int nextCastTurn = 0;
    public string lastSpell = "";
    public int lastSpellSubjectId = -1;
}

class Player
{
    static int magic = 0;
    static int turn = 0;

    public static Entity goal = new Entity
    {
        x = 16000,
        y = 3750
    };

    static Entity goalToDefend = new Entity
    {
        x = 0,
        y = 3750
    };

    static Entity defendPoint = new Entity
    {
        x = 1000,
        y = 3750,
        vx = -1 // sign of vector if something coming to our goal
    };

    static List<Entity> entities = new List<Entity>(13);
    static List<Entity> wizards;
    static List<Entity> snaffles;
    static WizardAttributes[] wa = new WizardAttributes[13];

    static bool splitMode = false;

    static void Main(string[] args)
    {
        int myTeamId = int.Parse(Console.ReadLine()); // if 0 you need to score on the right of the map, if 1 you need to score on the left
        if (myTeamId == 1)
        {
            goal.x = 0;
            goalToDefend.x = 16000;
            defendPoint.x = 16000 - defendPoint.x;
            defendPoint.vx = 1;
        }

        for (var i = 0; i < 13; i++)
        {
            wa[i] = new WizardAttributes();
        }

        Console.Error.WriteLine($"goal to defend: {goalToDefend.x} {goalToDefend.y}");

        // game loop
        while (true)
        {
            Console.Error.WriteLine(magic);

            int entitiesCount = int.Parse(Console.ReadLine()); // number of entities still in game
            entities.Clear();

            for (int i = 0; i < entitiesCount; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');

                var e = new Entity
                {
                    id = int.Parse(inputs[0]), // entity identifier
                    type = inputs[1], // "WIZARD", "OPPONENT_WIZARD" or "SNAFFLE" (or "BLUDGER" after first league)
                    x = int.Parse(inputs[2]), // position
                    y = int.Parse(inputs[3]), // position
                    vx = int.Parse(inputs[4]), // velocity
                    vy = int.Parse(inputs[5]), // velocity
                    state = int.Parse(inputs[6]) // 1 if the wizard is holding a Snaffle, 0 otherwise
                };

                entities.Add(e);
            }

            wizards = entities.Where(x => x.type == "WIZARD").ToList();
            snaffles = entities.Where(x => x.type == "SNAFFLE").ToList();

            //DoPETRIFICUS();
            /*
                        if (snaffles.Count <= 3)
                        {
                            splitMode = true;
                        }
            */
            //Defend();

            DoThrows();

            DoFLIPPENDO();

            DoACCIO();

            DoMoves();

            foreach (var w in wizards.OrderBy(ww => ww.id))
            {
                Console.WriteLine(w.Move);
            }

            magic++;
            turn++;
        }
    }

    private static void Defend()
    {
        if (!splitMode) return;

        var w = wizards[0];

        foreach (var s in snaffles.Where(s => s.y > 1700 && s.y < 5700).OrderBy(s => (s.x + s.vx) * defendPoint.vx))
        {
            Console.Error.Write($"{s.id} {s.x} {s.vx} {goalToDefend.x} {defendPoint.vx}");

            //Console.Error.Write($"defend against {s.id} x:{s.x} vx: {s.vx}");

            var xd = Math.Abs(goalToDefend.x - (s.x + s.vx * 2));

            Console.Error.Write($" xd: {xd}");

            if (xd > 3000)
            {
                Console.Error.WriteLine($" too far");
                continue;
            }

            Console.Error.WriteLine();

            //if (magic >= 10 && < magic. 30s.vx * defendPoint.vx > 100)
            //{
            //    w.Move = $"PETRIFICUS {s.id}";
            //    snaffles.Remove(s);
            //    magic -= 10;
            //    wa[w.id].lastSpell = "PETRIFICUS";
            //    wa[w.id].nextCastTurn = turn + 2;
            //    wa[w.id].lastSpellSubjectId = w.id;
            //    return;
            //}

            if (magic >= 20)
            {
                if (SqDist(s, goalToDefend) > SqDist(w, goalToDefend))
                {
                    w.Move = $"FLIPENDO {s.id}";
                    snaffles.Remove(s);
                    magic -= 20;
                    wa[w.id].lastSpell = "FLIPENDO";
                    wa[w.id].nextCastTurn = turn + 3;
                    wa[w.id].lastSpellSubjectId = s.id;
                }
                else
                {
                    w.Move = $"ACCIO {s.id}";
                    snaffles.Remove(s);
                    magic -= 20;
                    wa[w.id].lastSpell = "ACCIO";
                    wa[w.id].nextCastTurn = turn + 3;
                    wa[w.id].lastSpellSubjectId = s.id;
                }
                return;
            }

            if (w.state == 0)
            {
                w.Move = $"MOVE {s.x + s.vx} {s.y + s.vy} 150";
            }
            else
            {
                w.Move = $"THROW {goal.x} {goal.y} 500";
            }
            return;
        }

        w.Move = $"MOVE {defendPoint.x} {defendPoint.y} 150";
    }

    private static void DoPETRIFICUS()
    {
        while (true)
        {
            if (magic < 30)
                return;

            var wizard = wizards.FirstOrDefault(w => w.Move == null);
            if (wizard == null)
                return;

            var lastSubjId = wa[wizard.id].lastSpellSubjectId;
            var nextCastTurn = wa[wizard.id].nextCastTurn;

            var sn = snaffles.Where(s => (s.id != lastSubjId || nextCastTurn < turn) && Math.Abs(s.vx) > 20 && IsMyGoal(s.id, s.x, s.vx, s.y, s.vy)).FirstOrDefault();
            if (sn == null)
                return;

            wizard.Move = $"PETRIFICUS {sn.id}";
            snaffles.Remove(sn);
            magic -= 10;
            wa[wizard.id].lastSpell = "PETRIFICUS";
            wa[wizard.id].nextCastTurn = turn + 2;
            wa[wizard.id].lastSpellSubjectId = sn.id;
        }
    }

    static double GetACCIOPower(Entity wizard, Entity subject)
    {
        var dist = Math.Sqrt(SqDist(wizard, subject)) / 1000;
        return Math.Min(3000 / (dist * dist), 1000);
    }

    private static void DoACCIO()
    {
        while (true)
        {
            var achios = GetAchios().OrderByDescending(x => x.Power);

            //foreach (var f in achios)
            //{
            //    Console.Error.WriteLine(f.ToString());
            //}

            if (magic < 20)
                return;


            var achio = achios
                            .Where(x => x.Power < 1000 && x.Power > 100)
                            .Where(x => Math.Abs(x.Wizard.x - x.Subject.x) > 1000)
                            .Where(x => Math.Abs(x.Wizard.x - goal.x) < Math.Abs(x.Subject.x - goal.x))
                            .FirstOrDefault();

            if (achio == null)
                return;

            achio.Wizard.Move = achio.Text;
            snaffles.Remove(achio.Subject);
            magic -= 20;
            wa[achio.Wizard.id].lastSpell = "ACCIO";
            wa[achio.Wizard.id].nextCastTurn = turn + 6;
            wa[achio.Wizard.id].lastSpellSubjectId = achio.Subject.id;
        }
    }

    private static bool IsMyGoal(int id, int ox, int vx, int oy, int vy)
    {
        var x = ox + vx * 3;
        var y = oy + vy * 3;

        var goal = false;

        if (y > 1800 && y < 5500)
        {
            if (goalToDefend.x > 8000 && x > goalToDefend.x)
            {
                goal = true;
            }
            else if (goalToDefend.x < 8000 && x < goalToDefend.x)
            {
                goal = true;
            }
        }

        //Console.Error.WriteLine($"{id} {x}+{vx} {y}+{vy} goal: {goal}");

        return goal;
    }

    private static void DoMoves()
    {
        while (true)
        {
            if (splitMode)
            {
                snaffles = entities.Where(x => x.type == "SNAFFLE" && Math.Abs(x.x - goal.x) < 9000).ToList();
                if (!snaffles.Any())
                {
                    var www = wizards.FirstOrDefault(w => w.Move == null);
                    if (www != null)
                        www.Move = $"MOVE 8000 {goal.y} 100 move to center";
                    return;
                }
            }

            if (!snaffles.Any())
            {
                snaffles = entities.Where(x => x.type == "SNAFFLE").ToList();
            }

            var ww = wizards.Where(w => w.Move == null)
                .Select(w => Tuple.Create(w, FindClosest(w, snaffles)))
                .OrderBy(w => w.Item2.Item2)
                .FirstOrDefault();

            if (ww == null)
                break;

            if (ww.Item1.state == 0)
            {
                var s = ww.Item2.Item1;
                ww.Item1.Move = $"MOVE {s.x + s.vx} {s.y + s.vy} 150";
                snaffles.Remove(s);
            }
            else
            {
                ww.Item1.Move = $"MOVE {goal.x} {goal.y} 150 move to goal";
            }
        }
    }

    private static void DoThrows()
    {
        while (true)
        {
            var wizard = wizards.FirstOrDefault(w => w.state == 1 && w.Move == null);

            if (wizard == null)
                break;

            /*
            if (wa[wizard.id].nextThrowTurn >= turn)
                break;
                */

            wa[wizard.id].nextThrowTurn = turn + 3;

            var y = goal.y;
            if( wizard.y > 2400 && wizard.y < 5000 )
            {
                y = wizard.y;
            }

            wizard.Move = $"THROW {goal.x} {y} 500 throw";

            //snaffles.Remove(snaffles.First(s => s.x == wizard.x && s.y == wizard.y));
        }
    }

    private static void DoFLIPPENDO()
    {
        while (true)
        {
            var toFLIPENDOs = GetGoalsWithFlipendo().OrderByDescending(e => e.Power).ToArray();

            foreach (var f in toFLIPENDOs)
            {
                Console.Error.WriteLine(f.ToString());
            }

            if (magic < 20)
                return;

            var toFLIPENDO = toFLIPENDOs.FirstOrDefault(x => x.Power > 300 && Math.Abs(x.Subject.x - goal.x) > 2000);
            if (toFLIPENDO == null)
            {
                break;
            }

            toFLIPENDO.Wizard.Move = toFLIPENDO.Text + " FLIPPENDO power: " + toFLIPENDO.Power.ToString();

            snaffles.Remove(toFLIPENDO.Subject);
            magic -= 20;
            wa[toFLIPENDO.Wizard.id].lastSpell = "FLIPPENDO";
            wa[toFLIPENDO.Wizard.id].nextCastTurn = turn + 3;
            wa[toFLIPENDO.Wizard.id].lastSpellSubjectId = toFLIPENDO.Subject.id;
        }
    }

    static Tuple<Entity, long> FindClosest(Entity item, IEnumerable<Entity> items)
    {
        return items.Select(x => Tuple.Create(x, SqDist(item, x))).OrderBy(x => x.Item2).First();
    }

    static Entity FindFarest(Entity item, IEnumerable<Entity> items)
    {
        return items.OrderByDescending(x => SqDist(item, x)).First();
    }

    static long SqDist(Entity a, Entity b)
    {
        var xx = a.x - b.x;
        var yy = a.y - b.y;

        return xx * xx + yy * yy;
    }

    static long SqDist(int x1, int y1, int x2, int y2)
    {
        var xx = x1 - x2;
        var yy = y1 - y2;

        return xx * xx + yy * yy;
    }

    public static IEnumerable<Spell> GetAchios()
    {
        foreach (var wizard in wizards.Where(w => w.state == 0 && w.Move == null))
        {
            foreach (var subj in snaffles)
            {
                if (!wa.Any(w => w.nextCastTurn >= turn && w.lastSpellSubjectId == subj.id))
                {
                    yield return new Spell
                    {
                        Power = GetACCIOPower(wizard, subj),
                        Wizard = wizard,
                        Subject = subj,
                        Text = $"ACCIO {subj.id}"
                    };
                }
            }
        }
    }

    public static IEnumerable<Spell> GetGoalsWithFlipendo()
    {
        foreach (var wizard in wizards.Where(w => w.state == 0 && w.Move == null))
        {
            foreach (var subj in snaffles)
            {
                var x = goal.x;

                var subjx = (int)(subj.x + subj.vx * 0.75);
                var subjy = (int)(subj.y + subj.vy * 0.75);

                var wizardx = (int)(wizard.x + wizard.vx * 0.75);
                var wizardy = (int)(wizard.y + wizard.vy * 0.75);

                if (Math.Abs(subjx - x) > Math.Abs(wizardx - x))
                {
                    continue;
                }

                var dist = Math.Sqrt(SqDist(subjx, subjy, wizardx, wizardy)) / 1000;
                var power = Math.Min(6000 / (dist * dist), 1000);

                var straight = IsStraight(wizardx, wizardy, subjx, subjy);
                var ricochet = IsRicochet(wizardx, wizardy, subjx, subjy);

                if (straight)
                {
                    yield return new Spell
                    {
                        Wizard = wizard,
                        Subject = subj,
                        Text = $"FLIPENDO {subj.id}",
                        Power = power,
                        Notes = "straight"
                    };
                }
                else if (ricochet)
                {
                    yield return new Spell
                    {
                        Wizard = wizard,
                        Subject = subj,
                        Text = $"FLIPENDO {subj.id}",
                        Power = power * 0.75,
                        Notes = "ricochet"
                    };
                }
                else if (entities.Where(e => e.type == "SNAFFLE").Count() < 4)
                {
                    yield return new Spell
                    {
                        Wizard = wizard,
                        Subject = subj,
                        Text = $"FLIPENDO {subj.id}",
                        Power = power * 0.9,
                        Notes = "defend"
                    };
                }
            }
        }
    }

    public static bool IsStraight(int wizardx, int wizardy, int subjx, int subjy)
    {
        double dx = subjx - wizardx;
        double dy = subjy - wizardy;
        var rate = dy / dx;

        var ddx = goal.x - subjx;
        var ddy = ddx * rate;
        var y = subjy + ddy;

        var yLow = goal.y - 1500;
        var yHigh = goal.y + 1500;

        return (yLow < y && yHigh > y);
    }

    public static bool IsRicochet(int wizardx, int wizardy, int subjx, int subjy)
    {
        double c = Math.Abs(wizardy - subjy);
        double d = Math.Abs(wizardx - subjx);

        double rate = c / d;

        double a = wizardy / rate;

        double lx = wizardx + a;
        double ly = lx * rate;

        double y = ly;

        double yLow = goal.y - 1500;
        double yHigh = goal.y + 1500;

        return (yLow < y && yHigh > y);
    }
}

class Spell
{
    public Entity Wizard;
    public Entity Subject;
    public string Text;
    public double Power;
    public string Notes;

    public override string ToString()
    {
        return $"w:{Wizard.id} {Text} p: {Power} notes: {Notes}";
    }
}