class Parser
{
    public class EvaluatingException : Exception 
    {
        public EvaluatingException(string msg): base(msg) { }
    }

    public class MathException : EvaluatingException
    {
        public double Left, Right;
        public char Operator;
        public MathException(string err, double left, double right, char op) : 
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



    public static readonly string Operators = "+-*/";

    public static int GetOrder(char op) => op switch
    {
        '^' => 3,
        '*' or '/' => 2,
        '+' or '-' => 1,
        _ => -1,
    };


    public static List<char> ToPostfix(string expr)
    {
        Stack<char> stack = new();
        List<char> result = new();
        
        for (int i = 0; i < expr.Length; i++)
        {
            if (char.IsWhiteSpace(expr[i]))    // Пропускаем пробелы
                continue;

            else if (char.IsLetter(expr[i]))   // Переменные, только односимвольные разрешены
            {
                if (i != (expr.Length - 1) && char.IsLetter(expr[i + 1]))
                    throw new SyntaxException(expr, "Variables' names must contain only one latin symbol", i);

                result.Add(char.ToUpper(expr[i]));
            }

            else if (expr[i] == '(')           // Открывающая скобка, кладём её в стек
            { 
                stack.Push(expr[i]);
            }

            else if (expr[i] == ')')           // Закрывающая скобка, вытаскивем всё из стека
            {
                while (stack.Count != 0 && stack.Peek() != '(')
                    result.Add(stack.Pop());

                if (stack.Count == 0)
                    throw new SyntaxException(expr, "closing and opening brackets do not match");
                stack.Pop();
            }

            else if (Operators.Contains(expr[i]))   // Операторы
            {
                if (i == (expr.Length - 1) || Operators.Contains(expr[i + 1]))
                    throw new SyntaxException(expr, "unexpected operator", i);

                while ((stack.Count != 0) && GetOrder(expr[i]) <= GetOrder(stack.Peek()))
                {
                    char ch = stack.Pop();
                    if (ch != '(' && ch != ')') result.Add(ch);
                }
                stack.Push(expr[i]);
            }

            else throw new WrongSymbolException(expr, i);   // Недопустимые символы 
        }
        while (stack.Count != 0) 
        { 
            if (stack.Peek() != '(' && stack.Peek() != ')')            
                result.Add(stack.Pop());        
            else 
                throw new SyntaxException(expr, "closing and opening brackets do not match");
        }
        return result;
    }

    public static List<char> ToPostfixVerbose(string expr)
    {
        Stack<char> stack = new();
        List<char> result = new();
        Console.WriteLine("[PARSING]");       

        for (int i = 0; i < expr.Length; i++)
        {
            if (char.IsWhiteSpace(expr[i]))    // Пропускаем пробелы
                continue;

            else if (char.IsLetter(expr[i]))   // Переменные, только односимвольные разрешены
            {
                if (i != (expr.Length - 1) && char.IsLetter(expr[i + 1]))
                    throw new SyntaxException(expr, "Variables' names must contain only one latin symbol", i);

                result.Add(char.ToUpper(expr[i]));
                Console.WriteLine($"  [Lt] i={i}\tres='{string.Join("", result)}'\tstk=[{string.Join(", ", stack)}]");
            }

            else if (expr[i] == '(')           // Открывающая скобка, кладём её в стек
            {
                stack.Push(expr[i]);
                Console.WriteLine($"  [OB] i={i}\tres='{string.Join("", result)}'\tstk=[{string.Join(", ", stack)}]");
            }

            else if (expr[i] == ')')           // Закрывающая скобка, вытаскивем всё из стека
            {
                while (stack.Count != 0 && stack.Peek() != '(')
                    result.Add(stack.Pop());

                if (stack.Count == 0)
                    throw new SyntaxException(expr, "closing and opening brackets do not match");
                stack.Pop();
                Console.WriteLine($"  [CB] i={i}\tres='{string.Join("", result)}'\tstk=[{string.Join(", ", stack)}]");
            }

            else if (Operators.Contains(expr[i]))   // Операторы
            {
                while ((stack.Count != 0) && GetOrder(expr[i]) <= GetOrder(stack.Peek()))
                {
                    if (i == (expr.Length - 1) || Operators.Contains(expr[i + 1]))
                        throw new SyntaxException(expr, "unexpected operator", i);

                    char ch = stack.Pop();
                    if (ch != '(' && ch != ')') result.Add(ch);
                }
                stack.Push(expr[i]);
                Console.WriteLine($"  [Op] i={i}\tres='{string.Join("", result)}'\tstk=[{string.Join(", ", stack)}]");
            }

            else throw new WrongSymbolException(expr, i);   // Недопустимые символы 
        }
        while (stack.Count != 0)
        {
            if (stack.Peek() != '(' && stack.Peek() != ')')
                result.Add(stack.Pop());
            else
                throw new SyntaxException(expr, "closing and opening brackets do not match");

            Console.WriteLine($"  [Fl]\t\tres='{string.Join("", result)}'\tstk=[{string.Join(", ", stack)}]");
        }
        return result;
    }


    public static double Evaluate(IList<char> expr, Dictionary<char, double> variables)
    {
        Stack<double> values = new();

        foreach (char token in expr)
        {
            if (char.IsLetter(token))               // Переменная, заменяем её на число, кладём его в стек
            {
                double value;
                if (!variables.TryGetValue(token, out value))
                    throw new EvaluatingException($"Cannot evaluate expression, '{token}' is not defined");
                values.Push(value);
            }       
            else if (Operators.Contains(token))     // Оператор, выполняем его с двумя верхнимим числами
            {
                double right = values.Pop();
                double left = values.Pop();
                double res = Execute(left, right, token);
                if (!double.IsFinite(res))
                    throw new MathException("indeterminate", left, right, token);
                
                values.Push(res);
            }
            else 
                throw new EvaluatingException($"Cannot evaluate expression, unacceptable symbol '{token}' found");
        }
        return values.Pop();
    }

    public static double EvaluateVerbose(IList<char> expr, Dictionary<char, double> variables)
    {
        Stack<double> values = new();
        Console.WriteLine("[EVALUATING]");

        foreach (char token in expr)
        {
            if (char.IsLetter(token))
            {
                double value;
                if (!variables.TryGetValue(token, out value))
                    throw new EvaluatingException($"Cannot evaluate expression, '{token}' is not defined");
                values.Push(value);
                Console.WriteLine($"  [Lt] values=[{string.Join(", ", values)}]");
            }
            else if (Operators.Contains(token))
            {
                double right = values.Pop();
                double left = values.Pop();
                double res = Execute(left, right, token);
                if (!double.IsFinite(res))
                    throw new MathException("indeterminate", left, right, token);
                
                values.Push(res);
                Console.WriteLine($"  [Op] values=[{string.Join(", ", values)}]\t  oper='{token}'\taction: {left} {token} {right} = {res}");
            }
            else
                throw new EvaluatingException($"Cannot evaluate expression, unacceptable symbol '{token}' found");
        }
        return values.Pop();
    }


    private static double Execute(double left, double right, char op) => op switch
    {
        '^' => Math.Pow(left, right),
        '*' => left * right,
        '/' => left / right,
        '+' => left + right,
        '-' => left - right,
        _ => throw new EvaluatingException($"Cannot execute unknown operation '{op}'")
    };
}
