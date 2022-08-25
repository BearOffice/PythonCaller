using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonCaller;

internal class RandomString
{
    private int _seed;
    private readonly int[] _incrs;
    private readonly object _lock = new object();

    internal RandomString()
    {
        var rand = new Random();

        _seed = rand.Next(100000);
        _incrs = Enumerable.Range(0, 10).Select(_ => rand.Next(10)).ToArray();
    }

    internal string GetUniqueString()
    {
        var name = _seed.ToString().GetHashCode().ToString("X");

        lock (_lock)
        {
            _seed += _incrs[_seed % 10];
        }

        return name;
    }
}
