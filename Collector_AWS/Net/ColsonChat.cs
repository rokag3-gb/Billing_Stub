namespace Collector_AWS.Net;

public class ColsonChat : IDisposable
{
    private HttpMessage httpMessage;
    private string url = "https://gw.cloudmt.co.kr";
    private string? contentType = "application/json";
    private string? accept = "application/json";
    private string auth = null;
    private bool _disposed = false;

    public ColsonChat() {
        this.auth = "Basic " + Helper.Utf8.str_to_base64string($"{Secret.cm_app_colson_user_username}:{Secret.cm_app_colson_user_password}");

        httpMessage = new HttpMessage(this.url, this.contentType, this.accept, this.auth);
    }
    ~ColsonChat()
    {
        httpMessage = null;
    }

    public bool ColsonChatPost_to_User(string user, string message)
    {
        bool result = false;

        try
        {
            //var body = new
            //{
            //    user = user,
            //    message = message,
            //};

            //var bodyJson = JsonConvert.SerializeObject(body);

            string? bodyJson = $"{{\"user\":\"{user}\",\"message\":\"{message}\"}}";

            httpMessage.HttpPost("colson/api/sendUserMessage", bodyJson);

            result = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
        }

        return true;
    }

    record sendUserMessageBody(string user, string message);
    record sendGroupMessageBody (string id, string message);

    //[JsonSerializable(typeof(sendGroupMessageBody))]
    //[JsonSerializable(typeof(string))]
    //[JsonSerializable(typeof(string))]
    //public partial class sendGroupMessageBodyContext : JsonSerializerContext
    //{
    //}

    //[JsonSourceGenerationOptions(WriteIndented = true)]
    //[JsonSerializable(typeof(sendGroupMessageBody))]
    //internal partial class SourceGenerationContext : JsonSerializerContext
    //{
    //}

    public bool ColsonChatPost_to_Group(string groupId, string message)
    {
        bool result = false;

        try
        {
            // 외부 HTTP API 요청을 위한 body를 바이트 배열로 만든다.
            //byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { id = groupId, message = message }));

            //sendGroupMessageBody body = new(groupId, message);
            //sendGroupMessageBodyContext d = new sendGroupMessageBodyContext();
            
            //sendGroupMessageBody body = new
            //{
            //    id = groupId,
            //    message = message,
            //};

            //string? bodyJson = JsonConvert.SerializeObject(body);
            string? bodyJson = $"{{\"id\":\"{groupId}\",\"message\":\"{message}\"}}";

            httpMessage.HttpPost("colson/api/sendGroupMessage", bodyJson);

            result = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
        }

        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // 관리되는 리소스 정리
            httpMessage = null;
        }

        // 관리되지 않는 리소스 정리
        // ...

        _disposed = true;
    }
}