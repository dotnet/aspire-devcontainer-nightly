var builder = DistributedApplication.CreateBuilder(args);

var p = builder.AddParameter("mysecret");

var apiService = builder.AddProject<Projects.HelloAspire_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("FOO", p);

builder.AddProject<Projects.HelloAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
