using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropositionalCalculus;

namespace ProofBuilder
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

        static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("output.txt");
            string fileName = args.Length > 0 && File.Exists(args[0]) ? args[0] : AskForFile();
            Formula f = Formula.Parse(File.ReadAllLines(fileName)[0]);
            var contradiction = f.FindContradiction();
            if (contradiction != null)
            {
                StringBuilder s = new StringBuilder("Statement is false when ");
                int c = 0;
                foreach (var v in contradiction.Keys)
                    s.Append(v + "=" + contradiction[v] + (++c < contradiction.Keys.Count ? ", " : ""));
                output.WriteLine(s.ToString());
            }
            else
            {
                var proof = ProofOperator.BuildProof(f);
                foreach (var formula in proof)
                    output.WriteLine(formula);
            }
            output.Close();
        }
    }
}
