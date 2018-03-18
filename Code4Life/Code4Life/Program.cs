using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    const string SAMPLES = "SAMPLES";
    const string DIAGNOSIS = "DIAGNOSIS";
    const string MOLECULES = "MOLECULES";
    const string LABORATORY = "LABORATORY";

    const int A = 0;
    const int B = 1;
    const int C = 2;
    const int D = 3;
    const int E = 4;

    static string[] molecules = new[] { "A", "B", "C", "D", "E" };

    class Sample
    {
        public int Id;
        public int CarriedBy;
        public int Health;
        public int[] Costs = new int[5];

        public override string ToString()
        {
            return
                $"{Id}; have: {AllHave}; avail: {AllAvailable}; " +
                String.Join(" ", Costs.Select((x, i) => $"{molecules[i]}: {x}/{StillRequired[i]}"));
        }

        public bool IsDiagnosted() => Costs.Any(x => x >= 0);

        public int[] StillRequired = new int[5];
        public bool AllAvailable;
        public bool AllHave;
    }

    class Robot
    {
        public string Target;
        public int Eta;
        public int Score;
        public int[] Storage = new int[5];
        public int[] Expertise = new int[5];
    }

    static int[] available = new int[5];
    static Robot[] robots = new[] { new Robot(), new Robot() };
    static Sample[] mySamples;
    static Sample[] allSamples;
    static void Main(string[] args)
    {
        string[] inputs;
        int projectCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < projectCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int a = int.Parse(inputs[0]);
            int b = int.Parse(inputs[1]);
            int c = int.Parse(inputs[2]);
            int d = int.Parse(inputs[3]);
            int e = int.Parse(inputs[4]);
        }

        while (true)
        {
            for (int i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                robots[i].Target = inputs[0];
                robots[i].Eta = int.Parse(inputs[1]);
                robots[i].Score = int.Parse(inputs[2]);

                for (int j = 0; j < 5; j++)
                    robots[i].Storage[j] = int.Parse(inputs[j + 3]);

                for (int j = 0; j < 5; j++)
                    robots[i].Expertise[j] = int.Parse(inputs[j + 8]);
            }

            inputs = Console.ReadLine().Split(' ');
            for (int j = 0; j < 5; j++)
                available[j] = int.Parse(inputs[j]);

            int sampleCount = int.Parse(Console.ReadLine());
            var samples = new Sample[sampleCount];
            allSamples = samples;

            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = new Sample();

                inputs = Console.ReadLine().Split(' ');
                samples[i].Id = int.Parse(inputs[0]);
                samples[i].CarriedBy = int.Parse(inputs[1]);
                int rank = int.Parse(inputs[2]);
                string expertiseGain = inputs[3];
                samples[i].Health = int.Parse(inputs[4]);
                samples[i].AllAvailable = true;
                samples[i].AllHave = true;

                for (int j = 0; j < 5; j++)
                {
                    samples[i].Costs[j] = int.Parse(inputs[j + 5]);
                    if (samples[i].Costs[j] >= 0)
                    {
                        samples[i].StillRequired[j] = samples[i].Costs[j] - robots[0].Storage[j] - robots[0].Expertise[j];
                        if (samples[i].StillRequired[j] < 0)
                        {
                            samples[i].StillRequired[j] = 0;
                        }

                        if (samples[i].StillRequired[j] > available[j])
                        {
                            samples[i].AllAvailable = false;
                        }

                        if (samples[i].StillRequired[j] > 0)
                        {
                            samples[i].AllHave = false;
                        }
                    }
                    else
                    {
                        samples[i].AllAvailable = false;
                        samples[i].AllHave = false;
                    }
                }
                if (samples[i].CarriedBy == 0)
                {
                    Console.Error.WriteLine(samples[i]);
                }
            }

            if (robots[0].Eta > 0)
            {
                Console.WriteLine("WAIT");
                continue;
            }

            mySamples = samples.Where(x => x.CarriedBy == 0).ToArray();

            //Console.Error.WriteLine(robots[0].Target);

            switch (robots[0].Target)
            {
                case "START_POS":
                    Console.WriteLine($"GOTO {SAMPLES}");
                    break;
                case SAMPLES:
                    HandleSamples();
                    break;
                case DIAGNOSIS:
                    HandleDiagnosis();
                    break;
                case MOLECULES:
                    HandleMolecules();
                    break;
                case LABORATORY:
                    HandleLaboratory();
                    break;
            }
        }
    }

    private static void HandleLaboratory()
    {
        var sample = mySamples.FirstOrDefault(x => x.AllHave);
        if (sample != null)
        {
            Console.WriteLine($"CONNECT {sample.Id}");
            return;
        }

        if (mySamples.Any(x => x.AllAvailable))
        {
            Console.WriteLine("GOTO MOLECULES");
            return;
        }

        //todo: sometiems go to diagnosys
        Console.WriteLine("GOTO SAMPLES");
    }

    private static void HandleMolecules()
    {
        if (mySamples.Any(x => x.AllHave))
        {
            Console.WriteLine($"GOTO {LABORATORY}");
            return;
        }

        if (robots[0].Storage.Sum() < 10)
        {
            var c = mySamples
                .Where(x => x.AllAvailable && !x.AllHave)
                .OrderBy(x => x.StillRequired.Sum())
                .FirstOrDefault()
                ?.StillRequired.Select((r, i) => new { Req = r, Id = i })
                ?.Where(x => x.Req > 0)
                ?.OrderBy(x => x.Req)
                ?.FirstOrDefault();

            if (c != null)
            {
                Console.WriteLine($"CONNECT {molecules[c.Id]}");
                return;
            }
        }

        Console.WriteLine($"GOTO {DIAGNOSIS}");
    }

    private static void HandleDiagnosis()
    {
        if (mySamples.Any(x => x.AllHave))
        {
            Console.WriteLine($"GOTO {LABORATORY}");
            return;
        }

        var undiag = mySamples.FirstOrDefault(x => !x.IsDiagnosted());
        if (undiag != null)
        {
            Console.WriteLine($"CONNECT {undiag.Id} {undiag}");
            return;
        }

        var mySum = robots[0].Storage.Sum();
        if (mySamples.Any(x => x.AllAvailable && mySum + x.StillRequired.Sum() <= 10))
        {
            Console.WriteLine($"GOTO {MOLECULES} {mySum}");
            return;
        }

        if (mySamples.Length < 3)
        {
            var cloudSample = allSamples
                .Where(x => x.CarriedBy == -1 && x.AllHave)
                .OrderByDescending(x => x.Health)
                .FirstOrDefault();

            if (cloudSample != null)
            {
                Console.WriteLine($"CONNECT {cloudSample.Id} cloud: {cloudSample}");
                return;
            }
        }

        if (!mySamples.Any())
        {
            Console.WriteLine($"GOTO {SAMPLES}");
            return;
        }

        var unavail = mySamples.FirstOrDefault(x => !x.AllAvailable || mySum + x.StillRequired.Sum() > 10);
        if (unavail != null)
        {
            Console.WriteLine($"CONNECT {unavail.Id} unavail: {unavail}");
            return;
        }
    }

    static void HandleSamples()
    {
        if (mySamples.Length < 3)
        {
            var mySum = robots[0].Storage.Sum();

            if (mySum > 9)
                Console.WriteLine($"CONNECT 1");
            else
                Console.WriteLine($"CONNECT 2");

            return;
        }

        Console.WriteLine($"GOTO DIAGNOSIS");
    }
}