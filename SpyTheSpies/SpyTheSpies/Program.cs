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
        var s = Console.ReadLine();
        var enemies = s.Split(' ').ToHashSet();
        Console.Error.WriteLine(s);

        var people = new string[15];
        var peopleAttributes = new List<int>[15];
        var attributes = new Dictionary<string, int>();

        for (int i = 0; i < 15; i++)
        {
            s = Console.ReadLine();
            Console.Error.WriteLine(s);
            var inputs = s.Split(' ');
            people[i] = inputs[0];
            peopleAttributes[i] = new List<int>();

            var attrs = inputs.Skip(2).ToArray();
            foreach (var a in attrs)
            {
                if (!attributes.ContainsKey(a))
                {
                    attributes.Add(a, attributes.Count);
                }
                peopleAttributes[i].Add(attributes[a]);
            }
        }

        Console.Error.WriteLine("done");

        var titles = attributes.Keys.Concat(new[] { "spy" }).ToArray();
        var input = new List<List<LogicState>>();
        var output = new List<LogicState>();
        for (int i = 0; i < 15; i++)
        {
            input.Add(new List<LogicState>());

            for (var j = 0; j < titles.Length; j++)
            {
                input[i].Add(LogicState.False);
            }

            foreach (var j in peopleAttributes[i])
            {
                input[i][j] = LogicState.True;
            }

            var enemy = enemies.Any(x => x == people[i]);
            output.Add(enemy ? LogicState.True : LogicState.DontCare);
        }

        var table = new TruthTable(titles, input, output);

        Console.Error.WriteLine(table);

        var minimized = QuineMcCluskeyAlgorithm.MinimizeTruthTable(table);

        foreach (var x in minimized)
        {
            for (var i = 0; i < x.Count-1; i++)
            {
                if (x[i] == LogicState.True)
                {
                    Console.WriteLine(attributes.First(z => z.Value == i).Key);
                    continue;
                }

                if (x[i] == LogicState.False)
                {
                    Console.WriteLine("NOT " + attributes.First(z => z.Value == i).Key);
                    continue;
                }
            }
        }
    }
}

static class QuineMcCluskeyAlgorithm
{
    public static List<List<LogicState>> MinimizeTruthTable(TruthTable input)
    {
        if (input.OutputStates.Count != input.InputStates.Count)
        {
            throw new ArgumentException("Number of output and input states is not equal.");
        }
        else
        {

            List<List<LogicState>> trueRows = getRowsWithTrueOutput(input);

            bool minimized = false;
            while (!minimized && trueRows.Count > 0)
            {
                List<List<List<LogicState>>> sortedTrueRows = sortByNumberOfTruesOccuring(trueRows);
                List<List<bool>> handled = new List<List<bool>>();

                for (int i = 0; i < sortedTrueRows.Count; i++)
                {
                    handled.Add(new List<bool>());
                    for (int j = 0; j < sortedTrueRows[i].Count; j++)
                    {
                        // initialize with false
                        handled[i].Add(false);
                    }
                }

                List<List<LogicState>> newTrueRows = new List<List<LogicState>>();

                // go through each block of rows (every block has the same amount of "trues")
                for (int i = 0; i < sortedTrueRows.Count - 1; i++)
                {
                    // iterate through single block
                    for (int j = 0; j < sortedTrueRows[i].Count; j++)
                    {
                        // rows in the next block
                        for (int k = 0; k < sortedTrueRows[i + 1].Count; k++)
                        {
                            // only one field is different
                            if (numberOfDifferentFields(sortedTrueRows[i][j], sortedTrueRows[i + 1][k]) == 1)
                            {
                                handled[i][j] = true;
                                handled[i + 1][k] = true;

                                newTrueRows.Add(sortedTrueRows[i][j].Clone());
                                newTrueRows[newTrueRows.Count - 1][indexOfOnlyDifferentField(sortedTrueRows[i][j], sortedTrueRows[i + 1][k])] = LogicState.DontCare;
                            }
                        }
                    }
                }

                bool allNotHandled = true;

                for (int i = 0; i < handled.Count; i++)
                {
                    for (int j = 0; j < handled[i].Count; j++)
                    {
                        if (!handled[i][j])
                        {
                            newTrueRows.Add(sortedTrueRows[i][j].Clone());
                        }
                        else
                        {
                            allNotHandled = false;
                        }
                    }
                }

                if (allNotHandled)
                {
                    minimized = true;
                }

                trueRows = newTrueRows;
            }

            removeDoubles(trueRows);
            return PetricksMethod.RemoveNonEssentialPrimeImplicants(trueRows);
        }
    }

