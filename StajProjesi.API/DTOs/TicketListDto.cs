using System;

namespace StajProjesi.API.DTOs
{
    public class TicketListDto
    {
        public int TicketId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Urgency { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}