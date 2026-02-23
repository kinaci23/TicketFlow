using System.Threading.Tasks;
using System.Collections.Generic;
using StajProjesi.API.DTOs;

namespace StajProjesi.API.Data
{
    public interface ITicketRepository
    {
        Task<int> CreateTicketAsync(int userId, TicketCreateDto ticketDto, int predictedCategoryId);
        Task<IEnumerable<TicketListDto>> GetTicketsByUserIdAsync(int userId);
        Task<IEnumerable<TicketListDto>> GetAllTicketsAsync();
        Task<bool> UpdateTicketAsync(TicketUpdateDto updateDto);
        Task<TicketListDto> GetTicketByIdAsync(int ticketId);
        Task<List<TicketMessageDto>> GetTicketMessagesAsync(int ticketId);
        Task<TicketMessageDto> AddTicketMessageAsync(int ticketId, int userId, string messageText);
    }
}