namespace Tmon.Collector;

/// <summary>
/// plain ZendeskTicket
/// </summary>
public class ZendeskTicket
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("url")]
    public Uri Url { get; set; }

    [JsonProperty("external_id")]
    public string ExternalId { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("subject")]
    public string Subject { get; set; }

    [JsonProperty("raw_subject")]
    public string RawSubject { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
    
    //[JsonProperty("description_html")]
    public string Description_Html { get; set; }

    [JsonProperty("priority")]
    public string? Priority { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("recipient")]
    public string Recipient { get; set; }

    [JsonProperty("requester_id")]
    public long? RequesterId { get; set; }
    
    public ZendeskUser? Requester { get; set; }

    [JsonProperty("submitter_id")]
    public long? SubmitterId { get; set; }

    [JsonProperty("assignee_id")]
    public long? AssigneeId { get; set; }

    public ZendeskUser? Assignee { get; set; }

    [JsonProperty("organization_id")]
    public long? OrganizationId { get; set; }

    public ZendeskOrganization? Organization { get; set; }

    [JsonProperty("group_id")]
    public long? GroupId { get; set; }

    [JsonProperty("collaborator_ids")]
    public IReadOnlyList<long> CollaboratorIds { get; set; }

    [JsonProperty("follower_ids")]
    public IReadOnlyList<long> FollowerIds { get; set; }

    [JsonProperty("forum_topic_id")]
    public long? ForumTopicId { get; set; }

    [JsonProperty("problem_id")]
    public long? ProblemId { get; set; }

    [JsonProperty("has_incidents")]
    public bool HasIncidents { get; set; }

    [JsonProperty("due_at")]
    public DateTime? Due { get; set; }

    [JsonProperty("tags")]
    public IReadOnlyList<string> Tags { get; set; }

    [JsonProperty("via")]
    public object Via { get; set; }

    //[JsonProperty("custom_fields")]
    //[JsonConverter(typeof(CustomFieldsConverter))]
    //public IReadOnlyCustomFields CustomFields { get; set; }

    [JsonProperty("satisfaction_rating")]
    public object SatisfactionRating { get; set; }

    [JsonProperty("sharing_agreement_ids")]
    public IReadOnlyList<long> SharingAgreementIds { get; set; }

    [JsonProperty("followup_ids")]
    public IReadOnlyList<long> FollowupIds { get; set; }

    [JsonProperty("ticket_form_id")]
    public long? FormId { get; set; }

    [JsonProperty("brand_id")]
    public long? BrandId { get; set; }

    [JsonProperty("allow_channelback")]
    public bool AllowChannelback { get; set; }

    [JsonProperty("is_public")]
    public bool IsPublic { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("result_type")]
    private string ResultType { get; set; }

    public string ticketJson { get; set; }
}

/// <summary>
/// List<ZendeskTicket> 이 포함된 API 응답
/// </summary>
public class ZendeskTicketApiResponse
{
    [JsonPropertyName("results")]
    public List<ZendeskTicket> Results { get; set; }
    //[JsonPropertyName("facets")]
    public string? facets { get; set; }
    //[JsonPropertyName("next_page")]
    public string? next_page { get; set; }
    //[JsonPropertyName("previous_page")]
    public string? previous_page { get; set; }
    //[JsonPropertyName("count")]
    public int count { get; set; }
}

/// <summary>
/// plain ZendeskOrganization (불필요한 필드들은 제거되어 있음)
/// </summary>
public class ZendeskOrganization
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("url")]
    public Uri Url { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("shared_tickets")]
    public bool? shared_tickets { get; set; }

    public bool? shared_comments { get; set; }

    public object? external_id { get; set; }
}

/// <summary>
/// ZendeskOrganization 이 포함된 API 응답
/// </summary>
public class ZendeskOrganizationsApiResponse
{
    [JsonPropertyName("organizations")]
    public List<ZendeskOrganization> Organizations { get; set; }

    public string? next_page { get; set; }

    public string? previous_page { get; set; }

    public int? count { get; set; }
}

/// <summary>
/// plain ZendeskUser
/// </summary>
public class ZendeskUser
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("url")]
    public Uri Url { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }
}

/// <summary>
/// ZendeskUser 가 포함된 API 응답
/// </summary>
public class ZendeskUsersApiResponse
{
    [JsonPropertyName("users")]
    public List<ZendeskUser> Users { get; set; }