    private static int numberOfDifferentFields(List<LogicState> row1, List<LogicState> row2)
    {
        int count = 0;
        for (int i = 0; i < row1.Count; i++)
        {
            if (row1[i] != row2[i])
            {
                count++;
            }
        }
        return count;
    }

    private static int indexOfOnlyDifferentField(List<LogicState> row1, List<LogicState> row2)
    {
        for (int i = 0; i < row1.Count; i++)
        {
            if (row1[i] != row2[i])
            {
                return i;
            }
        }

        throw new InvalidOperationException("No different field has been found.");
    }

    private static List<List<LogicState>> getRowsWithTrueOutput(TruthTable input)
    {
        /* picks the rows with output equal to true
         * e.g.: 
         * 000|0
         * 001|1
         * 010|1
         * would return
         * 001|1
         * 010|1
         */


        List<List<LogicState>> output = new List<List<LogicState>>();

        for (int i = 0; i < input.OutputStates.Count; i++)
        {
            if (input.OutputStates[i] == LogicState.True)
            {
                output.Add(input.InputStates[i].Clone());
            }
        }

        return output;
    }

    private static List<List<List<LogicState>>> sortByNumberOfTruesOccuring(List<List<LogicState>> input)
    {
        List<List<List<LogicState>>> output = new List<List<List<LogicState>>>();
        for (int i = 0; i <= input[0].Count; i++)
        {
            output.Add(new List<List<LogicState>>());
        }

        for (int i = 0; i < input.Count; i++)
        {
            output[numberOfTruesOccuring(input[i])].Add(input[i].Clone());
        }

        return output;
    }

    private static int numberOfTruesOccuring(List<LogicState> inputRow)
    {
        int count = 0;
        for (int i = 0; i < inputRow.Count; i++)
        {
            if (inputRow[i] == LogicState.True)
            {
                count++;
            }
        }

        return count;
    }

    private static void removeDoubles(List<List<LogicState>> input)
    {
        for (int i = 0; i < input.Count; i++)
        {
            // compare row with all following
            for (int j = i + 1; j < input.Count; j++)
            {
                bool equal = true;
                // compare every segment of both rows
                for (int k = 0; k < input[i].Count; k++)
                {
                    if (input[i][k] != input[j][k])
                    {
                        equal = false;
                    }
                }

                if (equal)
                {
                    input.RemoveAt(j);
                    j--;
                }
            }
        }
    }
}

class TruthTable
{
    public string[] Titles;
    public List<List<LogicState>> InputStates;
    public List<LogicState> OutputStates;

    public TruthTable(string[] titles, List<List<LogicState>> inputStates, List<LogicState> outputStates)
    {
        this.Titles = titles;
        this.InputStates = inputStates;
        this.OutputStates = outputStates;
    }

    public void SetInputStates(List<List<LogicState>> newStates)
    {
        this.InputStates = newStates;
    }

    public void SetOutputStates(List<LogicState> newStates)
    {
        this.OutputStates = newStates;
    }

    public void SetOutputStatesToTrue()
    {
        int count = InputStates.Count;
        this.OutputStates = new List<LogicState>();
        for (int i = 0; i < count; i++)
        {
            this.OutputStates.Add(LogicState.True);
        }
    }

