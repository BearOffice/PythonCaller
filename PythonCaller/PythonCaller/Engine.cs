using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PythonCaller;

/// <summary>
/// Engine for calling python.
/// </summary>
public class Engine
{
    /// <summary>
    /// Python version.
    /// </summary>
    public string Version { get => GetVersion(Runtime); }
    /// <summary>
    /// Python runtime path.
    /// </summary>
    public string Runtime { get; }
    /// <summary>
    /// Max wait time for the python process.
    /// </summary>
    public int MaxWaitTime { get; set; } = -1;
    /// <summary>
    /// Total execution time of the called python process.
    /// </summary>
    public TimeSpan TotalExecutionTime { get; private set; }
    /// <summary>
    /// Error message from the called python process.
    /// </summary>
    public string ErrorMessage { get; private set; } = "";
    private string? _filePath;
    private readonly Scope? _scope;

    public Engine(string filePath)
    {
        _filePath = filePath;
        Runtime = "python";
    }

    public Engine(string filePath, string runtime)
    {
        _filePath = filePath;
        Runtime = runtime;
    }

    public Engine(Scope scope)
    {
        _scope = scope;
        Runtime = "python";
    }

    public Engine(Scope scope, string runtime)
    {
        _scope = scope;
        Runtime = runtime;
    }

    /// <summary>
    /// Call python with input/output asynchronously.
    /// </summary>
    /// <typeparam name="TSource">Type of input</typeparam>
    /// <typeparam name="TResult">Type of output</typeparam>
    /// <param name="source">Input</param>
    /// <returns><see cref="Task{TResult}"/></returns>
    public async Task<TResult> CallAsync<TSource, TResult>(TSource source)
    {
        return await Task.Run(() => Call<TSource, TResult>(source));
    }

    /// <summary>
    /// Call python with input/output.
    /// </summary>
    /// <typeparam name="TSource">Type of input</typeparam>
    /// <typeparam name="TResult">Type of output</typeparam>
    /// <param name="source">Input</param>
    /// <returns>Output</returns>
    public TResult Call<TSource, TResult>(TSource source)
    {
        var result = CallBase<TSource, TResult>(source, true, true);

        if (result is null)
            throw new Exception("Standard Output cannot be null in the specificed python file.");
        else
            return result;
    }

    /// <summary>
    /// Call python with input asynchronously.
    /// </summary>
    /// <typeparam name="TSource">Type of input</typeparam>
    /// <param name="source">Input</param>
    /// <returns><see cref="Task"/></returns>
    public async Task CallAsync<TSource>(TSource source)
    {
        await Task.Run(() => Call(source));
    }

    /// <summary>
    /// Call python with input.
    /// </summary>
    /// <typeparam name="TSource">Type of input</typeparam>
    /// <param name="source">Input</param>
    public void Call<TSource>(TSource source)
    {
        _ = CallBase<TSource, object>(source, true, false);
    }

    /// <summary>
    /// Call python with output asynchronously.
    /// </summary>
    /// <typeparam name="TResult">Type of output</typeparam>
    /// <returns><see cref="Task{TResult}"/></returns>
    public async Task<TResult> CallAsync<TResult>()
    {
        return await Task.Run(() => Call<TResult>());
    }

    /// <summary>
    /// Call python with output.
    /// </summary>
    /// <typeparam name="TResult">Type of output</typeparam>
    /// <returns>Output</returns>
    public TResult Call<TResult>()
    {
        var result = CallBase<object, TResult>(null, false, true);

        if (result is null)
            throw new Exception("Standard Output cannot be null in the specificed python file.");
        else
            return result;
    }

    /// <summary>
    /// Call python without input/output asynchronously.
    /// </summary>
    /// <returns><see cref="Task"/></returns>
    public async Task CallAsync()
    {
        await Task.Run(() => Call());
    }

    /// <summary>
    /// Call python without input/output.
    /// </summary>
    public void Call()
    {
        _ = CallBase<object, object>(null, false, false);
    }

    private TResult? CallBase<TSource, TResult>(TSource? source, bool hasStdin, bool hasStdout)
    { 
        InitializeScopeIfNeeded(hasStdin, hasStdout);

        TotalExecutionTime = default;
        ErrorMessage = "";

        var serializer = new JsonSerializer
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        var process = new Process
        {
            StartInfo = new ProcessStartInfo(Runtime)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = _filePath
            }
        };

        process.Start();
        if (MaxWaitTime > 0)
        {
            Task.Run(() =>
            {
                var result = process.WaitForExit(MaxWaitTime);
                if (!result) throw new Exception("Time out.");
            });
        }

        using (var writer = new JsonTextWriter(process.StandardInput))
        {
            if (hasStdin)
                serializer.Serialize(writer, source);
            else
                serializer.Serialize(writer, "");
        }

        var result = default(TResult);
        if (hasStdout)
        {
            using var reader = new JsonTextReader(process.StandardOutput);
            result = serializer.Deserialize<TResult>(reader);
        }

        process.WaitForExit();
        TotalExecutionTime = process.ExitTime - process.StartTime;

        using var errReader = process.StandardError;
        ErrorMessage = errReader.ReadToEnd();

        if (process.ExitCode != 0)
        {
            process.Close();
            CleanScopeIfNeeded();
            throw new Exception(ErrorMessage);
        }

        process.Close();
        CleanScopeIfNeeded();
        return result;
    }

    private void InitializeScopeIfNeeded(bool hasStdin, bool hasStdout)
    {
        if (_scope is not null)
            _filePath = _scope.Initialize(hasStdin, hasStdout);
    }

    private void CleanScopeIfNeeded()
    {
        if (_scope is not null)
        {
            _scope.Clean();
            _filePath = null;
        }
    }

    /// <summary>
    /// Check if runtime is valid
    /// </summary>
    /// <returns><see langword="true"/> if runtime is valid; otherwise, <see langword="false"/>.</returns>
    public bool IsRuntimeValid()
    {
        return IsRuntimeValid(Runtime);
    }

    /// <summary>
    /// Get python version.
    /// </summary>
    /// <param name="runtime"></param>
    /// <returns>Python version</returns>
    public static string GetVersion(string runtime)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo(runtime)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = "--version"
            }
        };

        process.Start();

        var version = process.StandardOutput.ReadToEnd().Remove('\n');

        process.Close();

        return version;
    }

    /// <summary>
    /// Check if runtime is valid
    /// </summary>
    /// <param name="runtime">Runtime path</param>
    /// <returns><see langword="true"/> if runtime is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsRuntimeValid(string runtime)
    {
        try
        {
            _ = GetVersion(runtime);
            return true;
        }
        catch
        {
            return false;
        }
    }
}