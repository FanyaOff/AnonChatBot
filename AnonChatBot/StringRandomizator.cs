using System;
using System.Linq;
using System.Text;

public class StringRandomizator
{
    public static string RandomizateString(string input)
    {
        var dict = new Dictionary<char, char>
        {
            ['а'] = 'a',
            ['с'] = 'c',
            ['е'] = 'e',
            ['о'] = 'o',
            ['р'] = 'p',
            ['х'] = 'x',
            ['А'] = 'A',
            ['В'] = 'B',
            ['С'] = 'C',
            ['Е'] = 'E',
            ['Н'] = 'H',
            ['К'] = 'K',
            ['М'] = 'M',
            ['О'] = 'O',
            ['Р'] = 'P',
            ['Х'] = 'X',
            ['Т'] = 'T',
        };
        double percentage = 0.7;
        var random = new Random();
        var result = new StringBuilder();

        foreach (char c in input)
        {
            if (dict.ContainsKey(c) && random.NextDouble() < percentage)
                result.Append(dict[c]);
            else
                result.Append(c);
        }
        return result.ToString();
    }
}