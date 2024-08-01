namespace IDPServer.Configs.Common;


public class KestrelSettings
{
    public EndpointsSettings Endpoints { get; set; } = new EndpointsSettings();
}

public class EndpointsSettings
{
    public HttpSettings Http { get; set; } = new HttpSettings();
}

public class HttpSettings
{
    public string Url { get; set; }
}
