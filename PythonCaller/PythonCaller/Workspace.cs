using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonCaller;

internal class Workspace
{
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
    private static readonly RandomString _rand = new RandomString();
    private static int _createdWorkspaceCount = 0;

    private readonly int _uniqueNumber;
    private readonly List<string> _tempFiles = new List<string>();

    private static string _path = "./__pycaller__";
    internal static string Path
    {
        get
        {
            return _path;
        }
        set
        {
            if (_dic.IsEmpty)
            {
                _path = value;
            }
            else
            {
                if (_path != value)
                    throw new InvalidOperationException("The path cannot be changed because the workspace is currently in use.");
            }
        }
    }

    internal Workspace()
    {
        lock (_lock)
        {
            _uniqueNumber = _createdWorkspaceCount;
            _createdWorkspaceCount++;
        }

        CreateSpace();
    }

    internal string CreateTempFile(string contents)
    {
        var filePath = default(string);

        while (filePath is null)
        {
            var fileName = _rand.GetUniqueString() + ".py";
            filePath = CreateFile(fileName, contents);
        }

        if (!_dic.ContainsKey(_uniqueNumber))
            _dic.GetOrAdd(_uniqueNumber, _tempFiles);

        _tempFiles.Add(filePath);

        return filePath;
    }

    internal void RemoveTempFiles(bool forceRemove = false)
    {
        if (forceRemove)
            _tempFiles.ForEach(f => RemoveFile(f));

        _dic.Remove(_uniqueNumber, out _);

        if (_dic.IsEmpty) CleanSpace();
    }

    private static string? CreateFile(string fileName, string contents)
    {
        var filePath = _path + "/" + fileName;

        if (!File.Exists(filePath))
        {
            using var writer = new StreamWriter(filePath);
            writer.Write(contents);

            return filePath;
        }

        return null;
    }

    private static void RemoveFile(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(_path);
    }

    internal static void CreateSpace()
    {
        var dirInfo = Directory.CreateDirectory(_path);
        // dirInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
        CreateFile("csconnector.py", _csconnector);
    }

    internal static void CleanSpace()
    {
        _dic.Clear();

        if (Directory.Exists(_path))
            Directory.Delete(_path, true);
    }
}
