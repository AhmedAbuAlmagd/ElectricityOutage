using Polly;
using System.Text;
using System.Text.Json;

namespace STA.Electricity.ConsoleApp
{
    internal class Program
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        private static readonly string apiBaseUrl = "http://localhost:5000/api";
        private static readonly Random random = new Random();
        private static readonly SemaphoreSlim rateLimitSemaphore = new SemaphoreSlim(3, 3);
        private static readonly TimeSpan rateLimitDelay = TimeSpan.FromMilliseconds(500);

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== STA Electricity Console Application ===");
            Console.WriteLine("This application runs forever and processes electricity outage incidents.");
            Console.WriteLine("Press Ctrl+C to stop the application.\n");

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.CancelAfter(TimeSpan.FromSeconds(5));
                Console.WriteLine("\nShutdown requested, waiting for tasks to complete...");
            };

            try
            {
                await RunProcessingLoop(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Application stopped gracefully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message} at {DateTime.Now}");
            }
            finally
            {
                httpClient.Dispose();
                rateLimitSemaphore.Dispose();
            }
        }

        private static async Task RunProcessingLoop(CancellationToken cancellationToken)
        {
            int cycleCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                cycleCount++;
                Console.WriteLine($"\n--- Processing Cycle {cycleCount} ---");

                try
                {
                    var incidentTasks = new List<Task>
                    {
                        ProcessCabinIncidents(cancellationToken),
                        ProcessCableIncidents(cancellationToken)
                    };
                    await Task.WhenAll(incidentTasks);
                    Console.WriteLine("Incidents processed successfully, starting sync...");

                    await RunSyncOperations(cancellationToken);

                    Console.WriteLine($"Cycle {cycleCount} completed successfully.");
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in cycle {cycleCount}: {ex.Message} at {DateTime.Now}");
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
            }
        }

        private static async Task ProcessCabinIncidents(CancellationToken cancellationToken)
        {
            await rateLimitSemaphore.WaitAsync(cancellationToken);
            try
            {
                Console.WriteLine("Generating cabin incidents (Source A)...");
                var scenarios = new[] { "planned", "emergency", "global", "mixed" };
                var selectedScenario = scenarios[random.Next(scenarios.Length)];
                var count = random.Next(2, 8);

                var request = new TestDataRequest { Count = count, Scenario = selectedScenario };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await Policy
                    .Handle<HttpRequestException>()
                    .Or<TaskCanceledException>()
                    .Or<TimeoutException>()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt))
                    .ExecuteAsync(() => httpClient.PostAsync($"{apiBaseUrl}/testdata/cabin-incidents", content, cancellationToken));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"✓ Generated {count} cabin incidents ({selectedScenario} scenario)");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to generate cabin incidents: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error generating cabin incidents: {ex.Message} at {DateTime.Now}");
            }
            finally
            {
                rateLimitSemaphore.Release();
                await Task.Delay(rateLimitDelay, cancellationToken);
            }
        }

        private static async Task ProcessCableIncidents(CancellationToken cancellationToken)
        {
            await rateLimitSemaphore.WaitAsync(cancellationToken);
            try
            {
                Console.WriteLine("Generating cable incidents (Source B)...");
                var scenarios = new[] { "planned", "emergency", "global", "mixed" };
                var selectedScenario = scenarios[random.Next(scenarios.Length)];
                var count = random.Next(2, 6);

                var request = new TestDataRequest { Count = count, Scenario = selectedScenario };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await Policy
                    .Handle<HttpRequestException>()
                    .Or<TaskCanceledException>()
                    .Or<TimeoutException>()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt))
                    .ExecuteAsync(() => httpClient.PostAsync($"{apiBaseUrl}/testdata/cable-incidents", content, cancellationToken));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"✓ Generated {count} cable incidents ({selectedScenario} scenario)");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to generate cable incidents: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error generating cable incidents: {ex.Message} at {DateTime.Now}");
            }
            finally
            {
                rateLimitSemaphore.Release();
                await Task.Delay(rateLimitDelay, cancellationToken);
            }
        }

        private static async Task RunSyncOperations(CancellationToken cancellationToken)
        {
            await rateLimitSemaphore.WaitAsync(cancellationToken);
            try
            {
                Console.WriteLine("Running sync operations...");
                var syncTasks = new[]
                {
                    CallSyncEndpoint("A", cancellationToken),
                    CallSyncEndpoint("B", cancellationToken)
                };

                await Task.WhenAll(syncTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error executing sync operations: {ex.Message} at {DateTime.Now}");
            }
            finally
            {
                rateLimitSemaphore.Release();
                await Task.Delay(rateLimitDelay, cancellationToken);
            }
        }

        private static async Task CallSyncEndpoint(string source, CancellationToken cancellationToken)
        {
            try
            {
                var response = await Policy
                    .Handle<HttpRequestException>()
                    .Or<TaskCanceledException>()
                    .Or<TimeoutException>()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt))
                    .ExecuteAsync(() => httpClient.PostAsync($"{apiBaseUrl}/sync?source={source}", null, cancellationToken));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    if (result.TryGetProperty("Success", out var success) && success.GetBoolean())
                    {
                        var message = result.TryGetProperty("Message", out var msg) ? msg.GetString() : "Sync completed";
                        Console.WriteLine($"  ✓ {message}");
                        if (result.TryGetProperty("TotalProcessed", out var totalProcessed))
                        {
                            Console.WriteLine($"  Total processed rows for Source {source}: {totalProcessed.GetInt32()}");
                        }
                    }
                    else
                    {
                        var error = result.TryGetProperty("Error", out var err) ? err.GetString() : "Unknown error";
                        Console.WriteLine($"  ✗ Sync failed for Source {source}: {error}");
                    }
                }
                else
                {
                    Console.WriteLine($"  ✗ HTTP error for Source {source}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error syncing Source {source}: {ex.Message} at {DateTime.Now}");
            }
        }
    }

    public class TestDataRequest
    {
        public int Count { get; set; }
        public string Scenario { get; set; } = string.Empty;
    }
}