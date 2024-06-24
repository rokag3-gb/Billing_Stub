namespace Collector_AWS.Net;

public class HttpMessage
{
    public string _url = "http://20.214.207.25:5000";
    public string? _contentType = null; // "text/json"
    public string? _accept = null; // "application/json"
    public string? _authorization = null; // 인증 헤더

    private const int defaultTimeout = 20 * 1000; // 20 sec
    private CancellationTokenSource cancellationTokenSource = null;

    public HttpMessage(string url, string? contentType, string? accept, string? authorization)
    {
        _url = url;
        _contentType = contentType;
        _accept = accept;
        _authorization = authorization;
    }

    public async Task<string> HttpGet(string path, string? query = null)
    {
        //string url = string.Concat(URL, $"{Path}?DealDateStart={DealDateStart.ToString("yyyy-MM-dd")}&DealDateEnd={DealDateEnd.ToString("yyyy-MM-dd")}".Replace("//", "/"));
        string returnData = string.Empty;

        try
        {
            var req = (HttpWebRequest)WebRequest.Create(_url + "/" + path + ((query != null) ? "?" + query : "")
                );

            req.Timeout = req.ContinueTimeout = req.ReadWriteTimeout = defaultTimeout;
            req.Method = "GET";
            req.ContentType = _contentType; // "text/json"
            req.Accept = _accept; // "application/json"

            if (_authorization != null)
                req.Headers.Add("Authorization", _authorization);

            req.KeepAlive = false;

            using (var res = (HttpWebResponse)req.GetResponse())
            {
                using (var resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    returnData += resStream.ReadToEnd() + "\r\n";
                }
            }
            //Console.WriteLine(returnData);
            return returnData;
        }
        catch (WebException ex)
        {
            Console.WriteLine(ex.Message);

            using (var res = ex.Response)
            {
                if ((HttpWebResponse)res == null)
                {
                    returnData += "-- Exception --\r\n" + ex.Message.ToString();
                }
                else
                {
                    using (var resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        returnData += "-- Exception --\r\n" + resStream.ReadToEnd();
                    }
                }
            }

            return returnData;
        }
        finally
        {
        }
    }

    public async Task HttpPost(string path, string? body = null)
    {
        //string url = string.Concat(URL, $"{Path}".Replace("//", "/"));
        string returnData = string.Empty;

        try
        {
            var req = (HttpWebRequest)WebRequest.Create(_url + "/" + path);

            req.Timeout = req.ContinueTimeout = req.ReadWriteTimeout = defaultTimeout;
            req.Method = "POST";
            req.ContentType = _contentType; // "text/json"
            req.Accept = _accept; // "application/json"

            if (_authorization != null)
            {
                req.Headers.Add("Authorization", _authorization);
                //req.Headers.Add("Authorization", "PG_BIZAPI_KEY 2b3a65d00d1dc166f605a184734f69ee5d4efe");
            }

            req.KeepAlive = false;

            if (body != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(body);
                req.ContentLength = bytes.Length;

                // POST body
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bytes, 0, bytes.Length);
                }
            }

            // GetResponse
            using (var res = (HttpWebResponse)req.GetResponse())
            {
                using (var resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    returnData += resStream.ReadToEnd() + "\r\n";
                }
            }

            Console.WriteLine(returnData);
        }
        catch (WebException ex)
        {
            Console.WriteLine(ex.Message);

            using (var res = ex.Response)
            {
                var res2 = (HttpWebResponse)res;

                if ((HttpWebResponse)res == null)
                {
                    returnData += "-- Exception --\r\n" + ex.Message.ToString();
                }
                else
                {
                    using (var resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        returnData += "-- Exception --\r\n" + resStream.ReadToEnd();
                    }
                }
            }
        }
        finally
        {
        }
    }
    public async Task HttpPost(string path, byte[] bodyBytes = null)
    {
        //string url = string.Concat(URL, $"{Path}".Replace("//", "/"));
        string returnData = string.Empty;

        try
        {
            var req = (HttpWebRequest)WebRequest.Create(_url + "/" + path);

            req.Timeout = req.ContinueTimeout = req.ReadWriteTimeout = defaultTimeout;
            req.Method = "POST";
            req.ContentType = _contentType; // "text/json"
            req.Accept = _accept; // "application/json"

            if (_authorization != null)
            {
                req.Headers.Add("Authorization", _authorization);
                //req.Headers.Add("Authorization", "PG_BIZAPI_KEY 2b3a65d00d1dc166f605a184734f69ee5d4efe");
            }

            req.KeepAlive = false;

            if (bodyBytes != null)
            {
                //byte[] bytes = Encoding.ASCII.GetBytes(body);
                //byte[] bytes = Encoding.UTF8.GetBytes(body);
                req.ContentLength = bodyBytes.Length;

                // POST body
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bodyBytes, 0, bodyBytes.Length);
                }
            }

            // GetResponse
            using (var res = (HttpWebResponse)req.GetResponse())
            {
                using (var resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    returnData += resStream.ReadToEnd() + "\r\n";
                }
            }

            Console.WriteLine(returnData);
        }
        catch (WebException ex)
        {
            Console.WriteLine(ex.Message);

            using (var res = ex.Response)
            {
                var res2 = (HttpWebResponse)res;

                if ((HttpWebResponse)res == null)
                {
                    returnData += "-- Exception --\r\n" + ex.Message.ToString();
                }
                else
                {
                    using (var resStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        returnData += "-- Exception --\r\n" + resStream.ReadToEnd();
                    }
                }
            }
        }
        finally
        {
        }
    }
}