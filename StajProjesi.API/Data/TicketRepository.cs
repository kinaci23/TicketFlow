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
        public async Task<int> CreateTicketAsync(int userId, TicketCreateDto ticketDto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_CreateTicket", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Prosedürün beklediği 3 parametreyi gönderiyoruz
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Title", ticketDto.Title);
                    cmd.Parameters.AddWithValue("@Description", ticketDto.Description);

                    await conn.OpenAsync();

                    // Veritabanından gelen yeni Bilet ID'sini yakalıyoruz
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
                                CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : null
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
                            tickets.Add(new TicketListDto
                            {
                                TicketId = Convert.ToInt32(reader["TicketId"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                Urgency = reader["Urgency"] != DBNull.Value ? reader["Urgency"].ToString() : null,
                                CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : null
                            });
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

                    await con.OpenAsync();

                    // ExecuteNonQuery: Geriye etkilenen satır sayısını döner. 
                    // Eğer bilet bulunduysa ve güncellendiyse 1 dönecektir.
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
            }
        }
    }
}