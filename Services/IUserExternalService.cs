using System.Threading.Tasks;
using CartonCapsApi.Models;

namespace CartonCapsApi.Services
{
    public interface IUserExternalService
    {
        Task<UserRecord> GetUserAsync(string userId);
    }
}
