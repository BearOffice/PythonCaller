using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonCaller;

internal class Workspace
{
    internal static string Path { get; } = "./.pythoncaller";

    private const string _csconnector = @"import sys, json

def get_input():
    return json.load(sys.stdin)

def set_output(obj):
    try:
        json.dump(obj, sys.stdout)
    except:
        if hasattr(obj, '__dict__'):
            json.dump(obj.__dict__, sys.stdout)
        else:
            json.dump(obj, sys.stdout)";

    private static readonly object _lock = new object();
    private static readonly ConcurrentDictionary<int, List<string>> _dic = new ConcurrentDictionary<int, List<string>>();
    private static int _seed = new Random().Next(10000);
    private static int _count = 0;

    private readonly int _uniqueNum;
    private readonly List<string> _tempFiles = new List<string>();
   
    internal Workspace()
    {
        lock (_lock)
        {
            _uniqueNum = _count;
            _count++;
        }

        CreateSpace();
    }

    internal string CreateTempFile(string contents)
    {
        var fileName = GetUniqueName() + ".py";
        var filePath = CreateFile(fileName, contents);

        if (filePath is null)
            throw new Exception("Failed to create temporary file.");

        if (!_dic.ContainsKey(_uniqueNum))
            _dic.GetOrAdd(_uniqueNum, _tempFiles);

        _tempFiles.Add(filePath);

        return filePath;
    }

    internal void RemoveTempFiles(bool forceRemove = false)
    {
        if (forceRemove)
            _tempFiles.ForEach(f => RemoveFile(f));

        _dic.Remove(_uniqueNum, out _);

        if (_dic.IsEmpty) CleanSpace();
    }
    
    private static string? CreateFile(string fileName, string contents)
    {
        var filePath = Path + "/" + fileName;

        if (!File.Exists(filePath))
        {
            using var writer = new StreamWriter(filePath);
            writer.Write(contents);

            return filePath;
        }

        return null;
    }

    private static string GetUniqueName()
    {
        var name = _seed.ToString().GetHashCode().ToString("X");

        lock (_lock)
        {
            _seed++;
        }

        return name;
    }

    private static void RemoveFile(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(Path);
    }

    internal static void CreateSpace()
    {
        var dirInfo = Directory.CreateDirectory(Path);
        dirInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
        CreateFile("csconnector.py", _csconnector);
    }

    internal static void CleanSpace()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, true);
    }
}