    public override string ToString()
    {
        StringBuilder output = new StringBuilder();
        for (int i = 0; i < Titles.Length; i++)
        {
            output.Append(Titles[i] + ((i < Titles.Length - 1) ? " " : ""));
        }

        output.Append("\r\n");


        for (int i = 0; i < InputStates.Count; i++)
        {
            for (int j = 0; j < InputStates[i].Count; j++)
            {
                output.Append(LogicStateToString(InputStates[i][j]) + " ");
            }
            output.Append(LogicStateToString(OutputStates[i]) + ((i < InputStates.Count - 1) ? "\r\n" : ""));
        }

        return output.ToString();
    }

    public static string LogicStateToString(LogicState state)
    {
        switch (state)
        {
            case LogicState.False:
                return "0";
            case LogicState.True:
                return "1";
            case LogicState.DontCare:
                return "X";
            default:
                throw new Exception("An unknown error occured.");
        }
    }
}

static class PetricksMethod
{
    public static List<List<LogicState>> RemoveNonEssentialPrimeImplicants(List<List<LogicState>> minifiedTruthTable)
    {
        if (minifiedTruthTable.Count == 0)
        {
            return minifiedTruthTable;
        }

        List<List<LogicState>> transposedTruthTable = transpose(minifiedTruthTable);

        List<PrimeImplicant> primeImplicantChart = new List<PrimeImplicant>();
        for (int i = 0; i < minifiedTruthTable.Count; i++)
        {
            primeImplicantChart.Add(new PrimeImplicant(minifiedTruthTable[i]));
        }

        List<List<PrimeImplicant>> chartEquationAndConnected = getChartEquation((int)Math.Pow(2, minifiedTruthTable[0].Count), primeImplicantChart);

        List<PrimeImplicant> requiredPrimeImplicants = getRequiredPrimeImplicants(chartEquationAndConnected);

        List<List<LogicState>> result = new List<List<LogicState>>();
        for (int i = 0; i < requiredPrimeImplicants.Count; i++)
        {
            result.Add(requiredPrimeImplicants[i].TruthTableRow);
        }
        return result;
    }

    // returns an equation like (K+L)(K+M)(L+N)(M+P)(N+Q)(P+Q)
    private static List<List<PrimeImplicant>> getChartEquation(int rows, List<PrimeImplicant> primeImplicantChart)
    {
        List<List<PrimeImplicant>> result = new List<List<PrimeImplicant>>();
        for (int i = 0; i < rows; i++)
        {
            result.Add(getPrimeImplicantsHandlingRow(i, primeImplicantChart));
        }
        return result;
    }

    private static List<PrimeImplicant> getPrimeImplicantsHandlingRow(int row, List<PrimeImplicant> primeImplicantChart)
    {
        List<PrimeImplicant> result = new List<PrimeImplicant>();
        for (int i = 0; i < primeImplicantChart.Count; i++)
        {
            if (primeImplicantChart[i].AffectedRows.Contains(row))
            {
                result.Add(primeImplicantChart[i]);
            }
        }
        return result;
    }

    // takes an equation like (K+L)(K+M)(L+N)(M+P)(N+Q)(P+Q)
    // expands the components to KKLMNP+KKLMNQ+... and returns the shortest
    private static List<PrimeImplicant> getRequiredPrimeImplicants(List<List<PrimeImplicant>> andConnectedEquation)
    {
        List<List<PrimeImplicant>> expanded = expand(andConnectedEquation);
        if (expanded.Count == 0)
        {
            return null;
        }
        List<PrimeImplicant> shortest = expanded[0];
        foreach (List<PrimeImplicant> term in expanded)
        {
            // remove duplicates
            for (int i = 0; i < term.Count; i++)
            {
                while (term.LastIndexOf(term[i]) != i)
                {
                    term.RemoveAt(term.LastIndexOf(term[i]));
                }
            }

            if (shortest.Count > term.Count)
            {
                shortest = term;
            }
        }
        return shortest;
    }

