using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace all21
{
    class Program
    {
        static void Main(string[] args)
        {
            var usings = new List<string>();
            var sources = new List<string>();
            foreach (var f in Directory.EnumerateFiles(args[0], "*.cs").ToArray())
            {
                ReadSources(f, usings, sources);
            }

            Console.Write($"writting {args[1]}...");

            using (var w = File.CreateText(args[1]))
            {
                foreach (var x in usings.Distinct())
                {
                    w.WriteLine(x);
                }

                foreach (var x in sources)
                {
                    w.WriteLine(x);
                }
            }

            Console.WriteLine("done");
        }

        private static void ReadSources(string f, IList<string> usings, IList<string> sources)
        {
            Console.WriteLine($"reading {f}...");

            using (var reader = File.OpenText(f))
            {
                while (!reader.EndOfStream)
                {
                    var s = reader.ReadLine();
                    if (Regex.IsMatch(s, @"using\s[\w\.]+?;\s*$"))
                    {
                        usings.Add(s);
                    }
                    else
                    {
                        sources.Add(s);
                    }
                }
            }
        }
    }
}
