using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PythonCaller;

public class Engine
{
    public string Version { get => GetVersion(Runtime); }
    public string Runtime { get; }
    public string FilePath { get; }
    public int MaxWaitTime { get; set; } = -1;
    public TimeSpan TotalExecutionTime { get; private set; }
    public string ErrorMessage { get; private set; }

    public Engine(string filePath)
    {
        Runtime = "python";
        FilePath = filePath;
    }

    public Engine(string runtime, string filePath)
    {
        Runtime = runtime;
        FilePath = filePath;
    }

    public async Task<TResult> CallAsync<TSource, TResult>(TSource source)
    {
        return await Task.Run(() => Call<TSource, TResult>(source));
    }

    public TResult Call<TSource, TResult>(TSource source)
    {
        var result = CallBase<TSource, TResult>(source, true, true);

        if (result is null)
            throw new Exception("Standard Output cannot be null in the specificed python file.");
        else
            return result;
    }

    public async Task CallAsync<TSource>(TSource source)
    {
        await Task.Run(() => Call(source));
    }

    public void Call<TSource>(TSource source)
    {
        _ = CallBase<TSource, object>(source, true, false);
    }

    public async Task<TResult> CallAsync<TResult>()
    {
        return await Task.Run(() => Call<TResult>());
    }

    public TResult Call<TResult>()
    {
        var result = CallBase<object, TResult>(null, false, true);

        if (result is null)
            throw new Exception("Standard Output cannot be null in the specificed python file.");
        else
            return result;
    }

    public async Task CallAsync()
    {
        await Task.Run(() => Call());
    }

    public void Call()
    {
        _ = CallBase<object, object>(null, false, false);
    }

    private TResult? CallBase<TSource, TResult>(TSource source, bool hasStdin, bool hasStdout)
    {
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
                Arguments = FilePath
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
            throw new Exception(ErrorMessage);
        }

        process.Close();
        return result;
    }

    public bool IsRuntimeValid()
    {
        return IsRuntimeValid(Runtime);
    }

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