using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PropositionalCalculus;

namespace ParsingTests
{
    [TestClass]
    public class FormulaTests
    {

        [TestMethod]
        public void TestIsomorphism()
        {
            Formula template = Formula.Parse("((A&B|B&C)->(!!C))&A");
            Formula f = Formula.Parse("(((X&Y)&(G->H)|(G->H)&!X)->(!!!X))&(X&Y)");
            Assert.IsTrue(f.HasFormOf(template));
        }

        [TestMethod]
        public void TestProofChecking()
        {
            var proof = new List<Formula>
            {
                Formula.Parse("A->(A->A)->A"),
                Formula.Parse("A->(A->A)"),
                Formula.Parse("(A->(A->A))->(A->((A->A)->A))->(A->A)"),
                Formula.Parse("(A->((A->A)->A))->(A->A)"),
                Formula.Parse("A->A")
            };
            Assert.IsTrue(ProofOperator.CheckProof(proof, null));
        }

        [TestMethod]
        public void TestInline()
        {
            Formula f = Formula.Parse("A&B->!C");
            Assert.AreEqual(f.Inline(new Dictionary<PropositionalVariable, Formula>
            {
                {new PropositionalVariable("A"), Formula.Parse("X->Y")},
                {new PropositionalVariable("B"), Formula.Parse("V|U")},
                {new PropositionalVariable("C"), Formula.Parse("M&N&K")}
            }), Formula.Parse("(X->Y)&(V|U)->!(M&N&K)"));
        }

        [TestMethod]
        public void TestToString()
        {
            string representation = "!(A->B|C&D)";
            Formula f = Formula.Parse(representation);
            Assert.IsTrue(f.ToString() == representation);

            representation = "(A->B)->(A->B&C)->D|F";
            f = Formula.Parse(representation);
            Assert.IsTrue(f.ToString() == representation);

            f = new Assumption(new List<string> {"A", "B&C"}.Select(Formula.Parse).ToList(), Formula.Parse("A->B&C"));
            Assert.IsTrue(f.ToString() == "A, B&C|-A->B&C");
        }

        [TestMethod]
        public void TestDeductionConvert()
        {
            var proof = ProofOperator.ConvertAssumptions(Formula.Parse(new string[] {"A", "B"}),
                Formula.Parse("A", "B", "A->B->(A&B)", "B->(A&B)", "A&B")).ToList();
            Assert.IsTrue(ProofOperator.CheckProof(proof, new StreamWriter(Console.OpenStandardOutput())));
        }

        [TestMethod]
        public void TestEvaluation()
        {
            Dictionary<PropositionalVariable, bool> varEvaluations = new Dictionary<PropositionalVariable, bool>
            {
                {"A", true},
                {"B", false},
                {"C", true}
            };
            Assert.IsTrue(Formula.Parse("A|B").Evaluate(varEvaluations));
            Assert.IsFalse(Formula.Parse("A&B").Evaluate(varEvaluations));
            Assert.IsFalse(Formula.Parse("A->B").Evaluate(varEvaluations));
            Assert.IsFalse(Formula.Parse("A|B->B").Evaluate(varEvaluations));
            Assert.IsTrue(Formula.Parse("A|B->!B").Evaluate(varEvaluations));
            Assert.IsTrue(Formula.Parse("A->B|C").Evaluate(varEvaluations));
        }

        [TestMethod]
        public void TestVariablesGathering()
        {
            Formula f = Formula.Parse("G->B&(C|D)|!A&A");
            var variables = f.Variables;
            Assert.IsTrue(variables.Contains("A"));
            Assert.IsTrue(variables.Contains("B"));
            Assert.IsTrue(variables.Contains("C"));
            Assert.IsTrue(variables.Contains("D"));
            Assert.IsTrue(variables.Contains("G"));
        }

        [TestMethod]
        public void TestTautology()
        {
            Assert.IsTrue(Formula.Parse("A->A").IsTautology());
            Assert.IsTrue(Formula.Parse("(D->C->B)->(D->C->B)").IsTautology());
            Assert.IsFalse(Formula.Parse("B->A->B->C").IsTautology());
        }

        [TestMethod]
        public void TestProofInParts()
        {
            var formulas = ProofOperator.BuildProof("(A&A)|(!A&!A)");
            StreamWriter sw = new StreamWriter("out_please.txt");
            foreach (var formula in formulas)
            {
                sw.WriteLine(formula);
            }
            sw.Close();
        }

        [TestMethod]
        public void Rewrite()
        {
            StreamWriter sw = new StreamWriter("here_it_is_desu.txt");
            foreach (var formula in Formula.Parse(File.ReadAllLines("rewrite_pls.txt")))
                sw.WriteLine(formula);
            sw.Close();
        }

