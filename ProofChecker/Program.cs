using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PropositionalCalculus;

namespace ProofChecker
{
    public class FileProofCheсker
    {
        private static string AskForFile()
        {
            string result;
            do
            {
                Console.WriteLine("Enter the proof file name:");
                result = Console.ReadLine();
            } while (!File.Exists(result));
            return result;
        }

        private static void Main(string[] args)
        {
            string fileName = args.Length > 0 && File.Exists(args[0]) ? args[0] : AskForFile();
            Stopwatch s = new Stopwatch();
            s.Start();
            var proofToCheck =
                    (from line in File.ReadAllLines(fileName) select Formula.Parse(line)).ToList();
            Console.WriteLine("{0} formulas parsed in {1} ms", proofToCheck.Count, s.ElapsedMilliseconds);
            var output = new StreamWriter("output.txt");
            output.WriteLine(ProofOperator.CheckProof(proofToCheck, output, new List<Formula>{"A", "!A", "B", "!B"})
                ? "Proof is correct."
                : "Proof is incorrect.");
            output.Close();
        }
    }
}
