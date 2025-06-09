using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RaftLabsAssignement.Config;
using RaftLabsAssignement.Services;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Extensions.Http;
//using RaftLabsAssignement.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<ReqresApiOptions>(context.Configuration.GetSection("ReqresApi"));

        services.AddMemoryCache();

        services.AddHttpClient<IExternalUserService, ExternalUserService>();

        services.AddHttpClient<IExternalUserService, ExternalUserService>()
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddLogging(configure => configure.AddConsole());
    })
    .Build();

var userService = host.Services.GetRequiredService<IExternalUserService>();

var user = await userService.GetUserByIdAsync(2);
Console.WriteLine($"User: {user?.FirstName} {user?.LastName} ({user?.Email})");
