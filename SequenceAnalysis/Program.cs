﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons;

namespace SequenceAnalysis
{
    public class Program
    {
        private static readonly char[] KnownLetters = {'A', 'C', 'G', 'T', 'N' };

        public static void Main(string[] args)
        {
            var fileName = @"G:\Projects\HumanGenome\Homo_sapiens.GRCh38.dna.chromosome.1.fa";
            if (args.Length > 1)
                fileName = args[0];
            var letterCount = KnownLetters.Length;
            var singleLetterFrequency = new CappedFrequencyCounter<char>(letterCount);
            var doubleLetterFrequency = new CappedFrequencyCounter<string>(letterCount * letterCount);
            var threeLetterFrequency = new CappedFrequencyCounter<string>(letterCount * letterCount * letterCount);
            var fiveLetterFrequency = new CappedFrequencyCounter<string>(letterCount * letterCount * letterCount * letterCount * letterCount);
            var tenLetterFrequency = new CappedFrequencyCounter<string>(100000);

            var letterBuffer = new CircularBuffer<char>(10);
            var baseIndex = 0uL;
            var lineIdx = 0;
            using (var streamReader = new StreamReader(fileName))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    lineIdx++;
                    if (line.StartsWith(">"))
                        continue;
                    if (lineIdx % (1000*1000) == 0)
                        Console.WriteLine($"Line {lineIdx}, base {baseIndex}");
                    foreach (var c in line)
                    {
                        if (!KnownLetters.Contains(c))
                            throw new Exception($"Unknown base {c}");
                        baseIndex++;
                        letterBuffer.Put(c);
                        singleLetterFrequency.Add(c);
                        if (baseIndex > 1)
                        {
                            var doubleLetter = new string(letterBuffer.Take(2).ToArray());
                            doubleLetterFrequency.Add(doubleLetter);
                        }
                        if (baseIndex > 2)
                        {
                            var threeLetter = new string(letterBuffer.Take(3).ToArray());
                            threeLetterFrequency.Add(threeLetter);
                        }
                        if (baseIndex > 4)
                        {
                            var fiveLetter = new string(letterBuffer.Take(5).ToArray());
                            fiveLetterFrequency.Add(fiveLetter);
                        }
                        if (baseIndex > 9)
                        {
                            var tenLetter = new string(letterBuffer.Take(10).ToArray());
                            tenLetterFrequency.Add(tenLetter);
                        }
                    }
                    if(lineIdx > 55*1000*1000)
                        break;
                }
            }
            File.WriteAllText("statistics.txt",
                singleLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a,b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + doubleLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + threeLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + fiveLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + tenLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine);
        }

        private static IEnumerable<IEnumerable<T>> GenerateCombination<T>(ICollection<T> letters, int level)
        {
            foreach (var letter in letters)
            {
                if (level == 1)
                    yield return new []{letter};
                else
                {
                    foreach (var subSequence in GenerateCombination(letters, level - 1))
                    {
                        yield return subSequence.Concat(new[] { letter });
                    }
                }
            }
        }
    }
}