        //[TestMethod]
        public void WriteDown()
        {
            var result = new List<Formula>();
            Formula a = new PropositionalVariable("A");
            Formula b = new PropositionalVariable("B");
            result.Add(new Implication(new Implication(a, b), new Implication(new Implication(a, b), new Implication(a, b))));
            result.Add(new Implication(new Implication(new Implication(a, b), new Implication(new Implication(a, b), new Implication(a, b))), new Implication(new Implication(new Implication(a, b), new Implication(new Implication(new Implication(a, b), new Implication(a, b)), new Implication(a, b))), new Implication(new Implication(a, b), new Implication(a, b)))));
            result.Add(new Implication(new Implication(new Implication(a, b), new Implication(new Implication(new Implication(a, b), new Implication(a, b)), new Implication(a, b))), new Implication(new Implication(a, b), new Implication(a, b))));
            result.Add(new Implication(new Implication(a, b), new Implication(new Implication(new Implication(a, b), new Implication(a, b)), new Implication(a, b))));
            result.Add(new Implication(new Implication(a, b), new Implication(a, b)));
            result.Add(a);
            result.Add(new Implication(a, new Implication(new Implication(a, b), a)));
            result.Add(new Implication(new Implication(a, b), a));
            result.Add(new Implication(new Implication(new Implication(a, b), a), new Implication(new Implication(new Implication(a, b), new Implication(a, b)), new Implication(new Implication(a, b), b))));
            result.Add(new Implication(new Implication(new Implication(a, b), new Implication(a, b)), new Implication(new Implication(a, b), b)));
            result.Add(new Implication(new Implication(a, b), b));
            result.Add(new Implication(b, new Disjunction(new Inversion(a), b)));
            result.Add(new Implication(new Implication(b, new Disjunction(new Inversion(a), b)), new Implication(new Implication(a, b), new Implication(b, new Disjunction(new Inversion(a), b)))));
            result.Add(new Implication(new Implication(a, b), new Implication(b, new Disjunction(new Inversion(a), b))));
            result.Add(new Implication(new Implication(new Implication(a, b), b), new Implication(new Implication(new Implication(a, b), new Implication(b, new Disjunction(new Inversion(a), b))), new Implication(new Implication(a, b), new Disjunction(new Inversion(a), b)))));
            result.Add(new Implication(new Implication(new Implication(a, b), new Implication(b, new Disjunction(new Inversion(a), b))), new Implication(new Implication(a, b), new Disjunction(new Inversion(a), b))));
            result.Add(new Implication(new Implication(a, b), new Disjunction(new Inversion(a), b)));
            /*
            result.AddAll(new Inversion(new Inversion(a)).getParticularProof(hypos));
            */
            
            result.Add(new Implication(new Implication(new Inversion(a), a), new Implication(new Implication(new Inversion(a), new Inversion(a)), new Inversion(new Inversion(a)))));
            result.Add(new Implication(a, new Implication(new Inversion(a), a)));
            result.Add(a);
            result.Add(new Implication(new Inversion(a), a));
            result.Add(new Implication(new Implication(new Inversion(a), new Inversion(a)), new Inversion(new Inversion(a))));
            result.Add(new Implication(new Inversion(a), new Implication(new Inversion(a), new Inversion(a))));
            result.Add(new Implication(new Implication(new Inversion(a), new Implication(new Inversion(a), new Inversion(a))), new Implication(new Implication(new Inversion(a), new Implication(new Implication(new Inversion(a), new Inversion(a)), new Inversion(a))), new Implication(new Inversion(a), new Inversion(a)))));
            result.Add(new Implication(new Implication(new Inversion(a), new Implication(new Implication(new Inversion(a), new Inversion(a)), new Inversion(a))), new Implication(new Inversion(a), new Inversion(a))));
            result.Add(new Implication(new Inversion(a), new Implication(new Implication(new Inversion(a), new Inversion(a)), new Inversion(a))));
            result.Add(new Implication(new Inversion(a), new Inversion(a)));
            result.Add(new Inversion(new Inversion(a)));
            /*

            result.AddAll(new Disjunction(new Inversion(a), b).getParticularProof(hypos));
            */
            a = Formula.Parse("!A");
            result.Add(new Implication(a, new Implication(a, a)));
            result.Add(new Implication(new Implication(a, new Implication(a, a)), new Implication(new Implication(a, new Implication(new Implication(a, a), a)), new Implication(a, a))));
            result.Add(new Implication(new Implication(a, new Implication(new Implication(a, a), a)), new Implication(a, a)));
            result.Add(new Implication(a, new Implication(new Implication(a, a), a)));
            result.Add(new Implication(a, a));
            result.Add(new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))));
            result.Add(new Implication(new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))), new Implication(a, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))))));
            result.Add(new Implication(a, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))));
            result.Add(new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a)));
            result.Add(new Implication(new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a)), new Implication(a, new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a)))));
            result.Add(new Implication(a, new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a))));
            result.Add(new Implication(new Implication(a, a), new Implication(new Implication(a, new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a))), new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a)))));
            result.Add(new Implication(new Implication(a, new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a))), new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a))));
            result.Add(new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a)));
            result.Add(new Implication(new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a)), new Implication(new Implication(a, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))), new Implication(a, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))))));
            result.Add(new Implication(new Implication(a, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), a), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))), new Implication(a, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))));
            result.Add(new Implication(a, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))));
            result.Add(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)));
            result.Add(new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)))));
            result.Add(new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a))));
            result.Add(new Implication(new Implication(a, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a))), new Implication(new Implication(a, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))), new Implication(a, new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))));
            result.Add(new Implication(new Implication(a, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(a)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))), new Implication(a, new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))));
            result.Add(new Implication(a, new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))));
            result.Add(new Implication(b, new Implication(b, b)));
            result.Add(new Implication(new Implication(b, new Implication(b, b)), new Implication(new Implication(b, new Implication(new Implication(b, b), b)), new Implication(b, b))));
            result.Add(new Implication(new Implication(b, new Implication(new Implication(b, b), b)), new Implication(b, b)));
            result.Add(new Implication(b, new Implication(new Implication(b, b), b)));
            result.Add(new Implication(b, b));
            result.Add(new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))));
            result.Add(new Implication(new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))), new Implication(b, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))))));
            result.Add(new Implication(b, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))));
            result.Add(new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b)));
            result.Add(new Implication(new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b)), new Implication(b, new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b)))));
            result.Add(new Implication(b, new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b))));
            result.Add(new Implication(new Implication(b, b), new Implication(new Implication(b, new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b))), new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b)))));
            result.Add(new Implication(new Implication(b, new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b))), new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b))));
            result.Add(new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b)));
            result.Add(new Implication(new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b)), new Implication(new Implication(b, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))), new Implication(b, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))))));
            result.Add(new Implication(new Implication(b, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), b), new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))), new Implication(b, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))));
            result.Add(new Implication(b, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))));
            result.Add(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)));
            result.Add(new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)))));
            result.Add(new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b))));
            result.Add(new Implication(new Implication(b, new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b))), new Implication(new Implication(b, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))), new Implication(b, new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))));
            result.Add(new Implication(new Implication(b, new Implication(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Inversion(b)), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))), new Implication(b, new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))));
            result.Add(new Implication(b, new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))));
            result.Add(new Implication(new Implication(a, new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))), new Implication(new Implication(b, new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))), new Implication(new Disjunction(a, b), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))))));
            result.Add(new Implication(new Implication(b, new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))), new Implication(new Disjunction(a, b), new Inversion(new Conjunction(new Inversion(a), new Inversion(b))))));
            result.Add(new Implication(new Disjunction(a, b), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))));
            result.Add(new Implication(new Inversion(a), new Implication(new Inversion(b), new Conjunction(new Inversion(a), new Inversion(b)))));
            result.Add(new Implication(new Inversion(b), new Conjunction(new Inversion(a), new Inversion(b))));
            result.Add(new Conjunction(new Inversion(a), new Inversion(b)));
            result.Add(new Implication(new Conjunction(new Inversion(a), new Inversion(b)), new Implication(new Disjunction(a, b), new Conjunction(new Inversion(a), new Inversion(b)))));
            result.Add(new Implication(new Disjunction(a, b), new Conjunction(new Inversion(a), new Inversion(b))));
            result.Add(new Implication(new Implication(new Disjunction(a, b), new Conjunction(new Inversion(a), new Inversion(b))), new Implication(new Implication(new Disjunction(a, b), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))), new Inversion(new Disjunction(a, b)))));
            result.Add(new Implication(new Implication(new Disjunction(a, b), new Inversion(new Conjunction(new Inversion(a), new Inversion(b)))), new Inversion(new Disjunction(a, b))));
            result.Add(new Inversion(new Disjunction(a, b)));

            a = new PropositionalVariable("A");

            result.Add(new Implication(new Inversion(new Disjunction(new Inversion(a), b)), new Implication(new Implication(a, b), new Inversion(new Disjunction(new Inversion(a), b)))));
            result.Add(new Implication(new Implication(a, b), new Inversion(new Disjunction(new Inversion(a), b))));
            result.Add(new Implication(new Implication(new Implication(a, b), new Disjunction(new Inversion(a), b)), new Implication(new Implication(new Implication(a, b), new Inversion(new Disjunction(new Inversion(a), b))), new Inversion(new Implication(a, b)))));
            result.Add(new Implication(new Implication(new Implication(a, b), new Inversion(new Disjunction(new Inversion(a), b))), new Inversion(new Implication(a, b))));
            result.Add(new Inversion(new Implication(a, b)));
            StreamWriter sw = new StreamWriter("write-down.txt");
            foreach (var formula in result)
                sw.WriteLine(formula);
            {
                sw.Close();
            }
        }
    }
}
