using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CartonCapsApi.Data;
using CartonCapsApi.Data.Entities;

namespace CartonCapsApi.Services
{
    public class ReferralService : IReferralService
    {
        private readonly ReferralContext _context;
        private readonly IUserExternalService _externalService;

        public ReferralService(ReferralContext context, IUserExternalService externalService)
        {
            _context = context;
            _externalService = externalService;
        }

        public async Task<string> GetReferralLinkAsync(string userId)
        {
            var userRecord = await _externalService.GetUserAsync(userId);

            if (string.IsNullOrEmpty(userRecord.ReferralCode))
            {
                throw new InvalidOperationException($"Cannot create referral link because no referral code found for user {userId}.");
            }

            var link = $"https://cartoncaps.link?referral_code={userRecord.ReferralCode}";
            return link;
        }

        public async Task<List<string>> GetReferredUsersAsync(string userId)
        {
            return await _context.ReferredUsers
                .Where(ru => ru.UserId == userId)
                .Select(ru => ru.ReferredUserId)
                .ToListAsync();
        }

        public async Task<Data.Entities.ReferredUser> CreateReferredUserAsync(string userId, string referredUserId)
        {
            var exists = await _context.ReferredUsers
                .AnyAsync(ru => ru.UserId == userId && ru.ReferredUserId == referredUserId);

            if (exists)
            {
                throw new InvalidOperationException("This referred user already exists for the given user.");
            }

            var referredUser = new Data.Entities.ReferredUser
            {
                UserId = userId,
                ReferredUserId = referredUserId,
                CreatedDate = DateTime.UtcNow
            };

            _context.ReferredUsers.Add(referredUser);
            await _context.SaveChangesAsync();
            return referredUser;
        }
    }
}
