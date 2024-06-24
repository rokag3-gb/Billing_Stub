namespace Tmon.Collector;

public class DapperContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public DapperContext()
    {
    }
    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection(string _conn_str) => new SqlConnection(_conn_str);
}