using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    static string[] grid;
    static int width;
    static int height;

    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');
        width = int.Parse(inputs[0]);
        height = int.Parse(inputs[1]);

        grid = new string[height];

        for (int i = 0; i < height; i++)
        {
            grid[i] = Console.ReadLine();
            Console.Error.WriteLine(grid[i]);
        }

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (grid[r][c] == '#') Console.Write('#');
                else
                {
                    var n = CalcAdjacent(r, c);
                    Console.Write(n);
                }
            }
            Console.WriteLine();
        }
    }

    private static object CalcAdjacent(int r, int c)
    {
        return
            CalcPassage(r - 1, c) +
            CalcPassage(r + 1, c) +
            CalcPassage(r, c + 1) +
            CalcPassage(r, c - 1);
    }

    private static int CalcPassage(int r, int c)
    {
        if (r < 0 || r > height - 1 || c < 0 || c > width - 1) return 0;
        return grid[r][c] == '0' ? 1 : 0;
    }
}