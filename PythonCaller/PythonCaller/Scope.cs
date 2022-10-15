using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonCaller;

/// <summary>
/// Scope for python codes.
/// </summary>
public class Scope
{
    private const string _importCodes = "import csconnector";
    private const string _initCodes = "csconnector.init_environment()";
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

    /// <summary>
    /// Create scope for python codes.
    /// <para>
    /// The words <b>input</b>, <b>output</b> and <b>csconnector</b> are reserved words.
    /// Use <b>input</b> variable to receive input from caller and 
    /// define <b>output</b> variable to send output to caller.
    /// </para>
    /// <example>example:
    /// <code>
    /// ndarr = np.array(input)
    /// ndarr.sort()
    /// output = ndarr
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="scope">Python codes.</param>
    /// <returns><see cref="Scope"/></returns>
    public static Scope Create(string scope)
    {
        return new Scope(scope);
    }

    internal string Initialize(bool hasInput, bool hasOutput)
    {
        if (_filePath is not null)
            return _filePath;

        _workspace = new Workspace();
        _filePath = _workspace.CreateTempFile(SupplyScope(_scope, hasInput, hasOutput));

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

    private static string SupplyScope(string scope, bool hasInput, bool hasOutput)
    {
        var tempStr = _importCodes + "\n" + _initCodes + "\n";

        if (hasInput)
            tempStr += _inputCodes + "\n" + scope;
        else
            tempStr += scope;

        if (hasOutput)
            return tempStr + "\n" + _outputCodes;
        else
            return tempStr;
    }
}
