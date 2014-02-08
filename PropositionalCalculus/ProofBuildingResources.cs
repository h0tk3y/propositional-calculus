using System;
using System.Collections.Generic;
using System.IO;

namespace PropositionalCalculus
{

    public class ProofBuildingResources
    {
        public static readonly List<Formula> Nf = Formula.Parse(File.ReadAllLines("Nf_a.txt"));
        public static readonly List<Formula> Nt = Formula.Parse(File.ReadAllLines("Nt_a.txt"));
        public static readonly List<Formula> Cff = Formula.Parse(File.ReadAllLines("Cff_a.txt"));
        public static readonly List<Formula> Cft = Formula.Parse(File.ReadAllLines("Cft_a.txt"));
        public static readonly List<Formula> Ctf = Formula.Parse(File.ReadAllLines("Ctf_a.txt"));
        public static readonly List<Formula> Ctt = Formula.Parse(File.ReadAllLines("Ctt_a.txt"));
        public static readonly List<Formula> Dff = Formula.Parse(File.ReadAllLines("Dff_a.txt"));
        public static readonly List<Formula> Dft = Formula.Parse(File.ReadAllLines("Dft_a.txt"));
        public static readonly List<Formula> Dtf = Formula.Parse(File.ReadAllLines("Dtf_a.txt"));
        public static readonly List<Formula> Dtt = Formula.Parse(File.ReadAllLines("Dtt_a.txt"));
        public static readonly List<Formula> Iff = Formula.Parse(File.ReadAllLines("Iff_a.txt"));
        public static readonly List<Formula> Ift = Formula.Parse(File.ReadAllLines("Ift_a.txt"));
        public static readonly List<Formula> Itf = Formula.Parse(File.ReadAllLines("Itf_a.txt"));
        public static readonly List<Formula> Itt = Formula.Parse(File.ReadAllLines("Itt_a.txt"));
        public static readonly List<Formula> TND = Formula.Parse(File.ReadAllLines("TND.txt"));

        public static readonly List<Formula> AEx = Formula.Parse(new[]
        {
            "(V->A)->(!V->A)->V|!V->A",
            "(!V->A)->V|!V->A",
            "V|!V->A",
            "A"
        }
            );
    }
}