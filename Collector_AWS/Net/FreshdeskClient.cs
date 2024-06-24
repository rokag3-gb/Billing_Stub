namespace Collector_AWS.Net;

public class FreshdeskClient
{
    private HttpMessage httpMessage;
    private string subDomain = "mate365";
    private string urlPrefix = string.Empty;
    private string? contentType = "application/json";
    private string? accept = "application/json";
    private string authorization = null;

    public FrozenSet<FreshdeskContact> contactList = null;
    public FrozenSet<FreshdeskAgent> agentList = null;
    public FrozenSet<FreshdeskCompany> companyList = null;

    public FreshdeskClient()
    {
        this.urlPrefix = $"https://{this.subDomain}.freshdesk.com/api/v2";
        //this.authorization = "Basic " + Helper.Utf8.str_to_base64string(Encoding.ASCII.GetBytes($"{Secret.freshdeskApiKey}:X").ToString());
        this.authorization = "Basic " + Helper.Utf8.str_to_base64string($"{Secret.freshdeskApiKey}:X");

        httpMessage = new HttpMessage(this.urlPrefix, contentType, accept, this.authorization);
    }

    public async Task<string> GetFreshdeskTicketsAsync(DateTime startUpdatedAt, DateTime endUpdatedAt, int page = 1)
    {
        var start = startUpdatedAt.ToString("yyyy-MM-dd");
        var end = endUpdatedAt.ToString("yyyy-MM-dd");

        // 티켓 목록 가져오기: /api/v2/tickets
        // 특정 티켓 정보 가져오기: /api/v2/tickets/{id}
        // 연락처 목록 가져오기: /api/v2/contacts
        // 고객 목록 가져오기: /api/v2/customers

        try
        {
            if (contactList == null)
                contactList = await GetFreshdeskContactListAsync();

            if (agentList == null)
                agentList = await GetFreshdeskAgentListAsync();

            if (companyList == null)
                companyList = await GetFreshdeskCompanyListAsync();

            //https://mate365.freshdesk.com/api/v2/search/tickets?query="updated_at:>'2024-05-24' and updated_at:<'2024-05-27'"
            //https://mate365.freshdesk.com/api/v2/tickets?updated_since=2024-05-24T00:00:00Z&updated_before=2024-05-27T23:59:59Z&include=description&include=requester&include=company&order_by=updated_at&order_type=desc
            //https://mate365.freshdesk.com/api/v2/tickets?updated_since={start_date}&updated_before={end_date}

            var path = $"search/tickets" +
                $"?" +
                $"query=\"updated_at:>'{start}' and updated_at:<'{end}'\"" +
                //$"&include=description" +
                //$"&include=requester" +
                //$"&include=company" +
                //$"&order_by=updated_at" +
                //$"&order_type=desc" +
                $"";

            path += $"&page={page}";

            //path = path.Replace(" ", "%20");
            Logger.log($"Freshdesk Ticket Url = {httpMessage._url}/{path}");
            
            return await httpMessage.HttpGet(path);
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
        }
        finally
        {
        }

        return "";
    }

    public async Task<FrozenSet<FreshdeskContact>> GetFreshdeskContactListAsync()
    {
        List<FreshdeskContact> list = new();

        int page = 1;

        try
        {
            Logger.log($"Freshdesk Contacts load starting..");

            while (true)
            {
                var path = $"contacts" +
                    $"?" +
                    $"page={page}" +
                    $"";
                //Logger.log($"Url: {httpMessage._url}/{path}");

                var json_data = await httpMessage.HttpGet(path);

                if (json_data is null || json_data == "[]" || json_data == "[]\r\n" || json_data == string.Empty)
                    break;

                if (page >= 500) // 무한루프를 방지하기 위함. 임의의 값을 초과하면 break
                    break;

                List<FreshdeskContact>? data = JsonConvert.DeserializeObject<List<FreshdeskContact>>(json_data);

                foreach (var r in data)
                    list.Add(r);

                page++;
            }

            Logger.log($"Freshdesk Contacts load completed. ({list.Count} rows)");

            return list.ToFrozenSet();
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
        }

        return list.ToFrozenSet();
    }

    public async Task<FrozenSet<FreshdeskAgent>> GetFreshdeskAgentListAsync()
    {
        List<FreshdeskAgent> list = new();

        int page = 1;

        try
        {
            Logger.log($"Freshdesk Agents load starting..");

            while (true)
            {
                var path = $"agents" +
                    $"?" +
                    $"page={page}" +
                    $"&per_page=100";

                //Logger.log($"Url: {httpMessage._url}/{path}");

                var json_data = await httpMessage.HttpGet(path);
                
                if (json_data is null || json_data == "[]" || json_data == "[]\r\n" || json_data == string.Empty)
                    break;

                if (page >= 500) // 무한루프를 방지하기 위함. 임의의 값을 초과하면 break
                    break;

                List<FreshdeskAgent>? data = JsonConvert.DeserializeObject<List<FreshdeskAgent>>(json_data);

                foreach (var r in data)
                    list.Add(r);

                page++;
            }

            Logger.log($"Freshdesk Agents load completed. ({list.Count} rows)");

            return list.ToFrozenSet();
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
        }

        return list.ToFrozenSet();
    }

    public async Task<FrozenSet<FreshdeskCompany>> GetFreshdeskCompanyListAsync()
    {
        List<FreshdeskCompany> list = new();

        int page = 1;

        try
        {
            Logger.log($"Freshdesk Companies load starting..");

            while (true)
            {
                var path = $"companies" +
                    $"?" +
                    $"page={page}" +
                    $"&per_page=100";

                //Logger.log($"Url: {httpMessage._url}/{path}");

                var json_data = await httpMessage.HttpGet(path);
                
                if (json_data is null || json_data == "[]" || json_data == "[]\r\n" || json_data == string.Empty)
                    break;

                if (page >= 500) // 무한루프를 방지하기 위함. 임의의 값을 초과하면 break
                    break;

                List<FreshdeskCompany>? data = JsonConvert.DeserializeObject<List<FreshdeskCompany>>(json_data);

                foreach (var r in data)
                    list.Add(r);

                page++;
            }

            Logger.log($"Freshdesk Companies load completed. ({list.Count} rows)");

            return list.ToFrozenSet();
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
        }

        return list.ToFrozenSet();
    }
}