namespace MathParser
{
    public class EvaluatingException : Exception
    {
        public EvaluatingException(string msg) : base(msg) { }
    }

    public class MathException : EvaluatingException
    {
        public double Left, Right;
        public string Operator;
        public MathException(string err, double left, double right, string op) :
            base($"Cannot execute {left}{op}{right} ({err})")
        {
            Left = left; ;
            Right = right;
            Operator = op;
        }
    }

    public class ParsingException : Exception
    {
        public string Expression;

        public ParsingException(string expr) : base($"Cannot parse '{expr}'")
        {
            Expression = expr;
        }

        public ParsingException(string expr, string msg) : base(msg)
        {
            Expression = expr;
        }
    }

    public class WrongSymbolException : ParsingException
    {
        public int Position;
        public char FailedChar;

        public WrongSymbolException(string expr, int pos) :
            base(expr, $"Cannot parse '{expr}': unacceptable character '{expr[pos]}' found at position {pos}")
        {
            FailedChar = expr[pos];
            Position = pos;
        }

        public WrongSymbolException(string expr, char failed) :
            base(expr, $"Cannot parse '{expr}': unacceptable character '{failed}' found in expression")
        {
            Position = expr.IndexOf(failed);
            FailedChar = failed;
        }
    }

    public class SyntaxException : ParsingException
    {
        public int Position;
        public string SyntaxError;

        public SyntaxException(string expr, string error, int pos) :
            base(expr, $"Cannot parse '{expr}': {error} (at position {pos})")
        {
            SyntaxError = error;
            Position = pos;
        }

        public SyntaxException(string expr, string error) :
            base(expr, $"Cannot parse '{expr}': {error}")
        {
            SyntaxError = error;
            Position = -1;
        }
    }
}
