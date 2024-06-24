namespace Collector_AWS;

public class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Logger.log($"Collector_AWS started.");

            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12
                //| SecurityProtocolType.Tls13
                ;

            DateTime today = DateTime.Now;
            var startDate = today.AddDays(-2); // 오늘로부터 2일전
            var endDate = today.AddDays(+1); // 오늘로부터 1일후

            bool isAllBackfill = false;

            #region args로 받은 값이 정상적으로 DateTime 변환이 가능하다면 "수집기간" override!
            if (args is not null && args.Length >= 1 && args[0] is not null)
            {
                startDate = Convert.ToDateTime(args[0]);
                endDate = Convert.ToDateTime(args[0]);

                if (args.Length >= 2 && args[1] is not null)
                {
                    endDate = Convert.ToDateTime(args[1]);

                    if (args.Length >= 3 && args[2] is not null)
                    {
                        isAllBackfill = (Convert.ToInt32(args[2]) == 1) ? true: false;
                    }
                }
            }
            #endregion

            Logger.log($"string[] args = startDate: {startDate:yyyy-MM-dd}, endDate: {endDate:yyyy-MM-dd}, isAllBackfill: {isAllBackfill}");
            
            int zendeskTicketCount = 0;
            int freshdeskTicketCount = 0;

            if (isAllBackfill) // 전체 소급 모드
            {
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(10) //.AddMonths(1)
                    )
                {
                    DateTime date1 = date;
                    DateTime date2 = date.AddDays(9);
                    //DateTime date1 = new DateTime(date.Year, date.Month, 1);
                    //DateTime date2 = date1.AddMonths(1).AddDays(-1);

                    Logger.log($"startDate: {date1:yyyy-MM-dd} ~ endDate: {date2:yyyy-MM-dd}");

                    using (Collector collector = new())
                    {
                        zendeskTicketCount = await collector.CollectZendeskTickets(date1, date2);
                        freshdeskTicketCount = await collector.CollectFreshdeskTickets(date1, date2);
                    };
                }
            }
            else // 최근 며칠 데이터만 채우는 모드
            {
                using (Collector collector = new())
                {
                    zendeskTicketCount = await collector.CollectZendeskTickets(startDate, endDate);
                    freshdeskTicketCount = await collector.CollectFreshdeskTickets(startDate, endDate);
                };
            }

            // 월 ~ 금 중에 오전 10:00 ~ 10:59 사이인 경우
            if ((
                DateTime.Now.DayOfWeek == DayOfWeek.Monday
                || DateTime.Now.DayOfWeek == DayOfWeek.Tuesday
                || DateTime.Now.DayOfWeek == DayOfWeek.Wednesday
                || DateTime.Now.DayOfWeek == DayOfWeek.Thursday
                || DateTime.Now.DayOfWeek == DayOfWeek.Friday
                )
                && DateTime.Now.Hour >= 10 && DateTime.Now.Hour < 11
                )
            {
                using (ColsonChat colson = new())
                {
                    var _message = $"# 😈 Collector_AWS 수집 내역<br>";
                    //_message += "<br>";
                    _message += "※ 수집 조건: 최근 3일간 수정(UpdatedAt)된 티켓<br>";
                    _message += "<br>";

                    _message += "<table>\\n" +
                        "<thead>\\n" +
                        "<tr>\\n" +
                        "<th>SaaS</th>\\n" +
                        "<th>Ticket count</th>\\n" +
                        "</tr>\\n" +
                        "</thead>\\n" +
                        "<tbody>";

                    _message += $"<tr>\\n" +
                            $"<td>Zendesk</td>\\n" +
                            //$"<td class=\"td3\" style=\"text-align: right;\">{zendeskTicketCount}</td>\\n" +
                            $"<td>{zendeskTicketCount}</td>\\n" +
                            $"</tr>";

                    _message += $"<tr>\\n" +
                            $"<td>Freshdesk</td>\\n" +
                            //$"<td class=\"td3\" style=\"text-align: right;\">{freshdeskTicketCount}</td>\\n" +
                            $"<td>{freshdeskTicketCount}</td>\\n" +
                            $"</tr>";

                    _message += "</tbody>\\n" +
                        "</table>" +
                        "<br>";

                    colson.ColsonChatPost_to_Group(
                        groupId: Secret.cm_teamsGroupId,
                        message: _message
                        );
                };
            }

            Logger.log($"Collector_AWS completed.");
        }
        catch (Exception ex)
        {
            Logger.log(ex.Message);
            //throw ex;
        }
    }
}