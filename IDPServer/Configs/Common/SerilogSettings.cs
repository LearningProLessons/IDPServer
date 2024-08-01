namespace IDPServer.Configs.Common;
public class SerilogSettings
{
    public MinimumLevel MinimumLevel { get; set; } = new MinimumLevel();
}

public class MinimumLevel
{
    public string Default { get; set; } = "Information";
    public Dictionary<string, string> Override { get; set; } = [];
}
