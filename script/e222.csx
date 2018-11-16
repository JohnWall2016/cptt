
// S ➝ S ( S ) S | ε
HashSet<string> Generate(HashSet<string> s)
{
    var r = new HashSet<string>();
    foreach (var e1 in s)
    {
        foreach (var e2 in s)
        {
            foreach (var e3 in s)
            {
                r.Add(e1 + "(" + e2 + ")" + e3);
            }
        }
    }
    return r;
}

var s = new HashSet<string> { "" };
for (var i = 0; i < 3; i++)
{
    s.UnionWith(Generate(s));
}

foreach (var e in s.OrderBy(s => s.Length))
{
    Console.WriteLine(e);
}
