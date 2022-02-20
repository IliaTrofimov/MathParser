using MathParser;

class Program
{
    public static void Main()
    {
        bool verbose = false;
        bool shoudEvaluate = false;
        Expression expression = new("", false, verbose);

        Console.WriteLine($"[INFO] Verbose mode is {(verbose ? "on" : "off")}");
        Console.WriteLine($"[INFO] Expression {(shoudEvaluate ? "will" : "won't")} be evaluated after parsing");
        Console.WriteLine("[INFO] Commands: '-v' for verbose mode, '-ae' for auto-evaluating, -e to evaluate last expression, '-?' for help,");
        Console.WriteLine("\t'-set <name>=<value>' to set value, '-values' to see all variables");
        Console.WriteLine("[INFO] Type ! to exit programm or stop entering variables");
        Console.WriteLine("--------------------------------------");

        while (true)
        {
            Console.Write("\nInput:\t");
            string s = Console.ReadLine();

            if (s is null || s == "!")
                break;
            else if (s == "-?")
            {
                Console.WriteLine("[INFO] Commands: '-v' for verbose mode, '-ae' for auto-evaluating, -e to evaluate last expression, '-?' for help,");
                Console.WriteLine("\t'-set <name>=<value>' to set value, '-values' to see all variables");
                Console.WriteLine("[INFO] Type ! to exit programm or stop entering variables");
                continue;
            }
            else if (s.ToLower() == "-v")
            {
                verbose = !verbose;
                expression.Verbose = verbose;
                Console.WriteLine($"[INFO] Verbose mode is {(verbose ? "on" : "off\n")} now");
                if (verbose)
                {
                    Console.WriteLine("[INFO] Parsing labels: [Lt] - letter, [Dg] - letter, [OB] - opening bracket, [CB] - closing bracket,");
                    Console.WriteLine("\t[Op] - operator, [Fl] -stack flushing");
                    Console.WriteLine("[INFO] Evaluating labels: [Va] - variable or constant, [Ex] -executing operator");
                }
                continue;
            }
            else if (s.ToLower() == "-ae")
            {
                shoudEvaluate = !shoudEvaluate;
                Console.WriteLine($"[INFO] Expression {(shoudEvaluate ? "will" : "won't")} be evaluated after parsing");
                continue;
            }
            else if (s.ToLower() == "-values")
            {
                Console.WriteLine($"Values: {expression.PrintVariables()}");
                continue;
            }
            else if (s.ToLower().StartsWith("-set "))
            {
                try
                {
                    string[] tokens = s.Remove(0, 5).Split('=');
                    expression.SetValue(tokens[0], Convert.ToDouble(tokens[1]));
                }
                catch
                {
                    Console.WriteLine("[ERROR] Cannot parse your input");
                }           
                continue;
            }          
            try
            {
                if (s.ToLower() == "-e")
                    Console.WriteLine($"Result:\t{expression.Evaluate()}");
                else
                {
                    expression.Parse(s);
                    Console.WriteLine($"Postfx:\t{expression.Postfix}");
                    if (shoudEvaluate)
                        Console.WriteLine($"Result:\t{expression.Evaluate()}");
                }
            }
            catch (WrongSymbolException e)
            {
                Console.WriteLine("[ERROR] Wrong symbol");
                Console.WriteLine("\t" + e.Message);
            }
            catch (SyntaxException e)
            {
                Console.WriteLine("[ERROR] Syntax error");
                Console.WriteLine("\t" + e.Message);
            }
            catch (ParsingException e)
            {
                Console.WriteLine("[ERROR] Parsing error");
                Console.WriteLine("\t" + e.Message);
            }
            catch (EvaluatingException e)
            {
                Console.WriteLine("[ERROR] Evaluating error");
                Console.WriteLine("\t" + e.Message);
            }
        }
    }
}
