//expr   ➝ expr + term | expr - term
//term   ➝ term * factor | term ÷ factor | factor
//factor ➝ num | ( expr ) 
//num    ➝ [+-]?[0..9]+(\.[0..9]+)?

class Token
{
    
}

var LB = new Token();
var RB = new Token();
var ADD = new Token();
var SUB = new Token();
var MUL = new Token();
var DIV = new Token();

class Scanner
{
    public void Store() {}
    public void Restore() {}

    public Token Next() {}
}

class Node {}

class Expr : Node {}

class Term : Node {}

class Factor : Node {}

class Num : Node {}

Num ParseNum(Scanner scan)
{
    // TODO
    return null;
}

Node ParseFactor(Scanner scan)
{
    scan.Store();
    var num = ParseNume();
    if (num != null)
        return num;
    scan.Restore();

    if (scan.Next() == LB)
    {
        var expr = ParseExpr(scan);
        if (expr == null)
            throw new Exception("An Expr is required.");
        if (scan.Next() != RB)
            throw new Exception("An RB is required.");
        return expr;
    }
    return null;
}

Expr ParseExpr(Scanner scan)
{
    // TODO
    return null;
}
