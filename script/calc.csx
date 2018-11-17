// expr   ➝ expr + term | expr - term | term
// term   ➝ term * factor | term ÷ factor | factor
// factor ➝ num | ( expr )
// num    ➝ [+-]?[0..9]+(\.[0..9]+)?

class Scanner
{
    char[] _expr;
    int _pos = 0;

    public Scanner(string expr) => _expr = expr.ToArray();

    public int Pos
    {
        get => _pos;
        set => _pos = value;
    }

    public bool Forward()
    {
        _pos += 1;
        if (_pos < _expr.Length)
            return true;
        else
            return false;
    }

    public bool IsValid => _pos >= 0 && _pos < _expr.Length;

    public bool IsDigit => IsValid && Current >= '0' && Current <= '9';

    public bool IsLBrace => IsValid && Current == '(';

    public bool IsRBrace => IsValid && Current == ')';

    public bool IsAdd => IsValid && Current == '+';

    public bool IsSub => IsValid && Current == '-';
    
    public bool IsMul => IsValid && Current == '*';

    public bool IsDiv => IsValid && Current == '/';

    public bool IsPeriod => IsValid && Current == '.';

    public bool IsSpace => IsValid && Current == ' ';

    public bool End => _pos >= _expr.Length;
    
    public char Current => _expr[_pos];

    public string Snapshot(int pos)
    {
        var str = new string(_expr);
        if (pos < str.Length)
            return $"{str.Substring(0, pos)}|{str.Substring(pos, str.Length-pos)}";
        else
            return str;
    }

}

abstract class Node<T>
{
    public abstract T Eval();
}

class Num : Node<decimal>
{
    decimal _val;
    
    public Num(decimal val) => _val = val;

    public override decimal Eval() => _val;
}

// num ➝ [+-]?[0..9]+(\.[0..9]+)?
Num ParseNum(Scanner scan)
{
    var num = new StringBuilder();

    if (!scan.IsValid) return null;
    if (scan.IsAdd || scan.IsSub)
    {
        num.Append(scan.Current);
        if (!scan.Forward()) return null;
    }
    if (!scan.IsDigit) return null;
    num.Append(scan.Current);
    while (scan.Forward() && scan.IsDigit)
    {
        num.Append(scan.Current);
    }
    if (scan.IsValid && scan.IsPeriod)
    {
        num.Append(scan.Current);
        if (!scan.Forward() || !scan.IsDigit) return null;
        num.Append(scan.Current);
        while (scan.Forward() && scan.IsDigit)
        {
            num.Append(scan.Current);
        }
    }
    
    return new Num(decimal.Parse(num.ToString()));
}

abstract class Factor : Node<decimal> {}

class NumFactor : Factor
{
    Num _num;
    
    public NumFactor(Num num) => _num = num;

    public override decimal Eval() => _num.Eval();
}

class ExprFactor : Factor
{
    Expr _expr;

    public ExprFactor(Expr expr) => _expr = expr;

    public override decimal Eval() => _expr.Eval();
}

// factor ➝ num | ( expr )
Factor ParseFactor(Scanner scan)
{
    while (scan.IsSpace) scan.Forward();
    
    var pos = scan.Pos;
    var num = ParseNum(scan);
    if (num != null)
    {
        return new NumFactor(num);
    }
    scan.Pos = pos;

    if (scan.IsValid && scan.IsLBrace)
    {
        scan.Forward();
        var expr = ParseExpr(scan);
        if (expr != null && scan.IsValid && scan.IsRBrace)
        {
            scan.Forward();
            return new ExprFactor(expr);
        }
    }
    throw new Exception($"Cannot parse factor: {scan.Snapshot(pos)}");
}

// term ➝ term * factor | term ÷ factor | factor
//
// term ➝ factor mul_div_factor
// mul_div_factor ➝ * factor mul_div_factor | ÷ factor mul_div_factor | ε

class Term : Node<decimal>
{
    Factor _factor;
    MulDivFactor _mdfactor;

    public Term(Factor factor, MulDivFactor mdfactor)
    {
        _factor = factor;
        _mdfactor = mdfactor;
    }

