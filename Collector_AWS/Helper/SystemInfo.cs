namespace Tmon.Collector;

public static class SystemInfo
{
    public static string getHostname()
    {
        string hostname = string.Empty;

        try
        {
            if (hostname == string.Empty)
                hostname = System.Environment.MachineName;
        }
        catch
        {
        }

        try
        {
            if (hostname == string.Empty)
                hostname = System.Net.Dns.GetHostName();
            if (hostname == string.Empty)
                hostname = System.Net.Dns.GetHostEntry("").HostName;
        }
        catch
        {
        }

        try
        {
            if (hostname == string.Empty)
                hostname = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
        }
        catch
        {
        }

        return hostname;
    }
}