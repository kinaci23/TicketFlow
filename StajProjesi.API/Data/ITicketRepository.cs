using System.Threading.Tasks;
using System.Collections.Generic;
using StajProjesi.API.DTOs;

namespace StajProjesi.API.Data
{
    
    public interface ITicketRepository
    {
        Task<int> CreateTicketAsync(int userId, TicketCreateDto ticketDto);
        Task<IEnumerable<TicketListDto>> GetTicketsByUserIdAsync(int userId);
        Task<IEnumerable<TicketListDto>> GetAllTicketsAsync();
        Task<bool> UpdateTicketAsync(TicketUpdateDto updateDto);
    }
}