    private static List<List<PrimeImplicant>> expand(List<List<PrimeImplicant>> b)
    {
        List<List<PrimeImplicant>> result = new List<List<PrimeImplicant>>();
        if (b.Count <= 1)
        {
            for (int i = 0; i < b[0].Count; i++)
            {
                result.Add(new List<PrimeImplicant>() { b[0][i] });
            }
        }
        else
        {
            List<PrimeImplicant> head = b[0];
            List<List<PrimeImplicant>> body = b;
            body.Remove(head);

            List<List<PrimeImplicant>> bodyExpanded = expand(body.Clone());

            if (head.Count == 0)
            {
                return bodyExpanded;
            }

            for (int i = 0; i < head.Count; i++)
            {
                if (bodyExpanded.Count == 0)
                {
                    List<PrimeImplicant> term = new List<PrimeImplicant>();
                    term.Add(head[i]);
                    result.Add(term);
                }
                else
                {
                    for (int j = 0; j < bodyExpanded.Count; j++)
                    {
                        List<PrimeImplicant> term = new List<PrimeImplicant>();
                        term.Add(head[i]);
                        term.AddRange(bodyExpanded[j]);
                        result.Add(term);
                    }
                }
            }
        }
        return result;
    }

    private static List<List<LogicState>> transpose(List<List<LogicState>> table)
    {
        List<List<LogicState>> transposed = new List<List<LogicState>>();

        if (table.Count == 0)
        {
            return table;
        }

        for (int i = 0; i < table[0].Count; i++)
        {
            transposed.Add(new List<LogicState>());
            for (int j = 0; j < table.Count; j++)
            {
                transposed[i].Add(table[j][i]);
            }
        }

        return transposed;
    }
}

class PrimeImplicant
{
    public readonly List<int> AffectedRows;
    public readonly List<LogicState> TruthTableRow;

    public PrimeImplicant(List<LogicState> truthTableRow)
    {
        this.TruthTableRow = truthTableRow;
        this.AffectedRows = getAffectedRowsFromTruthTableRow(truthTableRow);
    }

    private List<int> getAffectedRowsFromTruthTableRow(List<LogicState> truthTableRow)
    {
        List<int> affectedRows = getPossibleRowsForTruthTable(truthTableRow.Count);
        for (int i = 0; i < affectedRows.Count; i++)
        {
            if (!andConjunction(intToRow(affectedRows[i], truthTableRow.Count), truthTableRow))
            {
                affectedRows.RemoveAt(i);
                i--;
            }
        }
        return affectedRows;
    }

    private List<int> getPossibleRowsForTruthTable(int columns)
    {
        List<int> rows = new List<int>();
        for (int i = 0; i < Math.Pow(2, columns); i++)
        {
            rows.Add(i);
        }
        return rows;
    }

    private List<LogicState> intToRow(int x, int columns)
    {
        List<LogicState> row = new List<LogicState>(columns);
        for (int i = columns - 1; i >= 0; i--)
        {
            row.Add(((x & (1 << i)) == 0) ? LogicState.False : LogicState.True);
        }
        return row;
    }

    private bool andConjunction(List<LogicState> a, List<LogicState> b)
    {
        if (a.Count != b.Count)
        {
            throw new ArgumentException();
        }

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] == b[i] || a[i] == LogicState.DontCare || b[i] == LogicState.DontCare)
            {
                continue;
            }

            return false;
        }
        return true;
    }
}

static class Extensions
{
    public static List<T> Clone<T>(this List<T> list)
    {
        List<T> clone = new List<T>();
        for (int i = 0; i < list.Count; i++)
        {
            clone.Add(list[i]);
        }

        return clone;
    }

}

enum LogicState : byte
{
    False = 0,
    True = 1,
    DontCare = 2
}
