var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposePublisher();

var apiService = builder.AddProject<Projects.HelloAspire_ApiService>("apiservice")
    .WithHttpsHealthCheck("/health");

builder.AddProject<Projects.HelloAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpsHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

var frontend = builder.AddNpmApp("frontend", "../frontend", "dev")
                      .WithHttpEndpoint(targetPort: 3000, env: "PORT")
                      .WithReference(apiService)
                      .PublishAsDockerFile();

var frontendInstall = builder.AddExecutable("frontend-install", "npm", "../frontend", "install")
    .WithParentRelationship(frontend);

frontend.WaitForCompletion(frontendInstall);

builder.Build().Run();
