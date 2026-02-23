using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StajProjesi.API.DTOs;

namespace StajProjesi.API.Data
{
    // Bu sınıfın, az önce yazdığımız ITicketRepository sözleşmesine uyacağını söylüyoruz:
    public class TicketRepository : ITicketRepository
    {
        private readonly string _connectionString;

        // appsettings.json içindeki veritabanı şifremizi alıyoruz
        public TicketRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Sözleşmedeki işi gerçekten yapan asıl metodumuz:
        public async Task<int> CreateTicketAsync(int userId, TicketCreateDto ticketDto, int predictedCategoryId)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // NOT: Eğer Stored Procedure kullanıyorsan ismini kontrol et (Örn: sp_CreateTicket)
                using (var cmd = new SqlCommand("sp_CreateTicket", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parametreleri ekle
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Title", ticketDto.Title);
                    cmd.Parameters.AddWithValue("@Description", ticketDto.Description);
                    cmd.Parameters.AddWithValue("@Urgency", ticketDto.Urgency);

                    // --- YENİ EKLENEN KISIM ---
                    cmd.Parameters.AddWithValue("@PredictedCategoryId", predictedCategoryId);
                    // --------------------------

                    // ID'yi geri döndür (SELECT SCOPE_IDENTITY() yaptığını varsayıyoruz)
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        // YENİ METOT: Kullanıcının kendi biletlerini okuyup listeler
        public async Task<IEnumerable<TicketListDto>> GetTicketsByUserIdAsync(int userId)
        {
            // Verileri dolduracağımız boş bir kargo filosu (Liste) oluşturuyoruz
            var tickets = new List<TicketListDto>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Demin SQL'de yazdığımız okuma prosedürünü çağırıyoruz
                using (SqlCommand cmd = new SqlCommand("sp_GetTicketsByUserId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Prosedürün beklediği o kimlik numarasını (@UserId) C#'tan fırlatıyoruz
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    await con.OpenAsync();

                    // SqlDataReader: SQL'den gelen tabloyu satır satır okuyan özel bir robottur
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        // Okunacak satır (bilet) olduğu sürece döngüyü döndür
                        while (await reader.ReadAsync())
                        {
                            var ticket = new TicketListDto
                            {
                                TicketId = Convert.ToInt32(reader["TicketId"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),

                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                Urgency = reader["Urgency"] != DBNull.Value ? reader["Urgency"].ToString() : null,
                                CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : null,
                                UserName = reader["UserName"] != DBNull.Value ? reader["UserName"].ToString() : "Bilinmiyor",
                                PredictedCategoryId = reader["PredictedCategoryId"] != DBNull.Value ? Convert.ToInt32(reader["PredictedCategoryId"]) : (int?)null,
                                FinalCategoryId = reader["FinalCategoryId"] != DBNull.Value ? Convert.ToInt32(reader["FinalCategoryId"]) : (int?)null
                            };

                            // Doldurulan kargo kutusunu filoya (Listeye) ekle
                            tickets.Add(ticket);
                        }
                    }
                }
            }

            // Tüm biletler listeye dolduğunda bu listeyi Controller'a teslim et
            return tickets;
        }
        public async Task<IEnumerable<TicketListDto>> GetAllTicketsAsync()
        {
            var tickets = new List<TicketListDto>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllTickets", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var ticket = new TicketListDto
                            {
                                TicketId = Convert.ToInt32(reader["TicketId"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                // Status ve Urgency null gelebilir kontrolü (Mevcut kodun)
                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                Urgency = reader["Urgency"] != DBNull.Value ? reader["Urgency"].ToString() : null,
                                CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : null,
                                UserName = reader["UserName"] != DBNull.Value ? reader["UserName"].ToString() : "Bilinmiyor",

                                // --- YENİ EKLENEN GÜVENLİ KISIM ---
                                // Eğer veri tabanında bu alan boşsa (DBNull), C# tarafında null yap.
                                // Doluysa sayıya (int) çevir.
                                PredictedCategoryId = reader["PredictedCategoryId"] != DBNull.Value
                                                      ? Convert.ToInt32(reader["PredictedCategoryId"])
                                                      : (int?)null,

                                FinalCategoryId = reader["FinalCategoryId"] != DBNull.Value
                                                  ? Convert.ToInt32(reader["FinalCategoryId"])
                                                  : (int?)null
                            };

                            tickets.Add(ticket);
                        }
                    }
                }
            }
            return tickets;
        }
        public async Task<bool> UpdateTicketAsync(TicketUpdateDto updateDto)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateTicket", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parametreleri SP'ye gönderiyoruz
                    cmd.Parameters.AddWithValue("@TicketId", updateDto.TicketId);
                    cmd.Parameters.AddWithValue("@Status", updateDto.Status);
                    cmd.Parameters.AddWithValue("@FinalCategoryId", updateDto.FinalCategoryId);

                    // 🚀 YENİ EKLENEN KISIM: Admin Yanıtı (Null kontrolü ile birlikte SQL'e güvenli gönderim)
                    cmd.Parameters.AddWithValue("@AdminResponse", string.IsNullOrWhiteSpace(updateDto.AdminResponse) ? DBNull.Value : updateDto.AdminResponse);

                    await con.OpenAsync();

                    // ExecuteNonQuery: Geriye etkilenen satır sayısını döner
                    await cmd.ExecuteNonQueryAsync();

                    return true;
                }
            }
        }
        public async Task<TicketListDto> GetTicketByIdAsync(int ticketId)
        {
            TicketListDto ticket = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetTicketById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);
                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            ticket = new TicketListDto
                            {
                                TicketId = Convert.ToInt32(reader["TicketId"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                Urgency = reader["Urgency"] != DBNull.Value ? reader["Urgency"].ToString() : null,
                                CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : null,
                                UserName = reader["UserName"] != DBNull.Value ? reader["UserName"].ToString() : "Bilinmiyor",
                                PredictedCategoryId = reader["PredictedCategoryId"] != DBNull.Value ? Convert.ToInt32(reader["PredictedCategoryId"]) : (int?)null,
                                FinalCategoryId = reader["FinalCategoryId"] != DBNull.Value ? Convert.ToInt32(reader["FinalCategoryId"]) : (int?)null
                            };
                        }
                    }
                }
            }
            return ticket;
        }
        public async Task<List<TicketMessageDto>> GetTicketMessagesAsync(int ticketId)
        {
            var messages = new List<TicketMessageDto>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetTicketMessages", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);

                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            messages.Add(new TicketMessageDto
                            {
                                MessageId = Convert.ToInt32(reader["MessageId"]),
                                TicketId = Convert.ToInt32(reader["TicketId"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                MessageText = reader["MessageText"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                SenderName = reader["SenderName"].ToString(),
                                SenderRoleId = Convert.ToInt32(reader["SenderRoleId"])
                            });
                        }
                    }
                }
            }
            return messages;
        }
        public async Task<TicketMessageDto> AddTicketMessageAsync(int ticketId, int userId, string messageText)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_AddTicketMessage", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TicketId", ticketId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@MessageText", messageText);

                    await con.OpenAsync();

                    // Sadece Insert yapmıyoruz, eklenen mesajı detaylarıyla (isim, rol) geri okuyoruz
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new TicketMessageDto
                            {
                                MessageId = Convert.ToInt32(reader["MessageId"]),
                                TicketId = Convert.ToInt32(reader["TicketId"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                MessageText = reader["MessageText"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                SenderName = reader["SenderName"].ToString(),
                                SenderRoleId = Convert.ToInt32(reader["SenderRoleId"])
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}