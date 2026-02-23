using Microsoft.Data.SqlClient;
using System.Data;
using StajProjesi.API.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StajProjesi.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        // Proje başlarken appsettings.json'daki adresimizi buraya çeker
        public AuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // KAYIT OLMA METODU (SQL'deki sp_UserRegister'ı çalıştırır)
        public async Task<string> RegisterUserAsync(UserRegisterDto userDto)
        {
            // Şifreyi BCrypt ile güvenli, geri döndürülemez bir Hash'e çeviriyoruz
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UserRegister", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Username", userDto.Username);
                    cmd.Parameters.AddWithValue("@Email", userDto.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@RoleId", 2);

                    SqlParameter msgParam = new SqlParameter("@Message", SqlDbType.NVarChar, 100)
                    { Direction = ParameterDirection.Output };

                    SqlParameter successParam = new SqlParameter("@IsSuccess", SqlDbType.Bit)
                    { Direction = ParameterDirection.Output };

                    cmd.Parameters.Add(msgParam);
                    cmd.Parameters.Add(successParam);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                 
                    return msgParam.Value.ToString();
                }
            }
        }
        // 1. GİRİŞ YAPMA METODU (Login)
        public async Task<string> LoginUserAsync(UserLoginDto userLoginDto)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UserLogin", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", userLoginDto.Username);

                    await con.OpenAsync();

                    // SQL'den veriyi okumak için SqlDataReader kullanıyoruz
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync()) // Eğer böyle bir kullanıcı bulunduysa
                        {
                            string userId = reader["UserId"].ToString();
                            string dbPasswordHash = reader["PasswordHash"].ToString();
                            // Artık RoleId değil, SQL'den gelen RoleName sütununu okuyoruz
                            string roleName = reader["RoleName"].ToString();

                            // Şifreyi BCrypt ile doğruluyoruz
                            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(userLoginDto.Password, dbPasswordHash);

                            if (!isPasswordCorrect) return "Hata: Şifre yanlış!";

                            // Şifre doğruysa token'a userId, username ve roleName gönderiyoruz
                            return CreateToken(userId, userLoginDto.Username, roleName);
                        }
                        else
                        {
                            return "Hata: Kullanıcı bulunamadı!";
                        }
                    }
                }
            }
        }

        // 2. KİMLİK KARTI (JWT) ÜRETME METODU
        private string CreateToken(string userId, string username, string roleName)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Name, username),
        // SQL'den gelen rol adını (RoleName) direkt yaka kartına basıyoruz
        new Claim(ClaimTypes.Role, roleName)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("BenimCokGizliVeUzunStajProjesiAnahtarim123456789!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds,
                Issuer = "StajProjesiAPI",
                Audience = "StajProjesiUsers"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}