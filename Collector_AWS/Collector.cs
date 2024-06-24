using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Tmon.Collector.Net;

namespace Tmon.Collector;

public class Collector : IDisposable
{
    private ZendeskClient _zendeskClient;
    private FreshdeskClient _freshdeskClient;
    private DapperContext _context;
    private bool _disposed = false;

    public Collector()
    {
        _zendeskClient = new();
        _freshdeskClient = new();
        _context = new();
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
            _zendeskClient = null;
            _freshdeskClient = null;
            _context = null;
        }

        // 관리되지 않는 리소스 정리
        // ...

        _disposed = true;
    }

    public async Task<int> CollectZendeskTickets(DateTime startDate, DateTime endDate)
    {
        List<ZendeskTicketApiResponse> listResponse = new List<ZendeskTicketApiResponse>();

        string? nextPageUrl = null;
        int savedTicketCount = 0;
        int totalTicketCount = 0;

        try
        {
            while (true)
            {
                var ticket_json_data = await _zendeskClient.GetZendeskTicketsAsync(startDate, endDate, nextPageUrl);

                ZendeskTicketApiResponse? responseTicket = JsonConvert.DeserializeObject<ZendeskTicketApiResponse>(ticket_json_data);
                
                listResponse.Add(responseTicket);

                nextPageUrl = null;
                nextPageUrl = responseTicket.next_page;

                // nextPageUrl이 null 이면 마지막 page라는 것을 의미하기 때문에 break!
                if (nextPageUrl is null)
                    break;
            }

            //Logger.log($"List<ZendeskTicketApiResponse> listResponse count = {listResponse.Count}");

            totalTicketCount = listResponse.Sum(r => r.Results.Count);

            //Logger.log($"totalTicketCount = {totalTicketCount}.");

            foreach (var responseTicket2 in listResponse)
            {
                foreach (var ticket in responseTicket2.Results)
                {
                    try
                    {
                        //string? orgJson = await _zendeskClient.GetZendeskOrganizationNameByIdAsync(ticket.OrganizationId);
                        //string? requesterJson = await _zendeskClient.GetZendeskUserNameByIdAsync(ticket.RequesterId);
                        //string? AssigneeJson = await _zendeskClient.GetZendeskUserNameByIdAsync(ticket.AssigneeId);
                        string? CommentsJson = await _zendeskClient.GetZendeskTicketCommentsAsync(ticket.Id);

                        //ZendeskOrganizationApiResponse responseOrganization = JsonConvert.DeserializeObject<ZendeskOrganizationApiResponse>(orgJson);
                        //ZendeskUserApiResponse responseRequester = JsonConvert.DeserializeObject<ZendeskUserApiResponse>(requesterJson);
                        //ZendeskUserApiResponse responseAssignee = JsonConvert.DeserializeObject<ZendeskUserApiResponse>(AssigneeJson);
                        ZendeskTicketCommentApiResponse responseComments = JsonConvert.DeserializeObject<ZendeskTicketCommentApiResponse>(CommentsJson);

                        // Comments가 여러개인 경우에도 항상 Comments[0] 은 티켓 요청 본문이라고 함.
                        ticket.Description_Html = responseComments.Comments.FirstOrDefault().html_body;

                        ticket.CreatedAt = ticket.CreatedAt.AddHours(9); // UTC -> KST로 변환
                        ticket.UpdatedAt = ticket.UpdatedAt.AddHours(9); // UTC -> KST로 변환

                        if (ticket.OrganizationId is not null)
                            ticket.Organization = _zendeskClient.organizationList?.FirstOrDefault(o => o.Id == ticket.OrganizationId);

                        if (ticket.RequesterId is not null)
                            ticket.Requester = _zendeskClient.userList?.FirstOrDefault(u => u.Id == ticket.RequesterId);

                        if (ticket.AssigneeId is not null)
                            ticket.Assignee = _zendeskClient.userList?.FirstOrDefault(u => u.Id == ticket.AssigneeId);

                        //ticket.Requester = responseRequester?.user;
                        //ticket.Assignee = responseAssignee?.user;
                        ticket.ticketJson = JsonConvert.SerializeObject(ticket);
                        //ticket.ticketJson = ticket_json_data; // ZendeskOrganizationApiResponse

                        this.SaveTicketToDb(ticket);

                        savedTicketCount++;

                        Logger.log($"{savedTicketCount} of {totalTicketCount} ticket(s) saved. -> " +
                            $"id = {ticket.Id}" +
                            $", UpdatedAt = {ticket.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")}" +
                            $", subject = {((ticket.Subject.Length >= 20) ? ticket.Subject.Substring(0, 20) + ".." : ticket.Subject)}" +
                            //$", 조직 Name = {responseOrganization?.organization.Name}" +
                            //$", 요청자 Name = {responseRequester?.user.Name}" +
                            //$", 담당자 Name = {responseAssignee?.user.Name}" +
                            $"");
                    }
                    catch (Exception ex)
                    {
                        Logger.log(ex.Message);
                    }
                }
            }

            Logger.log($"Tmon.Collector has collected {savedTicketCount} Zendesk ticket(s).");

            return totalTicketCount;
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
        }
        finally
        {
        }

        return totalTicketCount;
    }

    public async Task<int> CollectFreshdeskTickets(DateTime startDate, DateTime endDate)
    {
        List<FreshdeskTicketApiResponse> listResponse = new List<FreshdeskTicketApiResponse>();

        int page = 1;
        int savedTicketCount = 0;
        int totalTicketCount = 0;

        try
        {
            while (true)
            {
                var ticket_json_data = await _freshdeskClient.GetFreshdeskTicketsAsync(startDate, endDate, page);

                try
                {
                    FreshdeskTicketApiResponse? responseTicket = JsonConvert.DeserializeObject<FreshdeskTicketApiResponse>(ticket_json_data);

                    listResponse.Add(responseTicket);

                    Logger.log($"responseTicket.Results.Count = {responseTicket.Results.Count}.");

                    if (responseTicket.Results.Count == 0)
                        break;

                    page++;
                }
                catch (Exception ex)
                {
                    break;
                }
            }

            totalTicketCount = listResponse.Sum(r => r.Results.Count);
            
            foreach (var responseTicket2 in listResponse)
            {
                responseTicket2.Results
                    // UpdatedAt 기준 오름차순 정렬
                    //.OrderBy(r => r.UpdatedAt)
                    .OrderByDescending(r => r.UpdatedAt) // UpdatedAt 기준 내림차순 정렬 -> api/v2/search/tickets?query="" 에서 정렬 기능 제공하지 않음.
                    .ToList()
                    .ForEach(ticket =>
                    {
                        try
                        {
                            if (ticket.RequesterId is not null)
                                ticket.Requester = _freshdeskClient.contactList?.FirstOrDefault(c => c.Id == ticket.RequesterId);

                            if (ticket.ResponderId is not null)
                                ticket.Responder = _freshdeskClient.agentList?.FirstOrDefault(c => c.Id == ticket.ResponderId).Contact;

                            if (ticket.CompanyId is not null)
                                ticket.Company = _freshdeskClient.companyList?.FirstOrDefault(c => c.Id == ticket.CompanyId);

                            ticket.CreatedAt = ticket.CreatedAt.AddHours(9); // UTC -> KST로 변환
                            ticket.UpdatedAt = ticket.UpdatedAt.AddHours(9); // UTC -> KST로 변환
                            ticket.ticketJson = JsonConvert.SerializeObject(ticket);

                            this.SaveTicketToDb(ticket);

                            savedTicketCount++;

                            Logger.log($"{savedTicketCount} of {totalTicketCount} ticket(s) saved. -> " +
                                    $"id = {ticket.Id}" +
                                    $", UpdatedAt = {ticket.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")}" +
                                    $", subject = {((ticket.Subject.Length >= 20) ? ticket.Subject.Substring(0, 20) + ".." : ticket.Subject)}" +
                                    $"");
                        }
                        catch (Exception ex)
                        {
                            Logger.log(ex.Message);
                        }
                    });
            }

            Logger.log($"Tmon.Collector has collected {savedTicketCount} Freshdesk ticket(s).");

            return totalTicketCount;
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
        }
        finally
        {
        }

        return totalTicketCount;
    }

    public async Task SaveTicketToDb(ZendeskTicket t)
    {
        string conn_str = Secret.conn_str_CM_DEV_DB;

        try
        {
            var BrandCode = "BRD-WSN";
            BrandCode = _zendeskClient.brands.FirstOrDefault(b => b.Id == t.BrandId).Code;
            
            var param = new DynamicParameters();
            param.Add("BrandCode", BrandCode); // "BRD-WSN": "WiseN", "BRD-MT3": "mate365"
            param.Add("RefSaaSCode", "TSA-ZEN"); // Zendesk
            param.Add("RefTicketId", t.Id);
            param.Add("CreatedAt", t.CreatedAt);
            param.Add("UpdatedAt", t.UpdatedAt);
            param.Add("OrganizationId", t.OrganizationId);
            param.Add("Organization", t.Organization?.Name);
            param.Add("RequesterId", t.RequesterId);
            param.Add("Requester", this.MakeMailbox(t.Requester?.Name, t.Requester?.Email));
            // Zendesk API가 응답해주는 Url 값 -> https://wisen.zendesk.com/api/v2/tickets/24301.json
            //param.Add("Url", t.Url.ToString());
            // Zendesk 웹에서 실제 티켓을 열 수 있는 Url -> https://wisen.zendesk.com/agent/tickets/24424
            param.Add("Url", $"https://wisen.zendesk.com/agent/tickets/{t.Id}");
            param.Add("Type", t.Type);
            param.Add("Priority", t.Priority);
            param.Add("AssigneeId", t.AssigneeId);
            param.Add("Assignee", this.MakeMailbox(t.Assignee?.Name, t.Assignee?.Email));
            param.Add("Status", t.Status);
            param.Add("Subject", t.Subject);
            param.Add("Description", t.Description);
            param.Add("Description_Html", t.Description_Html);
            param.Add("Tag", string.Join(", ", t.Tags));
            param.Add("RefUpdatedAt", DateTime.Now);
            param.Add("RefJsonData", t.ticketJson);

            int effected_row = 0;

            bool isExistTicket = false;

            using (IDbConnection conn = _context.CreateConnection(conn_str))
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var reader = conn.ExecuteReader(
                    $"select\tBrandCode, RefSaaSCode, RefTicketId, TicketSNo\r\n" +
                    $"from\tdbo.Ticket\r\n" +
                    $"where\tBrandCode = '{BrandCode}'\r\n" + // "BRD-WSN": "WiseN", "BRD-MT3": "mate365"
                    $"and\tRefSaaSCode = 'TSA-ZEN'\r\n" + // Zendesk
                    $"and\tRefTicketId = '{t.Id}';"))
                {
                    while (reader.Read())
                    {
                        isExistTicket = true;
                        break;
                    }
                }

                //Logger.log($"isExistTicket = {isExistTicket}");

                if (!isExistTicket) // 없으면 insert
                {
                    var rawSql = $"insert into dbo.Ticket (\r\n" +
                    $"BrandCode, RefSaaSCode, RefTicketId, OrganizationId, Organization, RequesterId, Requester, CreatedAt, UpdatedAt, Url, Type\r\n" +
                    $"\t, Priority, AssigneeId, Assignee, Status, Subject, Description, Description_Html, Tag, RefUpdatedAt, RefJsonData)\r\n" +
                    $"values(\r\n" +
                    $"@BrandCode, @RefSaaSCode, @RefTicketId, @OrganizationId, @Organization, @RequesterId, @Requester, @CreatedAt, @UpdatedAt, @Url, @Type\r\n" +
                    $"\t, @Priority, @AssigneeId, @Assignee, @Status, @Subject, @Description, @Description_Html, @Tag, @RefUpdatedAt, @RefJsonData);";

                    effected_row = conn.Execute(rawSql, param);
                }
                else // 있으면 update
                {
                    var rawSql = $"update dbo.Ticket\r\n" +
                        $"set\tOrganization = @Organization\r\n" +
                        $"\t, OrganizationId = @OrganizationId\r\n" +
                        $"\t, RequesterId = @RequesterId\r\n" +
                        $"\t, Requester = @Requester\r\n" +
                        $"\t, CreatedAt = @CreatedAt\r\n" +
                        $"\t, UpdatedAt = @UpdatedAt\r\n" +
                        $"\t, Url = @Url\r\n" +
                        $"\t, Type = @Type\r\n" +
                        $"\t, Priority = @Priority\r\n" +
                        $"\t, AssigneeId = @AssigneeId\r\n" +
                        $"\t, Assignee = @Assignee\r\n" +
                        $"\t, Status = @Status\r\n" +
                        $"\t, Subject = @Subject\r\n" +
                        $"\t, Description = @Description\r\n" +
                        $"\t, Description_Html = @Description_Html\r\n" +
                        $"\t, Tag = @Tag\r\n" +
                        $"\t, RefUpdatedAt = getdate()\r\n" +
                        $"\t, RefJsonData = @RefJsonData\r\n" +
                        $"where\tBrandCode = @BrandCode\r\n" +
                        $"and\tRefSaaSCode = @RefSaaSCode\r\n" +
                        $"and\tRefTicketId = @RefTicketId;";

                    effected_row = conn.Execute(rawSql, param);
                }

                if (conn.State == ConnectionState.Open) conn.Close();
            }

            //var data = new { effected_row = effected_row, message = $"InsertTicketToDb completed. ({(isExistTicket ? "updated" : "inserted")})" };
            //return data.ToString();
            //Logger.log($"InsertTicketToDb completed. ({(isExistTicket ? "updated" : "inserted")})");
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
            //throw ex;
        }
    }

    public async Task SaveTicketToDb(FreshdeskTicket t)
    {
        string conn_str = Secret.conn_str_CM_DEV_DB;

        try
        {
            var param = new DynamicParameters();
            param.Add("BrandCode", "BRD-MT3"); // Mate365
            param.Add("RefSaaSCode", "TSA-FRE"); // Freshdesk
            param.Add("RefTicketId", t.Id);
            param.Add("CreatedAt", t.CreatedAt);
            param.Add("UpdatedAt", t.UpdatedAt);
            param.Add("OrganizationId", t.CompanyId);
            param.Add("Organization", t.Company?.Name);
            param.Add("RequesterId", t.RequesterId);
            param.Add("Requester", this.MakeMailbox(t.Requester?.Name, t.Requester?.Email));
            param.Add("Url", $"https://support.mate365.co.kr/a/tickets/{t.Id}");
            param.Add("Type", t.Type);
            param.Add("Priority", t.PriorityMatch(t.Priority));
            param.Add("AssigneeId", t.ResponderId);
            param.Add("Assignee", this.MakeMailbox(t.Responder?.Name, t.Responder?.Email));
            param.Add("Status", t.StatusMatch(t.Status));
            param.Add("Subject", t.Subject);
            param.Add("Description", t.Description);
            param.Add("Description_Html", t.Description_Html);
            param.Add("Tag", string.Join(", ", t.Tags));
            param.Add("RefUpdatedAt", DateTime.Now);
            param.Add("RefJsonData", t.ticketJson);

            int effected_row = 0;

            bool isExistTicket = false;

            using (IDbConnection conn = _context.CreateConnection(conn_str))
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var reader = conn.ExecuteReader(
                    $"select\tBrandCode, RefSaaSCode, RefTicketId, TicketSNo\r\n" +
                    $"from\tdbo.Ticket\r\n" +
                    $"where\tBrandCode = 'BRD-MT3'\r\n" + // Mate365
                    $"and\tRefSaaSCode = 'TSA-FRE'\r\n" + // Freshdesk
                    $"and\tRefTicketId = '{t.Id}';"))
                {
                    while (reader.Read())
                    {
                        isExistTicket = true;
                        break;
                    }
                }

                //Logger.log($"isExistTicket = {isExistTicket}");

                if (!isExistTicket) // 없으면 insert
                {
                    var rawSql = $"insert into dbo.Ticket (\r\n" +
                    $"BrandCode, RefSaaSCode, RefTicketId, OrganizationId, Organization, RequesterId, Requester, CreatedAt, UpdatedAt, Url, Type\r\n" +
                    $"\t, Priority, AssigneeId, Assignee, Status, Subject, Description, Description_Html, Tag, RefUpdatedAt, RefJsonData)\r\n" +
                    $"values(\r\n" +
                    $"@BrandCode, @RefSaaSCode, @RefTicketId, @OrganizationId, @Organization, @RequesterId, @Requester, @CreatedAt, @UpdatedAt, @Url, @Type\r\n" +
                    $"\t, @Priority, @AssigneeId, @Assignee, @Status, @Subject, @Description, @Description_Html, @Tag, @RefUpdatedAt, @RefJsonData);";

                    effected_row = conn.Execute(rawSql, param);
                }
                else // 있으면 update
                {
                    var rawSql = $"update dbo.Ticket\r\n" +
                        $"set\tOrganization = @Organization\r\n" +
                        $"\t, OrganizationId = @OrganizationId\r\n" +
                        $"\t, RequesterId = @RequesterId\r\n" +
                        $"\t, Requester = @Requester\r\n" +
                        $"\t, CreatedAt = @CreatedAt\r\n" +
                        $"\t, UpdatedAt = @UpdatedAt\r\n" +
                        $"\t, Url = @Url\r\n" +
                        $"\t, Type = @Type\r\n" +
                        $"\t, Priority = @Priority\r\n" +
                        $"\t, AssigneeId = @AssigneeId\r\n" +
                        $"\t, Assignee = @Assignee\r\n" +
                        $"\t, Status = @Status\r\n" +
                        $"\t, Subject = @Subject\r\n" +
                        $"\t, Description = @Description\r\n" +
                        $"\t, Description_Html = @Description_Html\r\n" +
                        $"\t, Tag = @Tag\r\n" +
                        $"\t, RefUpdatedAt = getdate()\r\n" +
                        $"\t, RefJsonData = @RefJsonData\r\n" +
                        $"where\tBrandCode = @BrandCode\r\n" +
                        $"and\tRefSaaSCode = @RefSaaSCode\r\n" +
                        $"and\tRefTicketId = @RefTicketId;";

                    effected_row = conn.Execute(rawSql, param);
                }

                if (conn.State == ConnectionState.Open) conn.Close();
            }

            //var data = new { effected_row = effected_row, message = $"InsertTicketToDb completed. ({(isExistTicket ? "updated" : "inserted")})" };
            //return data.ToString();
            //Logger.log($"InsertTicketToDb completed. ({(isExistTicket ? "updated" : "inserted")})");
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
            //throw ex;
        }
    }

    /// <summary>
    /// It's called a mailbox, as specified in RFC 5322 on page 45: https://datatracker.ietf.org/doc/html/rfc5322
    /// </summary>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <returns>EX) "Gildong Ko<Gildong.Ko@cloudmt.co.kr>"</returns>
    public string MakeMailbox(string? name, string? email)
    {
        if (email is null || email == string.Empty)
            return $"{name?.Trim()}";
        else
            return $"{name?.Trim()} <{email}>";
    }

    ~Collector()
    {
        _zendeskClient = null;
        _freshdeskClient = null;
        _context = null;
    }
}