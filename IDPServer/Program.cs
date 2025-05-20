using IDPServer;
using Microsoft.AspNetCore.DataProtection;
using Serilog;
using System;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Console.WriteLine("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/Users/mohammadnazari/.aspnet/DataProtection-Keys"))
        .SetApplicationName("IDPServer");

    
    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(
            outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    
    builder.Services.ConfigureServices(builder.Configuration);

    var app = builder.Build();
    app.ConfigurePipeline();
    if (args.Contains("/seed"))
    {
        Console.WriteLine("Seeding database...");
        await SeedData.EnsureSeedDataAsync(app);
        Console.WriteLine("Done seeding database. Exiting.");
        return;
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Console.WriteLine(ex.Message);
}
finally
{
    Console.WriteLine("Shut down complete");
    // Log.CloseAndFlush();
}