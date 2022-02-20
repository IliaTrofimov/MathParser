﻿using MathParser;

class Expression
{
    private enum TokenType { None, Variable, Constant, Operator, BracketO, BracketC };
    private Dictionary<string, Variable> variables = new();
    private List<ParserToken> postfix = new();
    private string exprInf = "";

    public bool Verbose = false;

    public Expression(string expression, bool parse = false, bool verbose = false)
    {
        exprInf = expression;
        Verbose = verbose;
        if (parse) Parse(exprInf);
    }


    public string Infix => exprInf;
    public string Postfix => string.Join("", postfix);

    public double GetValue(string token)
    {
        return variables.ContainsKey(token) ? variables[token] : double.NaN;
    }
    public string PrintVariables()
    {
        System.Text.StringBuilder sb = new();
        foreach(var v in variables)
            sb.Append($"{v.Key}={(double.IsNaN(v.Value.Value) ? "n/a" : v.Value.Value)} ");
        return sb.ToString();
    }

    public void SetValue(string token, double value)
    {
        token = token.ToUpper();
        if (variables.ContainsKey(token))
            variables[token].Value = value;
        else
            variables.Add(token, new(token, value));
    }
    public void SetValues(string[] tokens, double[] values)
    {
        for (int i = 0; i < tokens.Length && i < values.Length; i++)
            SetValue(tokens[i], values[i]);
    }
    public void SetValues(string[] tokens, double value)
    {
        for (int i = 0; i < tokens.Length; i++)
            SetValue(tokens[i], value);
    }
    public void SetValues(Dictionary<string, double> variables)
    {
        foreach (var v in variables)
            SetValue(v.Key, v.Value);
    }


    public List<ParserToken> Parse(string expression)
    {
        exprInf = expression;
        return Parse();
    }

    public List<ParserToken> Parse()
    {
        string token = "";
        Stack<ParserToken> stack = new();
        TokenType last = TokenType.None;

        void pushOperand()
        {
            if (last == TokenType.Constant)
            {
                postfix.Add(new Constant(Convert.ToDouble(token)));
            }
            else if (last == TokenType.Variable)
            {
                if(variables.ContainsKey(token))
                    postfix.Add(variables[token]);
                else
                {
                    variables.Add(token, new Variable(token));
                    postfix.Add(variables[token]);
                }
            }
            token = "";
        }

        for (int i = 0; i < exprInf.Length; i++)
        {
            if (char.IsWhiteSpace(exprInf[i]))
                continue;
            else if (char.IsDigit(exprInf[i]) || exprInf[i] == '.' || exprInf[i] == ',')
            {
                if (last == TokenType.BracketC)
                    throw new WrongSymbolException(exprInf, i);
                if (last == TokenType.Variable && exprInf[i] == '.' || exprInf[i] == ',')
                    throw new WrongSymbolException(exprInf, i);
                else if (last != TokenType.Variable)
                    last = TokenType.Constant;

                token += (exprInf[i] == '.' ? ',' : exprInf[i]);
                if (Verbose) Console.WriteLine($"  [Dt] i={i}\tres='{Postfix}'\tstk=[{string.Join(", ", stack)}]");
            }
            else if (char.IsLetter(exprInf[i]))
            {
                if (last == TokenType.BracketC || last == TokenType.Constant)
                    throw new WrongSymbolException(exprInf, i);

                last = TokenType.Variable;
                token += exprInf[i];
                if (Verbose) Console.WriteLine($"  [Lt] i={i}\tres='{Postfix}'\tstk=[{string.Join(", ", stack)}]");
            }
            else if (exprInf[i] == '(')
            {
                if (last == TokenType.BracketC || last == TokenType.Variable || last == TokenType.Constant)
                    throw new WrongSymbolException(exprInf, i);

                last = TokenType.BracketO;
                stack.Push(new ParserToken("("));
                if (Verbose) Console.WriteLine($"  [Ob] i={i}\tres='{Postfix}'\tstk=[{string.Join(", ", stack)}]");
            }
            else if (exprInf[i] == ')')
            {
                if (last == TokenType.BracketO || last == TokenType.Operator || last == TokenType.None)
                    throw new WrongSymbolException(exprInf, i);

                pushOperand();
                while (stack.Count != 0 && stack.Peek() != "(")
                    postfix.Add(stack.Pop());

                if (stack.Count == 0)
                    throw new SyntaxException(exprInf, "closing and opening brackets do not match");

                last = TokenType.BracketC;
                stack.Pop();
                if (Verbose) Console.WriteLine($"  [Cb] i={i}\tres='{Postfix}'\tstk=[{string.Join(", ", stack)}]");
            }
            else if (last != TokenType.BracketO && last != TokenType.Operator && last != TokenType.None)
            {
                pushOperand();
                last = TokenType.Operator;
                BinaryOperator op = BinaryOperator.SelectOperator(exprInf[i]);

                while ((stack.Count != 0) && op.Order <= stack.Peek().Order)
                {
                    ParserToken t = stack.Pop();
                    if (t != "(" && t != ")") 
                        postfix.Add(t);
                }
                stack.Push(op);
                if (Verbose) Console.WriteLine($"  [Op] i={i}\tres='{Postfix}'\tstk=[{string.Join(", ", stack)}]");
            }
            else if (last != TokenType.Operator)
            {
                pushOperand();
                last = TokenType.Operator;
                UnaryOperator op = UnaryOperator.SelectOperator(exprInf[i]);

                while ((stack.Count != 0) && op.Order <= stack.Peek().Order)
                {
                    ParserToken t = stack.Pop();
                    if (t != "(" && t != ")")
                        postfix.Add(t);
                }
                stack.Push(op);
                if (Verbose) Console.WriteLine($"  [Op] i={i}\tres='{Postfix}'\tstk=[{string.Join(", ", stack)}]");
            }
            else throw new WrongSymbolException(exprInf, i);
        }

        pushOperand();

        while (stack.Count != 0)
        {
            if (stack.Peek() != "(" && stack.Peek() != ")")
                postfix.Add(stack.Pop());
            else
                throw new SyntaxException(exprInf, "closing and opening brackets do not match");
            if (Verbose) Console.WriteLine($"  [Fl] \t\tres='{Postfix}'\tstk=[{string.Join(", ", stack)}]");
        }
        return postfix;
    }

    public double Evaluate(string expression)
    {
        Parse(expression);
        return Evaluate();
    }

    public double Evaluate()
    {
        Stack<double> values = new();

        foreach (ParserToken token in postfix)
        {
            if (token.Arguments == 0)
            {
                values.Push(token.Eval());
                if (Verbose) Console.WriteLine($"  [Va] stk=[{string.Join(", ", values)}]\t{token}->{token.Eval()}");
            }
            else if (token.Arguments == 1)
            {
                double val = values.Pop();
                double res = token.Eval(val);
                values.Push(res);
                if (Verbose) Console.WriteLine($"  [Ex] stk=[{string.Join(", ", values)}]\t{token}{val}={res}");
            }
            else if (token.Arguments == 2)
            {
                double right = values.Pop();
                double left = values.Pop();
                double res = token.Eval(left, right);
                if (!double.IsFinite(res))
                    throw new MathException("indeterminate", left, right, token.Token);
                values.Push(res);
                if (Verbose) Console.WriteLine($"  [Ex] stk=[{string.Join(", ", values)}]\t{left}{token}{right}={res}");
            }
        }
        if (values.Count != 1)
            throw new EvaluatingException("Cannot evaluate expression, wrong postifx form");
        return values.Pop();
    }
}
