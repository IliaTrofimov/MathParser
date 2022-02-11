class Program
{
    public static void Main()
    {
        bool verbose = false;
        bool caseSensitive = false;
        bool shoudEvaluate = false;
        Dictionary<char, double> variables = new();
        Func<string, bool, List<char>> convert = Parser.ToPostfix;
        Func<IList<char>, Dictionary<char, double>, double> evaluate = Parser.Evaluate;

        Console.WriteLine($"[INFO] {(caseSensitive ? "case sensitive" : "not case sensitive")}");
        Console.WriteLine($"[INFO] verbose mode is {(verbose ? "on" : "off")}");
        Console.WriteLine($"[INFO] {(shoudEvaluate ? "expression will be evaluated after parsing" : "expression won't be evaluated after parsing")}");
        Console.WriteLine("--------------------------------------\n");

        while (true)
        {
            Console.Write("Input:\t");
            string s = Console.ReadLine();

            if (s is null || s == "!")
                break;
            else if (s.ToLower() == "-v")
            {
                verbose = !verbose;
                convert = verbose ? Parser.ToPostfixVerbose : Parser.ToPostfix;
                evaluate = verbose ? Parser.EvaluateVerbose : Parser.Evaluate;
                Console.WriteLine($"[INFO] verbose mode is {(verbose ? "on" : "off")} now\n");
                continue;
            }
            else if (s.ToLower() == "-c")
            {
                caseSensitive = !caseSensitive;
                Console.WriteLine($"[INFO] {(caseSensitive ? "case sensitive" : "not case sensitive")} now\n");
                continue;
            }
            else if (s.ToLower() == "-e")
            {
                shoudEvaluate = !shoudEvaluate;
                Console.WriteLine($"[INFO] {(shoudEvaluate ? "expression will be evaluated after parsing" : "expression won't be evaluated after parsing")}\n");
                continue;
            }
            else if (s.ToLower() == "-setvalues")
            {
                string newValues = SetVariables(variables);
                Console.WriteLine($"[INFO] {(newValues == "" ? "values weren't changed" : "vew values: " + newValues)}\n");
                continue;
            }
            else if (s.ToLower() == "-values")
            {
                Console.WriteLine($"All values: [ {DictToString(variables)}]\n");
                continue;
            }

            try
            {
                if (!shoudEvaluate)
                    Console.WriteLine($"Postfx:\t{string.Join("", convert(s, caseSensitive))}");
                else
                {
                    
                    var postfix = convert(s, caseSensitive);
                    Console.WriteLine($"Postfx:\t{string.Join("", postfix)}");
                    Console.WriteLine($"Result:\t{evaluate(postfix, variables)}");
                }
            }
            catch (Parser.WrongSymbolException e)
            {
                Console.WriteLine("[ERROR] Wrong symbol");
                Console.WriteLine(e.Message);
            }
            catch (Parser.SyntaxException e)
            {
                Console.WriteLine("[ERROR] Syntax error");
                Console.WriteLine(e.Message);
            }
            catch (Parser.ParsingException e)
            {
                Console.WriteLine("[ERROR] Parsing error");
                Console.WriteLine(e.Message);
            }
            catch (Parser.EvaluatingException e)
            {
                Console.WriteLine("[ERROR] Evaluating error");
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Undefined error");
                Console.WriteLine(e.Message);
            }
            Console.WriteLine();
        }
    }

    public static string SetVariables(Dictionary<char, double> dict)
    {
        Console.WriteLine("Enter variables and their values (var_name=var_value):");
        System.Text.StringBuilder sb = new();
        int lines = 1;

        while(true)
        {
            lines++;
            Console.Write(">>> ");
            string s = Console.ReadLine();
            if (s == "!")
                break;

            string[] data = s.Split('=');
            try
            {
                double val = Convert.ToDouble(data[1]);
                if (!dict.TryAdd(data[0][0], val))
                    dict[data[0][0]] = val;
                sb.Append($"{data[0][0]}={dict[data[0][0]]} ");
            }
            catch (FormatException)
            {
                Console.WriteLine("[ERROR] Wrong variable value!");
                lines++;
            }
            catch
            {
                Console.WriteLine("[ERROR] Cannot parse variable's name and value!");
                lines++;
            }
        }
        
        for(; lines > 0; lines--)
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
        }
        
        Console.SetCursorPosition(0, Console.CursorTop);
        return sb.ToString();
    }

    public static string DictToString<TKey, TValue>(Dictionary<TKey, TValue> dict)
    {
        System.Text.StringBuilder sb = new();
        foreach (var p in dict)
            sb.Append($"{p.Key}={p.Value} ");
        return sb.ToString();
    }
}

class Parser
{
    public class EvaluatingException : Exception 
    {
        public EvaluatingException(string msg): base(msg) { }
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
        '*' or '/' => 2,
        '+' or '-' => 1,
        _ => -1,
    };


    public static List<char> ToPostfix(string expr, bool caseSensitive = false)
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

                result.Add(caseSensitive ? expr[i] : char.ToUpper(expr[i]));
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

    public static List<char> ToPostfixVerbose(string expr, bool caseSensitive = false)
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

                result.Add(caseSensitive ? expr[i] : char.ToUpper(expr[i]));
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
                values.Push(Execute(left, right, token));
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
        '*' => left * right,
        '/' => left / right,
        '+' => left + right,
        '-' => left - right,
        _ => throw new EvaluatingException($"Cannot execute unknown operation '{op}'")
    };
}
