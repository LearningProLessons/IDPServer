using IDPServer;
using IDPServer.Configs;
using Serilog;

Log.Logger = new LoggerConfiguration()
    // .WriteTo.Console()
    .CreateBootstrapLogger();

Console.WriteLine("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        // .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(5000); // Listen on all IPs on port 5000
    });

    // Bind configuration to AppSettings model
    var appSettings = new AppSettings();
    builder.Configuration.Bind(appSettings);

    // Add AppSettings to the DI container
    builder.Services.AddSingleton(appSettings);

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();

    // this seeding is only for the template to bootstrap the DB and users.
    // in production you will likely want a different approach.
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