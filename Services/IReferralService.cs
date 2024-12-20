using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace CartonCapsApi.Services
{
    public interface IReferralService
    {
        Task<string> GetReferralLinkAsync(string userId);
        Task<List<string>> GetReferredUsersAsync(string userId);
        Task<Data.Entities.ReferredUser> CreateReferredUserAsync(string userId, string referredUserId);
    }
}
