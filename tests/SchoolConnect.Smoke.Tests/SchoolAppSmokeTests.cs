using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace SchoolConnect.Smoke.Tests;

[TestFixture]
public sealed class SchoolAppSmokeTests
{
    private static readonly Lazy<Task<SmokeAppHost>> AppHost = new(StartAppAsync);

    [Test]
    public async Task HomePage_RendersCoreNavigationAndAdmissionsCallToAction()
    {
        var host = await AppHost.Value;
        var response = await host.Client.GetAsync("/");

        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(body, Does.Contain("Sri Venkateswara Convent"));
        Assert.That(body, Does.Contain("Admissions"));
        Assert.That(body, Does.Contain("Why Choose Us"));
        Assert.That(body, Does.Contain("Latest Updates"));
        Assert.That(body, Does.Contain("Start Enquiry"));
        Assert.That(body, Does.Contain("Admission Form"));
    }

    [Test]
    public async Task AdmissionsPage_RendersLeadCaptureForm()
    {
        var host = await AppHost.Value;
        var response = await host.Client.GetAsync("/admissions");

        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(body, Does.Contain("Apply for admission"));
        Assert.That(body, Does.Contain("Submit admission"));
        Assert.That(body, Does.Contain("Student full name"));
    }

    [Test]
    public async Task LoginPage_RendersRoleSelectionAndCredentialInputs()
    {
        var host = await AppHost.Value;
        var response = await host.Client.GetAsync("/login");

        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(body, Does.Contain("Student Login"));
        Assert.That(body, Does.Contain("Teacher Login"));
        Assert.That(body, Does.Contain("PIN"));
        Assert.That(body, Does.Contain("Password"));
    }

    [OneTimeTearDown]
    public async Task TearDown() => await DisposeHostAsync();

    private static async Task<SmokeAppHost> StartAppAsync()
    {
        var port = GetFreeTcpPort();
        var baseUrl = $"http://127.0.0.1:{port}";
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(repoRoot, "src", "SchoolConnect.Web", "SchoolConnect.Web", "SchoolConnect.Web.csproj");

        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(projectPath);
        startInfo.ArgumentList.Add("--no-launch-profile");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("-p:UseSharedCompilation=false");
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(baseUrl);

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start the school web app.");
        var output = new StringWriter();
        var error = new StringWriter();

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                output.WriteLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                error.WriteLine(args.Data);
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true })
        {
            BaseAddress = new Uri(baseUrl)
        };

        await WaitForReadyAsync(client, process, baseUrl, output, error);
        return new SmokeAppHost(process, client, baseUrl, output, error);
    }

    private static async Task WaitForReadyAsync(HttpClient client, Process process, string baseUrl, StringWriter output, StringWriter error)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));

        while (!timeout.IsCancellationRequested)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException($"The web app exited before it became ready.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
            }

            try
            {
                using var response = await client.GetAsync("/", timeout.Token);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException) when (!timeout.IsCancellationRequested)
            {
            }

            await Task.Delay(500, timeout.Token);
        }

        throw new TimeoutException($"Timed out waiting for the school web app to start at {baseUrl}.\nSTDOUT:\n{output}\nSTDERR:\n{error}");
    }

    private static async Task DisposeHostAsync()
    {
        if (!AppHost.IsValueCreated)
        {
            return;
        }

        if (AppHost.Value.IsCompletedSuccessfully)
        {
            var host = await AppHost.Value;
            host.Dispose();
        }
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SchoolConnect.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root from the test output directory.");
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private sealed class SmokeAppHost : IDisposable
    {
        private readonly StringWriter output;
        private readonly StringWriter error;

        public SmokeAppHost(Process process, HttpClient client, string baseUrl, StringWriter output, StringWriter error)
        {
            Process = process;
            Client = client;
            BaseUrl = baseUrl;
            this.output = output;
            this.error = error;
        }

        public Process Process { get; }

        public HttpClient Client { get; }

        public string BaseUrl { get; }

        public void Dispose()
        {
            Client.Dispose();

            if (!Process.HasExited)
            {
                try
                {
                    Process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                }
            }

            Process.Dispose();
            output.Dispose();
            error.Dispose();
        }
    }
}
