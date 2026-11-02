using System.Diagnostics;

const string ApiUrl = "https://localhost:7169/";
const string WebUrl = "https://localhost:7106/";

var noBuild = args.Contains("--no-build");
var noBrowser = args.Contains("--no-browser");
var configuration = args.Contains("--release") ? "Release" : "Debug";

var root = FindSolutionRoot();
if (root is null)
{
    Log("LAUNCHER", ConsoleColor.Red,
        "Could not find Arniston_Letting.slnx. Run this from inside the repository.");
    return 1;
}

var solution = Path.Combine(root, "Arniston_Letting.slnx");
var apiProject = Path.Combine(root, "Arniston_Letting_Api", "Arniston_Letting_Api.csproj");
var webProject = Path.Combine(root, "Arniston_Letting_Front", "Arniston_Letting_Front.csproj");

using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    shutdown.Cancel();
};

if (RunToCompletion("dotnet", "dev-certs https --check", quiet: true) != 0)
{
    Log("LAUNCHER", ConsoleColor.Yellow,
        "No trusted HTTPS dev certificate found. If the browser warns about the " +
        "certificate, run:  dotnet dev-certs https --trust");
}

if (!noBuild)
{
    Log("LAUNCHER", ConsoleColor.Cyan, $"Building solution ({configuration})...");

    if (RunToCompletion("dotnet", $"build \"{solution}\" --configuration {configuration}") != 0)
    {
        Log("LAUNCHER", ConsoleColor.Red, "Build failed. Fix the errors above and try again.");
        return 1;
    }
}

var processes = new List<Process>();

try
{
    var api = StartService("API", ConsoleColor.Green, apiProject, new Dictionary<string, string>());
    processes.Add(api);

    Log("LAUNCHER", ConsoleColor.Cyan, "Waiting for the API to become ready...");

    if (!await WaitForUrl(ApiUrl + "health", api, TimeSpan.FromSeconds(90), shutdown.Token))
    {
        Log("LAUNCHER", ConsoleColor.Red,
            "The API did not start. Check its output above (database connection string and " +
            "MySQL running are the usual causes).");
        return 1;
    }

    Log("LAUNCHER", ConsoleColor.Cyan, $"API is ready at {ApiUrl}");

    var web = StartService("WEB", ConsoleColor.Magenta, webProject, new Dictionary<string, string>
    {
        ["ApiSettings__BaseUrl"] = ApiUrl
    });
    processes.Add(web);

    if (await WaitForUrl(WebUrl, web, TimeSpan.FromSeconds(60), shutdown.Token))
    {
        Log("LAUNCHER", ConsoleColor.Cyan, $"Site is ready at {WebUrl}  (press Ctrl+C to stop)");

        if (!noBrowser)
        {
            OpenBrowser(WebUrl);
        }
    }
    else
    {
        Log("LAUNCHER", ConsoleColor.Red, "The frontend did not start. Check its output above.");
        return 1;
    }

    await Task.WhenAny(
        api.WaitForExitAsync(),
        web.WaitForExitAsync(),
        Task.Delay(Timeout.Infinite, shutdown.Token));

    if (!shutdown.IsCancellationRequested)
    {
        Log("LAUNCHER", ConsoleColor.Yellow, "A service exited unexpectedly. Shutting down.");
    }

    return 0;
}
finally
{
    foreach (var process in processes)
    {
        StopProcess(process);
    }

    Log("LAUNCHER", ConsoleColor.Cyan, "Stopped.");
}

string? FindSolutionRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

    while (directory != null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Arniston_Letting.slnx")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    return null;
}

int RunToCompletion(string fileName, string arguments, bool quiet = false)
{
    var startInfo = new ProcessStartInfo(fileName, arguments)
    {
        UseShellExecute = false,
        RedirectStandardOutput = quiet,
        RedirectStandardError = quiet
    };

    using var process = Process.Start(startInfo);

    if (process is null)
    {
        return -1;
    }

    if (quiet)
    {
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }

    process.WaitForExit();
    return process.ExitCode;
}

Process StartService(
    string name,
    ConsoleColor color,
    string project,
    Dictionary<string, string> environment)
{
    var startInfo = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = Path.GetDirectoryName(project)!,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    startInfo.ArgumentList.Add("run");
    startInfo.ArgumentList.Add("--no-build");
    startInfo.ArgumentList.Add("--configuration");
    startInfo.ArgumentList.Add(configuration);
    startInfo.ArgumentList.Add("--project");
    startInfo.ArgumentList.Add(project);
    startInfo.ArgumentList.Add("--launch-profile");
    startInfo.ArgumentList.Add("https");

    foreach (var (key, value) in environment)
    {
        startInfo.Environment[key] = value;
    }

    var process = new Process { StartInfo = startInfo };

    process.OutputDataReceived += (_, e) =>
    {
        if (e.Data != null) Log(name, color, e.Data);
    };

    process.ErrorDataReceived += (_, e) =>
    {
        if (e.Data != null) Log(name, ConsoleColor.Red, e.Data);
    };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    Log("LAUNCHER", ConsoleColor.Cyan, $"Started {name} (pid {process.Id})");

    return process;
}

async Task<bool> WaitForUrl(string url, Process process, TimeSpan timeout, CancellationToken token)
{
    using var handler = new HttpClientHandler
    {
        AllowAutoRedirect = false,
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    };
    using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(3) };

    var deadline = DateTime.UtcNow + timeout;

    while (DateTime.UtcNow < deadline && !token.IsCancellationRequested)
    {
        if (process.HasExited)
        {
            return false;
        }

        try
        {
            using var response = await client.GetAsync(url, token);
            return true;
        }
        catch (Exception) when (!token.IsCancellationRequested)
        {
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        try
        {
            await Task.Delay(1000, token);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    return false;
}

void StopProcess(Process process)
{
    try
    {
        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(5000);
        }
    }
    catch (Exception)
    {
    }
    finally
    {
        process.Dispose();
    }
}

void OpenBrowser(string url)
{
    try
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
    catch (Exception)
    {
        Log("LAUNCHER", ConsoleColor.Yellow, $"Open {url} in your browser.");
    }
}

void Log(string source, ConsoleColor color, string message)
{
    lock (Console.Out)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write($"[{source,-8}] ");
        Console.ForegroundColor = previous;
        Console.WriteLine(message);
    }
}