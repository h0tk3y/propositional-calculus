using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PropositionalCalculus;

namespace DeductionConverter
{
    class Program
    {
        private static string AskForFile()
        {
            string result;
            do
            {
                Console.WriteLine("Enter the formulas file name:");
                result = Console.ReadLine();
            } while (!File.Exists(result));
            return result;
        }

        private static void Main(string[] args)
        {
            string fileName = args.Length > 0 && File.Exists(args[0]) ? args[0] : AskForFile();
            var lines = File.ReadAllLines(fileName).ToList();
            var assumptions = Formula.Parse(lines[0].Replace(" ", "").Split(','));
            lines.RemoveAt(0);
            List<Formula> result = ProofOperator.ConvertAssumptions(assumptions, Formula.Parse(lines.ToArray()));
            var output = new StreamWriter("output.txt");
            foreach (var formula in result)
                output.WriteLine(formula);
            output.Close();
        }
    }
}
