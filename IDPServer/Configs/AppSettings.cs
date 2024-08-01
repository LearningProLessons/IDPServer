

using IDPServer.Configs.Common;

namespace IDPServer.Configs;

public class AppSettings
{
    public SerilogSettings Serilog { get; set; } = new SerilogSettings();
    public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    public CorsSettings Cors { get; set; } = new CorsSettings();
}
