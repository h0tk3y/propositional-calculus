using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropositionalCalculus
{
    public abstract class Formula
    {
        private static readonly string[] BinaryOperationsSignsByPriority =
        {
            "->",
            "|",
            "&"
        };

        private static readonly Dictionary<string, BinaryOperationMaker> BinaryOperationMakers =
            new Dictionary<string, BinaryOperationMaker>
            {
                {"->", Implication.Maker},
                {"|", Disjunction.Maker},
                {"&", Conjunction.Maker}
            };

        private static readonly Dictionary<string, bool> BinaryOperationIsRightAssociative =
            new Dictionary<string, bool>
            {
                {"->", true},
                {"|", false},
                {"&", false}
            };

        public abstract HashSet<PropositionalVariable> Variables { get; } 
        public abstract bool Evaluate(Dictionary<PropositionalVariable, bool> variablesEvaluations);

        public bool IsTautology()
        {
            return FindContradiction() == null;
        }

        public Dictionary<PropositionalVariable, bool> FindContradiction()
        {
            PropositionalVariable[] vars = Variables.ToArray();
            Array.Sort(vars);
            for (int i = 0; i < 1 << vars.Length; ++i)
            {
                bool[] values = new bool[vars.Length];
                for (int j = vars.Length - 1; j >= 0; --j)
                    values[vars.Length - 1 - j] = ((i >> j) & 1) == 1;
                var variablesEvaluation = new Dictionary<PropositionalVariable, bool>();
                for (int j = 0; j < vars.Length; j++)
                    variablesEvaluation.Add(vars[j], values[j]);
                if (!Evaluate(variablesEvaluation)) return variablesEvaluation;
            }
            return null;
        }

        public abstract int Priority { get; }

        public abstract Formula Inline(Dictionary<PropositionalVariable, Formula> inlineFormulas);
        
        public bool HasFormOf(Formula that)
        {
            return HasFormOf(that, new Dictionary<Formula, Formula>());
        }
        public abstract bool HasFormOf(Formula that, Dictionary<Formula, Formula> variablesIsomorphism);

        public static List<Formula> InlineAll(List<Formula> to,
            Dictionary<PropositionalVariable, Formula> inlineFormulas)
        {
            return to.Select(x => x.Inline(inlineFormulas)).ToList();
        }

        public static implicit operator Formula(string s)
        {
            return Parse(s);
        }

        public static Formula Parse(string s)
        {
            s = RemoveBraces(s);
            foreach (var operation in BinaryOperationsSignsByPriority)
            {
                int operationSignIndex = FindUpperLevelOperation(s, operation);
                if (operationSignIndex != -1)
                {
                    string leftPart = s.Substring(0, operationSignIndex),
                            rightPart = s.Substring(operationSignIndex + operation.Length);
                    return BinaryOperationMakers[operation](Parse(leftPart), Parse(rightPart));
                }
            }
            if (s.StartsWith("!"))
                return new Inversion(Parse(s.Substring(1)));
            if (s.Length == 1 && s.ToUpper() == s) return new PropositionalVariable(s);
            throw new Exception("Invalid formula "+s);
        }
        private static int FindUpperLevelOperation(string expression, string operation)
        {
            int unclosedBraces = 0;
            for (int i = BinaryOperationIsRightAssociative[operation] ? 0 : expression.Length - 1; //TO or DOWNTO
                BinaryOperationIsRightAssociative[operation] ? i < expression.Length : i >= 0;
                i += BinaryOperationIsRightAssociative[operation] ? 1 : -1)
            {
                if (expression[i] == '(') unclosedBraces++;
                else if (expression[i] == ')') unclosedBraces--;
                else if (unclosedBraces == 0 && expression.Substring(i).StartsWith(operation)) return i;
            }
            return -1;
        }
        private static string RemoveBraces(string s)
        {
            while (s.StartsWith("(") && s.EndsWith(")"))
            {
                int braces = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '(') braces++;
                    else if (s[i] == ')') braces--;
                    if (i > 0 && i < s.Length - 1 && braces == 0) return s;
                }
                s = s.Substring(1, s.Length - 2);
            }
            return s;
        }

        public static List<Formula> Parse(params string[] s)
        {
            return s.Select(Parse).ToList();
        }

        private string _stringRepresentation;
        protected abstract string GetStringRepresentation();

        public override string ToString()
        {
            if (_stringRepresentation != null)
                return _stringRepresentation;
            return _stringRepresentation = GetStringRepresentation();
        }

        public override bool Equals(object obj)
        {
            if (obj is Formula)
                return obj.ToString().Equals(ToString());
            return obj.Equals(this);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    public class Assumption : Formula
    {
        public Assumption(List<Formula> assumptedFormulas, Formula consequence)
        {
            AssumptedFormulas = assumptedFormulas;
            Consequence = consequence;
        }

        public override HashSet<PropositionalVariable> Variables
        { get
        {
            return Consequence.Variables;
        }}

        public override bool Evaluate(Dictionary<PropositionalVariable, bool> variablesEvaluations)
        {
            return Consequence.Evaluate(variablesEvaluations);
        }

        public List<Formula> AssumptedFormulas { get; private set; }
        public Formula Consequence { get; private set; }

        public override int Priority { get { return 5; } }

        public override Formula Inline(Dictionary<PropositionalVariable, Formula> inlineFormulas)
        {
            List<Formula> assumptedFormulas = AssumptedFormulas.Select(x => x.Inline(inlineFormulas)).ToList();
            return new Assumption(assumptedFormulas, Consequence.Inline(inlineFormulas));
        }

        public override bool HasFormOf(Formula that, Dictionary<Formula, Formula> variablesIsomorphism)
        {
            return Equals(that);
        }

        protected override string GetStringRepresentation()
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < AssumptedFormulas.Count; i++)
                result.Append(AssumptedFormulas[i] + (i < AssumptedFormulas.Count - 1 ? ", " : ""));
            result.Append("|-" + Consequence.ToString());
            return result.ToString();
        }
    }

    public class PropositionalVariable : Formula, IComparable<PropositionalVariable>
    {
        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override HashSet<PropositionalVariable> Variables
        {
            get { return new HashSet<PropositionalVariable>{this};}
        }

        public override bool Evaluate(Dictionary<PropositionalVariable, bool> variablesEvaluations)
        {
            return variablesEvaluations[this];
        }

        public override int Priority { get { return 0; } }

        public override Formula Inline(Dictionary<PropositionalVariable, Formula> inlineFormulas)
        {
            return inlineFormulas.ContainsKey(this) ? inlineFormulas[this] : this;
        }

        public override bool HasFormOf(Formula that, Dictionary<Formula, Formula> variablesIsomorphism)
        {
            if (that is PropositionalVariable)
            {
                if (variablesIsomorphism.ContainsKey(that))
                    return variablesIsomorphism[that].Equals(this);
                variablesIsomorphism.Add(that, this);
                return true;
            }
            return false;
        }

        public PropositionalVariable(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public int CompareTo(PropositionalVariable other)
        {
            return String.Compare(Name, other.Name, System.StringComparison.Ordinal);
        }

        protected override string GetStringRepresentation()
        {
            return Name;
        }

        public static implicit operator PropositionalVariable(string s)
        {
            return new PropositionalVariable(s);
        }
    }

    public class Inversion : Formula
    {
        public override int GetHashCode()
        {
            return Operand != null ? Operand.GetHashCode() : 0;
        }

        public override HashSet<PropositionalVariable> Variables
        {
            get { return Operand.Variables; }
        }

        public override bool Evaluate(Dictionary<PropositionalVariable, bool> variablesEvaluations)
        {
            return !Operand.Evaluate(variablesEvaluations);
        }

        public override int Priority { get { return 1; } }

        public override Formula Inline(Dictionary<PropositionalVariable, Formula> inlineFormulas)
        {
            return new Inversion(Operand.Inline(inlineFormulas));
        }

        public override bool HasFormOf(Formula that, Dictionary<Formula, Formula> variablesIsomorphism)
        {
            if (that is PropositionalVariable)
            {
                if (variablesIsomorphism.ContainsKey(that))
                    return variablesIsomorphism[that].Equals(this);
                variablesIsomorphism.Add(that, this);
                return true;
            }
            return that is Inversion && Operand.HasFormOf((that as Inversion).Operand, variablesIsomorphism);
        }

        public Inversion(Formula operand)
        {
            this.Operand = operand;
        }

        public Formula Operand { get; private set; }

        protected override string GetStringRepresentation()
        {
            return "!"+(Operand.Priority <= Priority ? Operand.ToString() : "(" + Operand.ToString() + ")");
        }
    }

    public abstract class BinaryOperation : Formula
    {
        public override int GetHashCode()
        {
            unchecked
            {
                return ((LeftOperand != null ? LeftOperand.GetHashCode() : 0) * 397) ^ (RightOperand != null ? RightOperand.GetHashCode() : 0);
            }
        }

        public override HashSet<PropositionalVariable> Variables
        {
            get
            {
                var result = LeftOperand.Variables;
                result.UnionWith(RightOperand.Variables);
                return result;
            }
        }

        public override Formula Inline(Dictionary<PropositionalVariable, Formula> inlineFormulas)
        {
            return SameMaker(LeftOperand.Inline(inlineFormulas), RightOperand.Inline(inlineFormulas));
        }

        public override bool HasFormOf(Formula that, Dictionary<Formula, Formula> variablesIsomorphism)
        {
            if (that is PropositionalVariable)
            {
                if (variablesIsomorphism.ContainsKey(that))
                    return variablesIsomorphism[that].Equals(this);
                variablesIsomorphism.Add(that, this);
                return true;
            }
            var binaryOperation = that as BinaryOperation;
            return binaryOperation != null && (GetType() == that.GetType()
                                                       && LeftOperand.HasFormOf(binaryOperation.LeftOperand, variablesIsomorphism)
                                                       && RightOperand.HasFormOf(binaryOperation.RightOperand, variablesIsomorphism));
        }

        protected abstract string Sign { get; }

        protected virtual BinaryOperationMaker SameMaker { get { return null; } }
        protected BinaryOperation(Formula leftOperand, Formula rightOperand)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public Formula LeftOperand { get; private set; }
        public Formula RightOperand { get; private set; }

        protected override string GetStringRepresentation()
        {

            return (LeftOperand.Priority < Priority ? LeftOperand.ToString() : "(" + LeftOperand.ToString() + ")")
                   + Sign
                   + (RightOperand.Priority < Priority ? RightOperand.ToString() : "(" + RightOperand.ToString() + ")");
        }
    }

    public delegate BinaryOperation BinaryOperationMaker(Formula leftOperand, Formula rightOperand);

    public class Implication : BinaryOperation
    {
        public override bool Evaluate(Dictionary<PropositionalVariable, bool> variablesEvaluations)
        {
            return !LeftOperand.Evaluate(variablesEvaluations) || RightOperand.Evaluate(variablesEvaluations);
        }

        public Implication(Formula leftOperand, Formula rightOperand) : base(leftOperand, rightOperand) { }

        protected override string Sign { get { return "->"; } }

        protected override BinaryOperationMaker SameMaker { get { return Maker; } }

        public static readonly BinaryOperationMaker Maker =
            (leftOperand, rightOperand) => (new Implication(leftOperand, rightOperand));

        public override int Priority { get { return 4; } }
        protected override string GetStringRepresentation()
        {
            return (LeftOperand.Priority < Priority ? LeftOperand.ToString() : "("+LeftOperand.ToString()+")")
                + Sign
                + (RightOperand.Priority <= Priority ? RightOperand.ToString() : "("+RightOperand.ToString()+")");
        }
    }

    public class Conjunction : BinaryOperation
    {
        public override bool Evaluate(Dictionary<PropositionalVariable, bool> variablesEvaluations)
        {
            return LeftOperand.Evaluate(variablesEvaluations) && RightOperand.Evaluate(variablesEvaluations);
        }

        public Conjunction(Formula leftOperand, Formula rightOperand) : base(leftOperand, rightOperand) { }

        protected override BinaryOperationMaker SameMaker { get { return Maker; } }

        public static readonly BinaryOperationMaker Maker =
            (leftOperand, rightOperand) => (new Conjunction(leftOperand, rightOperand));

        public override int Priority { get { return 2; } }
        protected override string Sign { get { return "&"; } }
    }

    public class Disjunction : BinaryOperation
    {
        public override bool Evaluate(Dictionary<PropositionalVariable, bool> variablesEvaluations)
        {
            return LeftOperand.Evaluate(variablesEvaluations) || RightOperand.Evaluate(variablesEvaluations);
        }

        public Disjunction(Formula leftOperand, Formula rightOperand) : base(leftOperand, rightOperand) { }

        protected override BinaryOperationMaker SameMaker { get { return Maker; } }

        public static readonly BinaryOperationMaker Maker =
            (leftOperand, rightOperand) => (new Disjunction(leftOperand, rightOperand));

        public override int Priority { get { return 3; } }
        protected override string Sign { get { return "|"; } }
    }
}
