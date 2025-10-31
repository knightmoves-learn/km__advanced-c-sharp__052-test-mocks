using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HomeEnergyApi.Tests.Extensions
{
    public class WebApplicationFactoryDefaultApiKey : WebApplicationFactory<Program>
    {
        public TestLoggerProvider LoggerProvider { get; } = new();

        protected override void ConfigureClient(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("X-Api-Key", "9a3cac68-ae5e-4862-9188-aad397ccc6fb");

            base.ConfigureClient(client);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(LoggerProvider);

                logging.SetMinimumLevel(LogLevel.Debug);
            });

            builder.ConfigureServices(services =>
            {
                services.AddTransient<IStartupFilter, TestRemoteIpStartupFilter>();
            });
        }
    }

    public class TestRemoteIpMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPAddress _fakeIpAddress;

        public TestRemoteIpMiddleware(RequestDelegate next, string ipAddress = "127.0.0.1")
        {
            _next = next;
            _fakeIpAddress = IPAddress.Parse(ipAddress);
        }

        public async Task Invoke(HttpContext context)
        {
            context.Connection.RemoteIpAddress = _fakeIpAddress;
            await _next(context);
        }
    }

    public class TestRemoteIpStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<TestRemoteIpMiddleware>();
                next(builder);
            };
        }
    }
}