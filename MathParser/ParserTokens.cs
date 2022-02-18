namespace MathParser
{
    public class ParserToken
    {
        public readonly string Token;
        public readonly int Order;
        public readonly bool Executable;
        public virtual double Eval(params double[] args) => throw new NotImplementedException();
        public override string ToString() => Token;

        public static implicit operator string(ParserToken t) => t.Token;

        public ParserToken(string token, int order = -1, bool executable = false)
        {
            Token = token.ToUpper();
            Executable = executable;
            Order = order;
        }
    }

    public class Variable : ParserToken
    {
        public double Value;
        public override double Eval(params double[] args) =>
            double.IsNaN(Value) ? throw new Parser.EvaluatingException($"Cannot evaluate expression, '{Token}' is not defined") : Value;

        public static implicit operator double(Variable v) => v.Value;

        public Variable(string name, double value = double.NaN) : base(name, -1, false)
        {
            Value = value;
        }
    }

    public class Constant : Variable
    {
        public override double Eval(params double[] args) => Value;

        public Constant(double value) : base("_", value)
        {
            Value = value;
        }

        public override string ToString() => $"'{Value:F2}'";
    }

    public class BinaryOperator : ParserToken
    {
        private Func<double, double, double> Func;
        public override double Eval(params double[] args) => Func(args[0], args[1]);

        public BinaryOperator(string name, Func<double, double, double> func, int order) : base(name, order, true)
        {
            Func = func;
        }


        public static BinaryOperator SelectOperator(char op) => op switch
        {
            '+' => AddOper(),
            '-' => SubOper(),
            '*' => MulOper(),
            '/' => DivOper(),
            '^' => PowOper(),
            _ => throw new ArgumentException("Unknown operator")
        };
   

        public static BinaryOperator AddOper() => 
            new BinaryOperator("+", (double a, double b) => a + b, 1);
        public static BinaryOperator SubOper() =>
            new BinaryOperator("-", (double a, double b) => a - b, 1);
        public static BinaryOperator MulOper() =>
            new BinaryOperator("*", (double a, double b) => a * b, 2);
        public static BinaryOperator DivOper() =>
            new BinaryOperator("/", (double a, double b) => a / b, 2);
        public static BinaryOperator PowOper() =>
            new BinaryOperator("^", (double a, double b) => Math.Pow(a, b), 3);
    }
}
