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

var list = new List<double> { 3.0, 1.0, 2.0 };
list.ForEach(i => Console.Write(i + " "));
Console.WriteLine();

var result2 = engine2.Call<List<double>, List<double>>(list);
result2.ForEach(i => Console.Write(i + " "));
Console.WriteLine();
Console.WriteLine("Time: " + engine2.TotalExecutionTime);