    decimal Eval(decimal factor, MulDivFactor mdfactor)
    {
        if (mdfactor != null)
        {
            if (mdfactor.Op == MulDivFactor.OP.Mul)
                return Eval(factor * mdfactor.Factor.Eval(), mdfactor.MDFactor);
            else
                return Eval(factor / mdfactor.Factor.Eval(), mdfactor.MDFactor);
        }
        else
        {
            return factor;
        }

    }

    public override decimal Eval() => Eval(_factor.Eval(), _mdfactor);
}

class MulDivFactor
{
    public enum OP { Mul, Div }

    public OP Op { get; }
    public Factor Factor { get; }
    public MulDivFactor MDFactor { get; }

    public MulDivFactor(OP op, Factor factor, MulDivFactor mdfactor)
    {
        this.Op = op;
        this.Factor = factor;
        this.MDFactor = mdfactor;
    }
}

// term ➝ factor mul_div_factor
// mul_div_factor ➝ * factor mul_div_factor | ÷ factor mul_div_factor | ε
Term ParseTerm(Scanner scan)
{
    var factor = ParseFactor(scan);
    var mdfactor = ParseMDFactor(scan);
    
    return new Term(factor, mdfactor);
}

MulDivFactor ParseMDFactor(Scanner scan)
{
    while (scan.IsSpace) scan.Forward();
    
    if (scan.IsMul || scan.IsDiv)
    {
        var op = scan.IsMul ? MulDivFactor.OP.Mul
            : MulDivFactor.OP.Div;
        scan.Forward();
        var factor = ParseFactor(scan);
        var mdfactor = ParseMDFactor(scan);
        return new MulDivFactor(op, factor, mdfactor);
    }
    else
    {
        return null;
    }
}

// expr ➝ expr + term | expr - term | term
//
// expr ➝ term add_sub_term
// add_sub_term ➝ + term add_sub_term | - term add_sub_term |  ε

class Expr : Node<decimal>
{
    Term _term;
    AddSubTerm _asterm;

    public Expr(Term term, AddSubTerm asterm)
    {
        _term = term;
        _asterm = asterm;
    }

    decimal Eval(decimal term, AddSubTerm asterm)
    {
        if (asterm != null)
        {
            if (asterm.Op == AddSubTerm.OP.Add)
                return Eval(term + asterm.Term.Eval(), asterm.ASTerm);
            else
                return Eval(term - asterm.Term.Eval(), asterm.ASTerm);
        }
        else
        {
            return term;
        }
    }

    public override decimal Eval() => Eval(_term.Eval(), _asterm);
}

class AddSubTerm
{
    public enum OP { Add, Sub }

    public OP Op { get; }
    public Term Term { get; }
    public AddSubTerm ASTerm { get; }

    public AddSubTerm(OP op, Term term, AddSubTerm asterm)
    {
        this.Op = op;
        this.Term = term;
        this.ASTerm = asterm;
    }
}

// expr ➝ term add_sub_term
// add_sub_term ➝ + term add_sub_term | - term add_sub_term |  ε
Expr ParseExpr(Scanner scan)
{
    var term = ParseTerm(scan);
    var asterm = ParseASTerm(scan);
    
    return new Expr(term, asterm);
}

AddSubTerm ParseASTerm(Scanner scan)
{
    while (scan.IsSpace) scan.Forward();
    
    if (scan.IsAdd || scan.IsSub)
    {
        var op = scan.IsAdd ? AddSubTerm.OP.Add
            : AddSubTerm.OP.Sub;
        scan.Forward();
        var term = ParseTerm(scan);
        var asterm = ParseASTerm(scan);
        return new AddSubTerm(op, term, asterm);
    }
    else
    {
        return null;
    }
}

try
{
    var scan = new Scanner(string.Join("", Args));
    var node = ParseExpr(scan);
    if (scan.End)
        Console.WriteLine(node.Eval());
    else
        Console.WriteLine($"Unparsed string: {scan.Snapshot(scan.Pos)}");
}
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
}



