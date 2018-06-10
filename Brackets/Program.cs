using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
    static void Main(string[] args)
    {
        string expression = Console.ReadLine();

        var dic = new Dictionary<char, char> { { '[', ']' }, { '{', '}' }, { '(', ')' } };
        var closingStack = new Stack<char>();
        foreach (var ch in expression)
        {
            if (dic.ContainsKey(ch))
            {
                closingStack.Push(dic[ch]);
                continue;
            }

            if(dic.ContainsValue(ch))
            {
                if(!closingStack.Any() || closingStack.Pop() != ch)
                {
                    Console.WriteLine("false");
                    return;
                }
            }
        }

        Console.WriteLine( (closingStack.Count == 0).ToString().ToLower() );
    }
}