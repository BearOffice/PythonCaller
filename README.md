# PythonCaller
A class library to call and get result from python file.

Use these two functions `get_input()` and `set_output(obj)` in python program by importing `csconnector` to receive/send object from/to the caller(C# program).  
*** You must first use `init environment()` before using `get_input()` or `set_output(obj)`.
<br>
https://www.nuget.org/packages/PythonCaller/

## State of interprocess communication
```
[C#] source object
       ↓
---------------------------------------
[C#] serialize source object to json info
[C#] pass json_output_token and json info to python program
       ↓
 (can be simplified by using module csconnector)
[py] receive json_output_token and json info from stdin
[py] deserialize json info to source object
       ↓
[py] do some processes (C# can receive stdoutput from python program)
       ↓
 (can be simplified by using module csconnector)
[py] serialize result object to json info
[py] pass json_output_token and json info to standard output
       ↓
[C#] receive json_output_token and deserialize result object from json info 
---------------------------------------
       ↓
[C#] result object
(json_output_token is to identify the border between normal stdoutput and json info output)
```

<br>

# How to use
## example1
tsne.py
```py
from sklearn.datasets import load_digits
from sklearn.manifold import TSNE
import csconnector as csc

csc.init_environment()

digits = load_digits()
tsne = TSNE(init="pca", learning_rate="auto")
digits_tsne = tsne.fit_transform(digits.data)
#                          ↓ convert ndarray to list
csc.set_output(digits_tsne.tolist())  # pass output to c#
```
C# test code
```C#
var engine = new Engine("tsne.py");

var result = engine.Call<List<List<double>>>();  // call python
Console.WriteLine($"Shape: ({result.Count}, {result[0].Count})");
```
Output
```
(1797, 2)
```

<br>

## example2
Call python from string literal  
**'input'**, **'output'** and **'csconnector'** are reserved words.  
`init environment()` is automatically implemented.
```C#
var scope = Scope.Create(@"
import numpy as np

ndarr = np.array(input)
ndarr.sort()
ndarr += np.array([10.0, 8.0, 15.0])
output = ndarr.tolist()  # set output
");
var engine = new Engine(scope);
var list = new List<double> { 5.0, 2.0, 3.0 };

var result = engine.Call<List<double>, List<double>>(list);
result.ForEach(i => Console.Write(i + " "));
```
Output
```
12 11 20 
```

<br>

## example3
Define an object.
```C#
public class Weather
{
    public double Temp { get; set; }
    public string Description { get; set; }
}
```
```py
class Weather(object):
    def __init__(self, temp, description):
        self.temp = temp
        self.description = description
```

test.py
```py
import csconnector as csc

csc.init_environment()

input = csc.get_input()  # receive input from c#
input = Weather(**input)  # convert object to Weather object

input.temp += 12.8
input.description = "Too hot"

csc.set_output(input)  # pass output to c#
```

C# test code
```C#
var weather = new Weather
{
    Temp = 22.5,
    Description = "Warm"
};

var engine = new Engine("test.py");
var result = engine.Call<Weather, Weather>(weather);  // call python
Console.WriteLine(result);
```
Output
```
35.3, Too hot
```

## example4
Receive stdoutput from python program
```C#
engine.StdOutput += str => Console.WriteLine(str)
```
