using PythonCaller;
using Test;

var engine1 = new Engine("test1.py");

Console.WriteLine(engine1.Version);
Console.WriteLine("-------------------");

var weather = new Weather
{
    Temp = 22.5,
    Description = "Warm"
};
Console.WriteLine(weather);

var result1 = engine1.Call<Weather, Weather>(weather);
Console.WriteLine(result1);
Console.WriteLine("Time: " + engine1.TotalExecutionTime);
Console.WriteLine("-------------------");


var engine2 = new Engine("test2.py");

var list1 = new List<double> { 3.0, 1.0, 2.0 };
list1.ForEach(i => Console.Write(i + " "));
Console.WriteLine();

var result2 = engine2.Call<List<double>, List<double>>(list1);
result2.ForEach(i => Console.Write(i + " "));
Console.WriteLine();
Console.WriteLine("Time: " + engine2.TotalExecutionTime);
Console.WriteLine("-------------------");


// 'input', 'output' and 'csconnector' are reserved words.
var scope = Scope.Create(@"
import numpy as np

ndarr = np.array(input)
ndarr.sort()
ndarr += np.array([10.0, 8.0, 15.0])
output = ndarr.tolist()");
var engine3 = new Engine(scope);

var list2 = new List<double> { 5.0, 2.0, 3.0 };
list2.ForEach(i => Console.Write(i + " "));
Console.WriteLine();

var result3 = engine3.Call<List<double>, List<double>>(list2);
result3.ForEach(i => Console.Write(i + " "));
Console.WriteLine();
Console.WriteLine("Time: " + engine3.TotalExecutionTime);
Console.WriteLine("-------------------");


var engine4 = new Engine("test3.py");
var result4 = engine4.Call<List<List<double>>>();
Console.WriteLine($"Shape: ({result4.Count}, {result4[0].Count})");
Console.WriteLine("Time: " + engine4.TotalExecutionTime);
Console.WriteLine("-------------------");


var engine5 = new Engine("test4.py");
engine5.StdOutput += str => Console.WriteLine(str);
engine5.Call();
Console.WriteLine("-------------------");