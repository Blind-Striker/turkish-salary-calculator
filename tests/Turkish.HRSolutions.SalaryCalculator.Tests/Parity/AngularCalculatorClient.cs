using System.Diagnostics;
using System.IO.Abstractions;
using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
using TUnit.Core.Interfaces;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Parity;

/// <summary>
/// Client for calling Angular salary calculator CLI tool.
/// Thread-safe and designed for parallel test execution.
/// </summary>
public sealed class AngularCalculatorClient : IAsyncInitializer, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IFileSystem _fileSystem;
    private string? _cliDirectory;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="AngularCalculatorClient"/> class.
    /// </summary>
    public AngularCalculatorClient()
        : this(new FileSystem())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AngularCalculatorClient"/> class.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction.</param>
    public AngularCalculatorClient(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Initializes the client by verifying prerequisites and installing npm dependencies.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            var repoRoot = ResolveRepoRoot();
            _cliDirectory = _fileSystem.Path.Combine(repoRoot, "tests", "tools", "angular-parity-fixtures");

            // Verify Angular submodule exists
            var externalDir = _fileSystem.Path.Combine(repoRoot, "external", "maas-hesaplama");
            if (!_fileSystem.Directory.Exists(externalDir))
            {
                throw new InvalidOperationException(
                    "Angular submodule not found at external/maas-hesaplama. " +
                    "Run 'git submodule update --init --recursive' first.");
            }

            // Verify CLI tool exists
            var calculateTsPath = _fileSystem.Path.Combine(_cliDirectory, "src", "calculate.ts");
            if (!_fileSystem.File.Exists(calculateTsPath))
            {
                throw new InvalidOperationException(
                    "Angular calculator CLI not found at " + calculateTsPath);
            }

            // Ensure node_modules is installed
            await EnsureNodeModulesAsync();

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Calculates salary using Angular's calculator via CLI.
    /// </summary>
    /// <param name="input">Calculation input parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Angular CLI response containing calculation output.</returns>
    public async Task<AngularCliResponse> CalculateAsync(
        TestInput input,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Client not initialized. Call InitializeAsync() first.");
        }

        var inputJson = JsonSerializer.Serialize(input, JsonOptions);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await Cli.Wrap("npx")
                .WithArguments(["ts-node", "--transpile-only", "src/calculate.ts"])
                .WithWorkingDirectory(_cliDirectory!)
                .WithStandardInputPipe(PipeSource.FromString(inputJson))
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync(cancellationToken);

            stopwatch.Stop();

            if (string.IsNullOrWhiteSpace(result.StandardOutput))
            {
                return new AngularCliResponse
                {
                    Success = false,
                    Error = new AngularCliError
                    {
                        Code = "INTERNAL_ERROR",
                        Message = "No output from CLI",
                        Details = string.IsNullOrWhiteSpace(result.StandardError)
                            ? null
                            : [result.StandardError]
                    },
                    ElapsedMs = stopwatch.ElapsedMilliseconds
                };
            }

            var response = JsonSerializer.Deserialize<AngularCliResponse>(result.StandardOutput, JsonOptions);
            if (response is null)
            {
                return new AngularCliResponse
                {
                    Success = false,
                    Error = new AngularCliError
                    {
                        Code = "INTERNAL_ERROR",
                        Message = "Failed to deserialize CLI response",
                        Details = [result.StandardOutput]
                    },
                    ElapsedMs = stopwatch.ElapsedMilliseconds
                };
            }

            response.ElapsedMs = stopwatch.ElapsedMilliseconds;
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new AngularCliResponse
            {
                Success = false,
                Error = new AngularCliError
                {
                    Code = "INTERNAL_ERROR",
                    Message = ex.Message,
                    Details = ex.StackTrace is not null ? [ex.StackTrace] : null
                },
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private async Task EnsureNodeModulesAsync()
    {
        var nodeModulesPath = _fileSystem.Path.Combine(_cliDirectory!, "node_modules");
        if (_fileSystem.Directory.Exists(nodeModulesPath))
        {
            return;
        }

        // Run npm ci (or npm install if no lock file)
        var lockFilePath = _fileSystem.Path.Combine(_cliDirectory!, "package-lock.json");
        var installCommand = _fileSystem.File.Exists(lockFilePath) ? "ci" : "install";

        var result = await Cli.Wrap("npm")
            .WithArguments(installCommand)
            .WithWorkingDirectory(_cliDirectory!)
            .ExecuteBufferedAsync();

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "npm " + installCommand + " failed: " + result.StandardError);
        }
    }

    private string ResolveRepoRoot()
    {
        var current = _fileSystem.DirectoryInfo.New(_fileSystem.Directory.GetCurrentDirectory());

        while (current is not null)
        {
            var slnPath = _fileSystem.Path.Combine(current.FullName, "TurkishHRSolutions.sln");
            if (_fileSystem.File.Exists(slnPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate repository root (TurkishHRSolutions.sln).");
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        _initLock.Dispose();
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Response from Angular calculator CLI.
/// </summary>
public sealed record AngularCliResponse
{
    /// <summary>
    /// Gets or sets whether the calculation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the input that was sent to the CLI.
    /// </summary>
    public TestInput? Input { get; set; }

    /// <summary>
    /// Gets or sets the calculation output.
    /// </summary>
    public TestOutput? Output { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the calculation was performed.
    /// </summary>
    public string? CalculatedAt { get; set; }

    /// <summary>
    /// Gets or sets error details if the calculation failed.
    /// </summary>
    public AngularCliError? Error { get; set; }

    /// <summary>
    /// Gets or sets elapsed time for the CLI call in milliseconds.
    /// </summary>
    public long ElapsedMs { get; set; }
}

/// <summary>
/// Error details from Angular calculator CLI.
/// </summary>
public sealed record AngularCliError
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets additional error details.
    /// </summary>
    public IReadOnlyList<string>? Details { get; init; }
}
