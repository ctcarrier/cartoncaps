using System.Threading.Tasks;
using CartonCapsApi.Models;

namespace CartonCapsApi.Services
{
    public class MockUserExternalService : IUserExternalService
    {
        public Task<UserRecord> GetUserAsync(string userId)
        {
            // Mock some data. If user is 'user123', they have a code; otherwise no code.
            // In a real scenario, you'd call an external API here.
            if (userId == "user123")
            {
                return Task.FromResult(new UserRecord
                {
                    UserId = "user123",
                    ReferralCode = "ABC123" // This user has a referral code from external system
                });
            }
            else
            {
                return Task.FromResult(new UserRecord
                {
                    UserId = userId
                    // No referral code returned by external system
                });
            }
        }
    }
}
