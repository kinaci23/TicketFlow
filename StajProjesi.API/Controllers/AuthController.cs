using Microsoft.AspNetCore.Mvc;
using StajProjesi.API.Data;
using StajProjesi.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace StajProjesi.API.Controllers
{
    // 1. ADRES TANIMLAMASI: Bu sınıfın bir API kapısı olduğunu ve adresinin "api/auth" olacağını belirtiyoruz.
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        // 2. BAĞIMLILIK ENJEKSİYONU (Dependency Injection): 
        // "Bana o veritabanıyla konuşan işçimizi (AuthRepository) getir" diyoruz.
        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        // 3. KAYIT OLMA KAPISI (POST: api/auth/register)
        // Dışarıdan "yeni bir veri" gönderileceği için HttpPost metodunu kullanıyoruz.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
        {
            // Resepsiyonist (Controller) veriyi Postman'den alır ve İşçiye (Repository) verir.
            string resultMessage = await _authRepository.RegisterUserAsync(userDto);

            // SQL'den gelen mesaja bakıyoruz. Eğer "Hata" kelimesiyle başlıyorsa:
            // 400 Bad Request (Geçersiz İstek) HTTP durum koduyla beraber hatayı JSON olarak dönüyoruz.
            if (resultMessage.StartsWith("Hata") || resultMessage.StartsWith("Kritik"))
            {
                return BadRequest(new { message = resultMessage });
            }

            // Her şey yolundaysa: 200 OK (Başarılı) mesajı ile sonucu dönüyoruz.
            return Ok(new { message = resultMessage });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
        {
            
            var result = await _authRepository.LoginUserAsync(userDto);

            // Eğer Repository'den dönen cevap "Hata:" ile başlıyorsa (yanlış şifre vs.)
            if (result.StartsWith("Hata:"))
            {
                return BadRequest(new { message = result }); // 400 Koduyla hatayı fırlat
            }

            // Her şey yolundaysa, üretilen o şifreli Token'ı 200 OK koduyla kullanıcıya ver!
            return Ok(new { token = result });
        }
        // Başına [Authorize] yazdığımız için Token'ı olmayan BURAYA GİREMEZ!
        [Authorize]
        [HttpGet("gizli-oda")]
        public IActionResult GizliOda()
        {
            return Ok(new { message = "Kimlik kartın onaylandı, gizli odaya girmeyi başardın!" });
        }

    }
}