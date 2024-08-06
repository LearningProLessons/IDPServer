public class AppSettings
{
    public SerilogSettings Serilog { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
    public CorsSettings Cors { get; set; }
}

public class SerilogSettings
{
    public MinimumLevel MinimumLevel { get; set; }
}

public class MinimumLevel
{
    public string Default { get; set; }
    public Dictionary<string, string> Override { get; set; }
}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; }
}

public class CorsSettings
{
    public List<string> AllowedOrigins { get; set; }
}
