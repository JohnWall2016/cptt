
// S ➝ S S +
// S ➝ S S *
HashSet<string> Generate(HashSet<string> s)
{
    var r = new HashSet<string>();
    foreach (var e1 in s)
    {
        foreach (var e2 in s)
        {
            r.Add(e1 + e2 + "+");
            r.Add(e1 + e2 + "*");
        }
    }
    return r;
}

var s = new HashSet<string> { "a" };
for (var i = 0; i < 3; i++)
{
    s.UnionWith(Generate(s));    
}

foreach (var e in s.OrderBy(s => s.Length))
{
    Console.WriteLine(e);
}