    public string? next_page { get; set; }

    public string? previous_page { get; set; }

    public int? count { get; set; }
}

/// <summary>
/// Zendesk 특정 ticket에 대한 Comment
/// </summary>
public class ZendeskTicketComment
{
    //[JsonPropertyName("id")]
    public long? id { get; set; }

    //[JsonPropertyName("type")]
    public string? type { get; set; }

    //[JsonPropertyName("author_id")]
    public long? author_id { get; set; }

    //[JsonPropertyName("body")]
    public string? body { get; set; }

    //[JsonPropertyName("html_body")]
    public string? html_body { get; set; }

    //[JsonPropertyName("plain_body")]
    public string? plain_body { get; set; }

    //[JsonPropertyName("public")]
    //public bool? Public { get; set; }

    //[JsonPropertyName("created_at")]
    public DateTime? created_at { get; set; }
}

/// <summary>
/// ZendeskTicketComment 가 포함된 API 응답
/// </summary>
public class ZendeskTicketCommentApiResponse
{
    [JsonPropertyName("comments")]
    public List<ZendeskTicketComment> Comments { get; set; }

    //[JsonPropertyName("next_page")]
    public string next_page { get; set; }

    //[JsonPropertyName("previous_page")]
    public string previous_page { get; set; }

    //[JsonPropertyName("count")]
    public int count { get; set; }
}

/// <summary>
/// plain FreshdeskTicket - https://developers.freshdesk.com/api/#ticket-fields
/// </summary>
public class FreshdeskTicket
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("subject")]
    public string Subject { get; set; }

    [JsonProperty("description_text")]
    public string Description { get; set; }

    [JsonProperty("description")]
    public string Description_Html { get; set; }

    /// <summary>
    /// 1: "Low", 2: "Medium", 3: "High", 4: "Urgent"
    /// </summary>
    [JsonProperty("priority")]
    public string? Priority { get; set; }

    public string PriorityMatch(string? Priority)
    {
#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
        return Priority switch
        {
            "1" => "Low",
            "2" => "Medium",
            "3" => "High",
            "4" => "Urgent",
            _ => Priority, // else 인 경우 Priority 값을 그대로 리턴
        };
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
    }

    /// <summary>
    /// 2: "Open", 3: "Pending", 4: "Resolved", 5: "Closed"
    /// </summary>
    [JsonProperty("status")]
    public string? Status { get; set; }
    public string StatusMatch(string? Status)
    {
#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
        return Status switch
        {
            "2" => "Open", // 열려있음
            "3" => "Pending", // 보류
            "4" => "Resolved", // 해결됨
            "5" => "Closed", // 종료됨
            "6" => "Pending", // 보류
            _ => Status, // else 인 경우 Status 값을 그대로 리턴
        };
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
    }

    [JsonProperty("requester_id")]
    public long? RequesterId { get; set; }

    public FreshdeskContact? Requester { get; set; }

    [JsonProperty("responder_id")]
    public long? ResponderId { get; set; }
    public FreshdeskContact? Responder { get; set; }

    [JsonProperty("company_id")]
    public long? CompanyId { get; set; }
    public FreshdeskCompany? Company { get; set; }
    
    [JsonProperty("group_id")]
    public long? GroupId { get; set; }
    //public ZendeskOrganization? Group { get; set; }

    [JsonProperty("cc_emails")]
    public IReadOnlyList<string> ccEmails { get; set; }

    [JsonProperty("due_by")]
    public DateTime? Due { get; set; }

    [JsonProperty("tags")]
    public IReadOnlyList<string> Tags { get; set; }

    [JsonProperty("form_id")]
    public long? FormId { get; set; }
    
    [JsonProperty("product_id")]
    public long? ProductId { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public string ticketJson { get; set; }
}

/// <summary>
/// List<FreshdeskTicket> 이 포함된 API 응답
/// </summary>
public class FreshdeskTicketApiResponse
{
    [JsonPropertyName("results")]
    public List<FreshdeskTicket>? Results { get; set; }
    
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

/// <summary>
/// plain FreshdeskContact
/// </summary>
public class FreshdeskContact
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

/// <summary>
/// plain FreshdeskAgent
/// </summary>
public class FreshdeskAgent
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("contact")]
    public FreshdeskContact? Contact { get; set; }
}

/// <summary>
/// plain FreshdeskCompany
/// </summary>
public class FreshdeskCompany
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}