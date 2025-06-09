using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaftLabsAssignement.Config;
using RaftLabsAssignement.Services;

var services = new ServiceCollection();

// Register configuration for ReqresApiOptions manually for demo
services.Configure<ReqresApiOptions>(options =>
{
    options.BaseUrl = "https://reqres.in/api/";
    options.CacheExpirationSeconds = 60;
});

// Add logging
services.AddLogging(config => config.AddConsole());

// Register memory cache
services.AddMemoryCache();

// Register HttpClient and ExternalUserService
services.AddHttpClient<IExternalUserService, ExternalUserService>();

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
