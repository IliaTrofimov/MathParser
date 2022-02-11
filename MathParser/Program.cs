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
        Console.WriteLine("[INFO] Commands: '-v' for verbose mode, '-c' for case sensivity, '-e' for evaluating, '-?' for help, '-setvalues', '-values'\n[INFO] Type ! to exit programm or stop entering variables");
        Console.WriteLine("--------------------------------------\n");

        while (true)
        {
            Console.Write("Input:\t");
            string s = Console.ReadLine();

            if (s is null || s == "!")
                break;
            else if (s == "-?")
            {
                Console.WriteLine("[INFO] Avaliabble commands: -v, -c, -e, -setvalues, -values, -?\n[INFO] Type ! to exit\n");
                continue;
            }
            else if (s.ToLower() == "-v")
            {
                verbose = !verbose;
                convert = verbose ? Parser.ToPostfixVerbose : Parser.ToPostfix;
                evaluate = verbose ? Parser.EvaluateVerbose : Parser.Evaluate;
                Console.WriteLine($"[INFO] verbose mode is {(verbose ? "on" : "off\n")} now");
                if (verbose)
                    Console.WriteLine("[INFO] labels: [Lt] - letter, [OB] - opening bracket, [CB] - closing bracket, [Op] - operator, [Fl] - stack flushing\n");
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
                Console.WriteLine((newValues == "" ? "Values weren't changed" : "New values: " + newValues) + "\n");
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
