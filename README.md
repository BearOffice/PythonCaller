# PythonCaller
A class library to call and get result from python file.

The python file to be called should have at most ONE standard input to receive json info from caller(e.g. c# program) and at most ONE standard output to pass json info to caller.

`csconnector.py` wrapped the serialization and deserialization procedure into two simple functions `get_input()` and `set_output(ob)`. Import `csconnector` to use these functions.  
<br>

## State of interprocess communication
```
[C#] source object
       ↓
---------------------------------------
[C#] serialize source object to json info
[C#] pass json info to python program
       ↓
 (can be simplified by using module csconnector)
[py] receive json info from stdin
[py] deserialize json info to source object
       ↓
[py] do some processes
       ↓
 (can be simplified by using module csconnector)
[py] serialize result object to json info
[py] pass json info to standard output
       ↓
[C#] deserialize result object from json info 
---------------------------------------
       ↓
[C#] result object
```

<br>

# How to use
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

python code(test1.py)
```py
import csconnector as csc

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

var engine1 = new Engine("test1.py");
var result1 = engine1.Call<Weather, Weather>(weather);  // call python
Console.WriteLine(result1);
```
output
```
35.3, Too hot
```