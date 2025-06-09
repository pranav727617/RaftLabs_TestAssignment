using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaftLabsAssignement.Config;
using RaftLabsAssignement.Services;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Extensions.Http;

var services = new ServiceCollection();

// Manually configure Reqres API options
services.Configure<ReqresApiOptions>(options =>
{
    options.BaseUrl = "https://reqres.in/api/";
    options.CacheExpirationSeconds = 60;
});

// Logging
services.AddLogging(config => config.AddConsole());

// Memory cache
services.AddMemoryCache();

// HttpClient with Polly and BaseAddress
services.AddHttpClient<IExternalUserService, ExternalUserService>(client =>
{
    client.BaseAddress = new Uri("https://reqres.in/api/");
    client.DefaultRequestHeaders.Add("x-api-key", "reqres-free-v1");
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));


var serviceProvider = services.BuildServiceProvider();

var userService = serviceProvider.GetRequiredService<IExternalUserService>();

var user = await userService.GetUserByIdAsync(2);

if (user != null)
{
    Console.WriteLine($"User: {user.FirstName} {user.LastName}, Email: {user.Email}");
}
else
{
    Console.WriteLine("User not found.");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
