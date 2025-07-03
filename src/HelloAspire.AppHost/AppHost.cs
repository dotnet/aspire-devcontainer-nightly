using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.HelloAspire_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.HelloAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddExecutable("ping", "echo", ".", ["google.com"])
    .OnBeforeResourceStarted(async (r, e, ct) =>
    {
        _ = Task.Run(async () =>
        {
            var logger = e.Services.GetRequiredService<ILogger<Program>>();
            var rns = e.Services.GetRequiredService<ResourceNotificationService>();
            var rls = e.Services.GetRequiredService<ResourceLoggerService>();

            _ = Task.Run(async () =>
            {
                await rns.WaitForResourceAsync(r.Name, (re) =>
                {
                    return re.Snapshot.State == KnownResourceStates.Finished;
                }, ct);

                // Uncomment this to make the output show up.
                //await Task.Delay(1000, ct);

                rls.Complete(r);
            }, cancellationToken: ct);

            var batches = rls.WatchAsync(r);
            await foreach (var batch in batches.WithCancellation(ct))
            {
                foreach (var line in batch)
                {
                    logger.LogCritical(line.Content);
                }
            }

            logger.LogCritical("Ping completed!");
        }, cancellationToken: ct);
    });

builder.Build().Run();
