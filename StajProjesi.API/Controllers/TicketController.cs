using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajProjesi.API.Data;
using StajProjesi.API.DTOs;
using StajProjesi_API;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StajProjesi.API.Controllers
{
    // DİKKAT: Bu kapıya [Authorize] koyduk. Yani elinde JWT Token olmayan buraya giremez!
    [Authorize]
    [Route("api/Tickets")]
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
            // 1. Kimlik Doğrulama (Aynı kalıyor)
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { message = "Kimlik doğrulama hatası. Lütfen tekrar giriş yapın." });
            }

            int userId = int.Parse(userIdString);

            // ============================================================
            // 2. YAPAY ZEKA DEVREYE GİRİYOR (YENİ KISIM) 🧠
            // ============================================================

            // Model için girdiyi hazırla (Sadece Başlık ve Açıklama lazım)
            var input = new TicketClassifier.ModelInput
            {
                Title = ticketDto.Title,
                Description = ticketDto.Description
            };

            // Yapay Zekaya tahmin ettir
            var result = TicketClassifier.Predict(input);

            // Tahmin sonucunu al (1, 2, 3 veya 4 dönecek)
            // Not: Model float dönebilir, int'e çeviriyoruz.
            int predictedCategoryId = (int)result.PredictedLabel;

            // ============================================================
            // 3. REPOSITORY ÇAĞRISI (GÜNCELLENDİ)
            // ============================================================

            // Artık işçiye (Repository) sadece DTO'yu değil, yapay zekanın tahminini de veriyoruz.
            // DİKKAT: Bu satır kızarabilir, aşağıda düzelteceğiz.
            int newTicketId = await _ticketRepository.CreateTicketAsync(userId, ticketDto, predictedCategoryId);

            return Ok(new
            {
                message = "Bilet oluşturuldu ve Yapay Zeka kategori tahminini yaptı!",
                ticketId = newTicketId,
                predictedCategory = predictedCategoryId // Kullanıcıya da ne tahmin ettiğimizi gösterelim
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
        [HttpGet("all-tickets")]
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
        [HttpGet("{id}")]
        [Authorize] // Hem admin hem user kendi biletinin detayını görebilmeli
        public async Task<IActionResult> GetTicketById(int id)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);

            if (ticket == null)
            {
                return NotFound("Bilet bulunamadı.");
            }

            return Ok(ticket);
        }
        // ==========================================
        // 🚀 1. BİLETİN MESAJLARINI GETİRME (GET)
        // ==========================================
        [HttpGet("{ticketId}/messages")]
        [Authorize] // Hem Admin hem Kullanıcı görebilir
        public async Task<IActionResult> GetTicketMessages(int ticketId)
        {
            // Repository'den biletin geçmiş mesajlarını çekiyoruz
            var messages = await _ticketRepository.GetTicketMessagesAsync(ticketId);

            // Eğer hiç mesaj yoksa bile boş bir liste ([]) dönecek, hata vermeyecek
            return Ok(messages);
        }

        // ==========================================
        // 🚀 2. YENİ MESAJ GÖNDERME (POST)
        // ==========================================
        [HttpPost("{ticketId}/messages")]
        [Authorize] // Sadece giriş yapmış kişiler mesaj atabilir
        public async Task<IActionResult> AddTicketMessage(int ticketId, [FromBody] AddMessageDto messageDto)
        {
            // Güvenlik: Mesajı atan kişinin ID'sini Frontend'den (Angular'dan) güvenmeyip, 
            // direkt olarak sunucudaki güvenilir JWT Token içinden çekiyoruz! (Kimse başkasının adına mesaj atamaz)
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Geçersiz kullanıcı kimliği. Lütfen tekrar giriş yapın.");
            }

            // Güvenlik 2: URL'deki ID ile paketteki ID aynı mı kontrolü
            if (ticketId != messageDto.TicketId)
            {
                return BadRequest("Bilet ID uyuşmazlığı.");
            }

            // Mesajı veritabanına ekle
            var newMessage = await _ticketRepository.AddTicketMessageAsync(ticketId, userId, messageDto.MessageText);

            if (newMessage != null)
            {
                // Başarıyla eklendiyse, eklenen mesajı (isim ve rolüyle birlikte) Angular'a geri yolluyoruz
                return Ok(newMessage);
            }

            return BadRequest("Mesaj eklenirken bir hata oluştu.");
        }
    }
}