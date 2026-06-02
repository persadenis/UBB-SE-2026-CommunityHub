var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.ChatAndEvents_API_Server>("server")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.AddProject<Projects.ChatAndEvents_Web>("chatandevents-web");

builder.Build().Run();
