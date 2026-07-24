using cloudflare_dns_container;
using cloudflare_dns_container.Models;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<CloudflareDnsSettings>(context.Configuration.GetSection("CloudflareDnsSettings"));
        services.AddHostedService<Worker>();
    });

var host = builder.Build();
host.Run();
