using System.Threading.Tasks;
using StajProjesi.API.DTOs;

namespace StajProjesi.API.Data
{
    public interface IAuthRepository
    {
        Task<string> RegisterUserAsync(UserRegisterDto userDto);
        Task<string> LoginUserAsync(UserLoginDto userLoginDto);
    }
}