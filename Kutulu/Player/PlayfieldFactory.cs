using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class PlayfieldFactory
{
    public static Playfield Create(TextReader reader)
    {
        var width = int.Parse(reader.ReadLine());
        var height = int.Parse(reader.ReadLine());
        var map = new Playfield(width, height);
        for (int y = 0; y < height; y++)
        {
            string line = reader.ReadLine().Trim();
            var x = 0;
            foreach (var ch in line)
            {
                map.SetCell(x++, y, ch);
            }
        }

        return map;
    }
}
