namespace Collector_AWS.Net;

public class ZendeskClient
{
    private HttpMessage httpMessage;
    private string subDomain = "wisen";
    private string urlPrefix = string.Empty;
    private string? contentType = "application/json";
    private string? accept = "application/json";
    private string authorization = null;

    public FrozenSet<ZendeskOrganization> organizationList = null;
    public FrozenSet<ZendeskUser> userList = null;

    public readonly ZendeskBrand[] brands = new ZendeskBrand[2];

    public record ZendeskBrand
    {
        public long? Id { get; set; } = 0;
        public string? Name { get; set; } = string.Empty;
        public string? Code { get; set; } = string.Empty;
    }

    public ZendeskClient()
    {
        this.urlPrefix = $"https://{this.subDomain}.zendesk.com/api/v2";
        this.authorization = "Basic " + Helper.Utf8.str_to_base64string($"{Secret.zendeskUserId2}/token:{Secret.zendeskApiToken2}");

        httpMessage = new HttpMessage(this.urlPrefix, contentType, accept, this.authorization);

        // Brand data
        brands[0] = new ZendeskBrand { Id = 712217, Name = "WiseN Support", Code = "BRD-WSN" };
        brands[1] = new ZendeskBrand { Id = 360003389454, Name = "mate365", Code = "BRD-MT3" };
    }

    public async Task<string> GetZendeskTicketsAsync(DateTime startUpdatedAt, DateTime endUpdatedAt, string? nextPageUrl = null)
    {
        var start = startUpdatedAt.ToString("yyyy-MM-dd");
        var end = endUpdatedAt.ToString("yyyy-MM-dd");
        
        try
        {
            if (organizationList == null)
                organizationList = await GetZendeskOrganizationsAsync();

            if (userList == null)
                userList = await GetZendeskUsersAsync();

            var path = $"search.json"
                + $"?"
                + $"query=type:ticket + updated>{start}T00:00:00Z + updated<={end}T23:59:59Z"
                //+ $"query=type:ticket + created>2024-01-01T00:00:00Z + created<=2024-01-05T23:59:59Z"
                + $"&sort_by=updated_at&sort_order=desc" // "desc"
                //+ $"&per_page=200"
                ;
            path = path.Replace(" ", "");

            //var path = $"search.json?query=type:ticket status:new status:open";
            //path = $"search.json?query=type:ticket status:new";
            //path = $"search.json?query=type:ticket created>2024-05-01T00:00:00Z";
            //var ss = "&sort_by=custom_fields\"\": [{\"\"id\"\": 360006555153,value\"\":\"\"\"\"}]=asc";

            if (nextPageUrl != null)
                path = nextPageUrl.Replace(this.urlPrefix + "/", "");

            Logger.log($"Zendesk Ticket Url = {httpMessage._url}/{path}");
            
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

    public async Task<FrozenSet<ZendeskOrganization>> GetZendeskOrganizationsAsync()
    {
        List<ZendeskOrganization> list = new();

        int page = 1;
        
        try
        {
            Logger.log($"Zendesk Organizations load starting..");

            while(true)
            {
                var path = $"organizations.json" +
                    $"?" +
                    $"page={page}" +
                    $"";

                var json_data = await httpMessage.HttpGet(path);

                if (json_data is null || json_data == "[]" || json_data == "[]\r\n" || json_data == string.Empty)
                    break;

                if (page >= 500) // 무한루프를 방지하기 위함. 임의의 값을 초과하면 break
                    break;

                ZendeskOrganizationsApiResponse data = JsonConvert.DeserializeObject<ZendeskOrganizationsApiResponse>(json_data);

                Logger.log($"path = {path}, data.Organizations.Count = {data.Organizations.Count}");

                if (data.Organizations.Count == 0)
                    break;

                foreach (var org in data.Organizations)
                    list.Add(org);

                page++;
            }

            Logger.log($"Zendesk Organizations load completed. ({list.Count} rows)");

            return list.ToFrozenSet();
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
        }
        finally
        {
        }

        return list.ToFrozenSet();
    }
    
    public async Task<FrozenSet<ZendeskUser>> GetZendeskUsersAsync()
    {
        List<ZendeskUser> list = new();

        int page = 1;

        try
        {
            Logger.log($"Zendesk Users load starting..");

            while (true)
            {
                var path = $"users.json" +
                    $"?" +
                    $"page={page}" +
                    $"";

                var json_data = await httpMessage.HttpGet(path);

                if (json_data is null || json_data == "[]" || json_data == "[]\r\n" || json_data == string.Empty)
                    break;

                if (page >= 500) // 무한루프를 방지하기 위함. 임의의 값을 초과하면 break
                    break;

                ZendeskUsersApiResponse data = JsonConvert.DeserializeObject<ZendeskUsersApiResponse>(json_data);

                Logger.log($"path = {path}, data.Users.Count = {data.Users.Count}");

                if (data.Users.Count == 0)
                    break;

                foreach (var user in data.Users)
                    list.Add(user);

                page++;
            }

            Logger.log($"Zendesk Users load completed. ({list.Count} rows)");

            return list.ToFrozenSet();
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
        }
        finally
        {
        }

        return list.ToFrozenSet();
    }

    public async Task<string> GetZendeskTicketCommentsAsync(long? ticketId)
    {
        if (ticketId is null || ticketId == 0)
            return "";

        try
        {
            var path = $"tickets/{ticketId}/comments.json";

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

    #region 미사용 로직
    public async Task<string> GetZendeskOrganizationNameByIdAsync(long? organization_id)
    {
        if (organization_id is null || organization_id == 0)
            return "";

        try
        {
            var path = $"organizations/{organization_id}.json";
            
            //Logger.log($"Url: {httpMessage._url}/{path}");

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

    public async Task<string> GetZendeskUserNameByIdAsync(long? user_id)
    {
        if (user_id is null || user_id == 0)
            return "";

        try
        {
            var path = $"users/{user_id}.json";
            
            //Logger.log($"Url: {httpMessage._url}/{path}");

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
    #endregion
}