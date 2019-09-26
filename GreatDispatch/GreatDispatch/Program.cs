using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Box
{
    public float Weight;
    public float Volume;
    public int Track;
}

class Track
{
    public int Id;
    public float Weight;
    public float Volume;
}

class Player
{
    static void Main(string[] args)
    {
        int boxCount = int.Parse(Console.ReadLine());
        var boxes = new Box[boxCount];

        for (int i = 0; i < boxCount; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            boxes[i] = new Box
            {
                Weight = float.Parse(inputs[0]),
                Volume = float.Parse(inputs[1]),
                Track = -1
            };
        }

        var tracks = new Track[100];
        for (var i = 0; i < 100; i++)
        {
            tracks[i] = new Track { Id = i, Volume = 0, Weight = 0 };
        }

        foreach (var box in boxes.OrderByDescending(x => x.Weight))
        {
            var track = tracks
                .Where(x => x.Volume + box.Volume <= 100)
                .OrderBy(x => x.Weight)
                .FirstOrDefault();

            if (track != null)
            {
                box.Track = track.Id;
                track.Weight += box.Weight;
                track.Volume += box.Volume;
            }
        }

        var result = string.Join(" ", boxes.Select(x => x.Track.ToString()));
        
        Console.WriteLine(result);
    }
}