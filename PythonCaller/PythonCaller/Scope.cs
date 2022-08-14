﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonCaller;

public class Scope
{
    private const string _importCodes = "import csconnector";
    private const string _inputCodes = "input = csconnector.get_input()";
    private const string _outputCodes = "csconnector.set_output(output)";

    private readonly string _scope;
    private Workspace? _workspace;
    private string? _filePath;

    private Scope(string scope)
    {
        _scope = scope;
        _workspace = default;
    }

    public static Scope Create(string scope)
    {
        return new Scope(scope);
    }

    internal string Initialize(bool hasStdin, bool hasStdout)
    {
        if (_filePath is not null)
            return _filePath;

        _workspace = new Workspace();
        _filePath = _workspace.CreateTempFile(SupplyScope(_scope, hasStdin, hasStdout));

        return _filePath;
    }
    
    internal void Clean()
    {
        if (_workspace is not null)
        {
            _workspace.RemoveTempFiles();
            _filePath = null;
        }
    }

    private static string SupplyScope(string scope, bool hasStdin, bool hasStdout)
    {
        string tempStr;

        if (hasStdin)
            tempStr = _importCodes + "\n" + _inputCodes + "\n" + scope;
        else
            tempStr = _importCodes + "\n" + scope;

        if (hasStdout)
            return tempStr + "\n" + _outputCodes;
        else
            return tempStr;
    }
}