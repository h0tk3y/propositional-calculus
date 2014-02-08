using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PropositionalCalculus
{
    public class ProofOperator
    {
        public static List<Formula> AxiomSchemas = Formula.Parse(
            "A->B->A",
            "(A->B)->(A->B->C)->(A->C)",
            "A->B->A&B",
            "A&B->A",
            "A&B->B",
            "A->A|B",
            "B->A|B",
            "(A->C)->(B->C)->(A|B->C)",
            "(A->B)->(A->!B)->!A",
            "!!B->B"
            );

        #region Proof checking

        public static bool IsAxiom(Formula f)
        {
            return AxiomSchemas.Any(f.HasFormOf);
        }

        public static bool IsMP(List<Formula> proof, int index, HashSet<Formula> knownWhole,
            Dictionary<Formula, List<Implication>> knownRightOperand)
        {
            if (knownRightOperand.ContainsKey(proof[index]))
            {
                List<Implication> entries = knownRightOperand[proof[index]];
                return entries.Any(x => knownWhole.Contains(x.LeftOperand));
            }
            return false;
        }

        public static bool CheckProof(List<Formula> proof, StreamWriter output, List<Formula> assumptions = null)
        {
            var knownWhole = new HashSet<Formula>();
            var knownRightOperand = new Dictionary<Formula, List<Implication>>();
            for (int i = 0; i < proof.Count; i++)
            {
                bool statementIsLegal = IsAxiom(proof[i])
                                        || IsMP(proof, i, knownWhole, knownRightOperand)
                                        || assumptions != null && assumptions.Any(x => x.Equals(proof[i]));
                //nope
                if (!statementIsLegal)
                {
                    output.WriteLine("Statement {0} is illegal", i);
                    return false;
                }

                if (!knownWhole.Contains(proof[i]))
                    knownWhole.Add(proof[i]);

                var implication = proof[i] as Implication;
                if (implication != null)
                    if (knownRightOperand.ContainsKey(implication.RightOperand))
                        knownRightOperand[implication.RightOperand].Add(implication);
                    else
                        knownRightOperand.Add(implication.RightOperand, new List<Implication> { implication });
            }
            return true;
            StringBuilder sb = new StringBuilder();
            sb.a
        }

        #endregion

        #region Deduction conversion

        public static List<Formula> ConvertAssumptions(List<Formula> assumptions, List<Formula> formulas)
        {
            List<Formula> result = formulas;
            while (true)
            {
                var knownWhole = new HashSet<Formula>();
                var knownRightOperand = new Dictionary<Formula, List<Implication>>();
                Formula convertedAssumption = assumptions[assumptions.Count - 1];
                assumptions.RemoveAt(assumptions.Count - 1);
                foreach (var assumption in assumptions)
                {
                    if (!knownWhole.Contains(assumption))
                        knownWhole.Add(assumption);
                    Implication i = assumption as Implication;
                    if (i != null)
                        if (knownRightOperand.ContainsKey(i.RightOperand))
                            knownRightOperand[i.RightOperand].Add(i);
                        else
                            knownRightOperand.Add(i.RightOperand, new List<Implication> { i });
                }
                var step = new List<Assumption>();
                for (int i = 0; i < result.Count; i++)
                {
                    step.AddRange(ConvertDeductionLine(result, i, convertedAssumption, assumptions
                        , knownWhole, knownRightOperand));

                    if (!knownWhole.Contains(result[i]))
                        knownWhole.Add(result[i]);

                    var implication = result[i] as Implication;
                    if (implication != null)
                        if (knownRightOperand.ContainsKey(implication.RightOperand))
                            knownRightOperand[implication.RightOperand].Add(implication);
                        else
                            knownRightOperand.Add(implication.RightOperand, new List<Implication> { implication });
                }
                result = step.Select(x => x.Consequence).ToList();
                if (assumptions.Count == 0)
                    return result;
            }
        }

        public static List<Assumption> ConvertDeductionLine(List<Formula> formulas, int i, Formula convertedAssumption,
            List<Formula> otherAssumptions, HashSet<Formula> knownWhole,
            Dictionary<Formula, List<Implication>> knownRightOperand)
        {
            //is the converted assumption?
            if (formulas[i].Equals(convertedAssumption))
                return
                    DeductionAisA.Select(
                        x => new Assumption(otherAssumptions,
                            x.Inline(new Dictionary<PropositionalVariable, Formula>
                            {
                                {new PropositionalVariable("A"), formulas[i]}
                            }))).ToList();
            //is axiom or another assumption?
            if (IsAxiom(formulas[i])
                || otherAssumptions.Any(assumtion => formulas[i].Equals(assumtion)))
                return
                    DeductionAisAxiom.Select(
                        x =>
                            new Assumption(otherAssumptions, x.Inline(new Dictionary<PropositionalVariable, Formula>
                            {
                                {new PropositionalVariable("A"), formulas[i]},
                                {new PropositionalVariable("B"), convertedAssumption}
                            }))).ToList();
            //is modus ponens?
            List<Implication> mpCandidates = knownRightOperand[formulas[i]];
            Implication origin =
                mpCandidates.Find(x => knownWhole.Contains(x.LeftOperand) && x.RightOperand.Equals(formulas[i]));
            return
                DeductionAisMP.Select(
                    x =>
                        new Assumption(otherAssumptions, x.Inline(new Dictionary<PropositionalVariable, Formula>
                        {
                            {new PropositionalVariable("B"), convertedAssumption},
                            {new PropositionalVariable("L"), origin.LeftOperand},
                            {new PropositionalVariable("R"), formulas[i]}
                        }))).ToList();
        }

        #region Deduction proof

        private static readonly List<Formula> DeductionAisA = Formula.Parse(
            "A->(A->A)->A",
            "A->(A->A)",
            "(A->(A->A))->(A->((A->A)->A))->(A->A)",
            "(A->((A->A)->A))->(A->A)",
            "A->A"
            );

        private static readonly List<Formula> DeductionAisAxiom = Formula.Parse(
            "A",
            "A->(B->A)",
            "B->A"
            );

        private static readonly List<Formula> DeductionAisMP = Formula.Parse(
            "(B->L)->(B->L->R)->B->R",
            "(B->L->R)->B->R",
            "B->R"
            );

        #endregion

        #endregion

        #region Proof bulding

        public static List<Formula> BuildProof(Formula f)
        {
            var variables = f.Variables.ToArray();
            Array.Sort(variables);
            var result = new List<Formula>();
            for (int i = 0; i < 1 << variables.Length; ++i)
            {
                bool[] values = new bool[variables.Length];
                for (int j = variables.Length - 1; j >= 0; --j)
                    values[variables.Length - 1 - j] = ((i >> j) & 1) == 1;
                var variablesEvaluation = new Dictionary<PropositionalVariable, bool>();
                for (int j = 0; j < variables.Length; j++)
                    variablesEvaluation.Add(variables[j], values[j]);
                var partialProof = WriteProofInPartsEvaluation(f, variablesEvaluation);
                var assumptions =
                    variablesEvaluation.Select(x => (x.Value ? x.Key as Formula : new Inversion(x.Key))).ToList();
                result.AddRange(ConvertAssumptions(assumptions, partialProof));
            }
            for (int i = 0; i < variables.Length; ++i)
            {
                result.AddRange(Formula.InlineAll(ProofBuildingResources.TND, new Dictionary<PropositionalVariable, Formula>
                {
                    {"A", variables[i]}
                }));
                for (int j = 0; j < 1 << variables.Length - 1 - i; ++j)
                {
                    int otherVarsCount = variables.Length - 1 - i;
                    Formula fWithOtherVars = f;
                    bool[] values = new bool[otherVarsCount];
                    for (int k = 0; k<otherVarsCount; ++k)
                        values[k] = ((j >> k) & 1) == 1;
                    for (int k = 0; k < otherVarsCount; ++k)
                        fWithOtherVars = new Implication(
                            values[k]?
                                variables[variables.Length-1-k] as Formula:
                                new Inversion(variables[variables.Length-1-k]),
                           fWithOtherVars);
                    result.AddRange(Formula.InlineAll(ProofBuildingResources.AEx, new Dictionary<PropositionalVariable, Formula>
                    {
                        {"A", fWithOtherVars},
                        {"V", variables[i]}
                    }));
                }
            }
            return result;
        }

        public static List<Formula> WriteProofInPartsEvaluation(Formula f,
            Dictionary<PropositionalVariable, bool> variablesEvaluation)
        {
            List<Formula> result = new List<Formula>();
            BinaryOperation o = f as BinaryOperation;
            if (o != null)
            {
                result.AddRange(ProofOperator.WriteProofInPartsEvaluation(o.LeftOperand, variablesEvaluation));
                result.AddRange(ProofOperator.WriteProofInPartsEvaluation(o.RightOperand, variablesEvaluation));
                bool lEval = o.LeftOperand.Evaluate(variablesEvaluation),
                     rEval = o.RightOperand.Evaluate(variablesEvaluation);
                List<Formula> proof = null;
                if (!lEval)
                    if (!rEval)
                        if (o is Conjunction) proof = ProofBuildingResources.Cff;
                        else if (o is Disjunction) proof = ProofBuildingResources.Dff;
                        else if (o is Implication) proof = ProofBuildingResources.Iff;
                        else ;
                    else if (o is Conjunction) proof = ProofBuildingResources.Cft;
                    else if (o is Disjunction) proof = ProofBuildingResources.Dft;
                    else if (o is Implication) proof = ProofBuildingResources.Ift;
                    else ;
                else if (!rEval)
                    if (o is Conjunction) proof = ProofBuildingResources.Ctf;
                    else if (o is Disjunction) proof = ProofBuildingResources.Dtf;
                    else if (o is Implication) proof = ProofBuildingResources.Itf;
                    else ;
                else if (o is Conjunction) proof = ProofBuildingResources.Ctt;
                else if (o is Disjunction) proof = ProofBuildingResources.Dtt;
                else if (o is Implication) proof = ProofBuildingResources.Itt;
                else ;
                proof = Formula.InlineAll(proof, new Dictionary<PropositionalVariable, Formula>
                {
                    {"A", o.LeftOperand},
                    {"B", o.RightOperand}
                });
                result.AddRange(proof);
            }
            Inversion i = f as Inversion;
            if (i != null)
            {
                result.AddRange(ProofOperator.WriteProofInPartsEvaluation(i.Operand, variablesEvaluation));
                bool operandEval = i.Operand.Evaluate(variablesEvaluation);
                List<Formula> proof = operandEval ? ProofBuildingResources.Nt : ProofBuildingResources.Nf;
                proof = Formula.InlineAll(proof, new Dictionary<PropositionalVariable, Formula>
                {
                    {"A", i.Operand}
                });
                result.AddRange(proof);
            }
            return result;
        }

        #endregion
    }
}