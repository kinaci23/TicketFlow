using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajProjesi.API.Data;
using StajProjesi.API.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StajProjesi.API.Controllers
{
    // DİKKAT: Bu kapıya [Authorize] koyduk. Yani elinde JWT Token olmayan buraya giremez!
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;

        // Garsonumuza (Controller) işçimizi (Repository) tanıtıyoruz
        public TicketController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        // Bilet oluşturma kapımız (POST İsteği)
        [HttpPost("create")]
        public async Task<IActionResult> CreateTicket([FromBody] TicketCreateDto ticketDto)
        {
            // Kullanıcının ID'sini dışarıdan almıyoruz, Token'ın içindeki NameIdentifier'dan (Yani yaka kartından) kendimiz söküp alıyoruz!
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { message = "Kimlik doğrulama hatası. Lütfen tekrar giriş yapın." });
            }

            int userId = int.Parse(userIdString);

            // Kutudaki bilgileri (Title, Description) ve Token'dan bulduğumuz ID'yi alıp işçimize teslim ediyoruz:
            int newTicketId = await _ticketRepository.CreateTicketAsync(userId, ticketDto);

            // Her şey yolundaysa 200 OK ile müjdeyi veriyoruz
            return Ok(new
            {
                message = "Harika! Biletin başarıyla oluşturuldu.",
                ticketId = newTicketId
            });
        }
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            // 1. GÜVENLİK: İstek yapan kişinin yaka kartından (Token) ID'sini gizlice okuyoruz
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Kimlik doğrulanamadı. Lütfen geçerli bir token ile tekrar deneyin.");
            }

            int userId = int.Parse(userIdString);

            // 2. İŞLEM: Sözleşme üzerinden işçiye "Bu adamın biletlerini getir" diyoruz
            var tickets = await _ticketRepository.GetTicketsByUserIdAsync(userId);

            // 3. SUNUM: Gelen kargo kutularını (DTO listesini) 200 OK statüsüyle müşteriye sunuyoruz
            return Ok(tickets);
        }
        [HttpGet("all")]
        [Authorize(Roles = "Admin")] // DİKKAT: Sadece Token'ında "Admin" rolü olanlar buraya girebilir!
        public async Task<IActionResult> GetAllTickets()
        {
            var tickets = await _ticketRepository.GetAllTicketsAsync();
            return Ok(tickets);
        }
        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTicket([FromBody] TicketUpdateDto updateDto)
        {
            var result = await _ticketRepository.UpdateTicketAsync(updateDto);

            if (result)
            {
                return Ok(new { message = "Bilet başarıyla güncellendi ve sınıflandırıldı." });
            }

            return BadRequest("Bilet güncellenemedi. TicketId kontrol ediniz.");
        }
    }
}