// Read inputs from Standard input.
// Write outputs to Standard output.

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
    static void Main(String[] args)
    {
        string line = Console.ReadLine();

        int lastState = 2;

        foreach (char ch in line.ToCharArray())
        {
            for (int i = 6; i >= 0; i--)
            {
                int state = ((ch >> i) & 1);

                if (lastState != state)
                {
                    Console.Write(' ');
                    Console.Write(new string('0', 2 - state));
                    Console.Write(' ');
                    lastState = state;
                }

                Console.Write('0');
            }
        }
    }
}