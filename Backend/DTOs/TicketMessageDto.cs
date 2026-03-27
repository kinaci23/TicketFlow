public class TicketMessageDto
{
    public int MessageId { get; set; }
    public int TicketId { get; set; }
    public int UserId { get; set; }
    public string MessageText { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SenderName { get; set; }
    public int SenderRoleId { get; set; }
}