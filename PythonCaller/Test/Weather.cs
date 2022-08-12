using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test;

public class Weather
{
    public double Temp { get; set; }
    public string Description { get; set; }

    public override string ToString()
    {
        return $"{Temp}, {Description}";
    }
}
