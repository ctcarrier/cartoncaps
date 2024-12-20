using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CartonCapsApi.Services;
using CartonCapsApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

namespace CartonCapsApi.Controllers
{
    [ApiController]
    [Route("/")]
    [Authorize]
    public class ReferralController : ControllerBase
    {
        private readonly IReferralService _referralService;
        private readonly Serilog.ILogger _logger;

        public ReferralController(IReferralService referralService)
        {
            _referralService = referralService;
            _logger = Log.ForContext<ReferralController>();
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("NameIdentifier claim not found in the JWT token.");
            }
            return userId;
        }

        [HttpGet("referral-link")]
        [SwaggerOperation(
            Summary = "Retrieves a referral link",
            Description = "Retrieves a referral link for the authenticated user."
        )]
        [SwaggerResponse(statusCode: 200, description: "Referral link successfully created", type: typeof(CreateReferralLinkResponse))]
        [SwaggerResponse(statusCode: 401, description: "Unauthorized if no valid JWT is provided")]
        public async Task<IActionResult> GetReferralLink()
        {
            var userId = GetUserId();
            _logger.Information("Getting referral link for user {UserId}", userId);

            var link = await _referralService.GetReferralLinkAsync(userId);

            return Ok(new CreateReferralLinkResponse { ReferralLink = link });
        }

        [HttpGet("referrals")]
        [SwaggerOperation(
            Summary = "Retrieve all referred users",
            Description = "Returns a list of user IDs that the authenticated user has referred."
        )]
        [SwaggerResponse(statusCode: 200, description: "Returns the list of referred users")]
        [SwaggerResponse(statusCode: 401, description: "Unauthorized if no valid JWT is provided")]
        public async Task<IActionResult> GetReferredUsers()
        {
            var userId = GetUserId();
            _logger.Information("Retrieving referred users for user {UserId}", userId);

            var users = await _referralService.GetReferredUsersAsync(userId);
            return Ok(new { referrals = users });
        }

        [HttpPost("referrals")]
        [SwaggerOperation(
            Summary = "Create a referred user record",
            Description = "Creates a new referred user record with CreatedDate set to the current UTC time. If the referred user already exists for this authenticated user, returns 409 Conflict."
        )]
        [SwaggerResponse(statusCode: 201, description: "The referred user record has been created", type: typeof(CartonCapsApi.Data.Entities.ReferredUser))]
        [SwaggerResponse(statusCode: 401, description: "Unauthorized if no valid JWT is provided")]
        [SwaggerResponse(statusCode: 409, description: "The referred user already exists for this user")]
        public async Task<IActionResult> CreateReferredUser([FromBody] CreateReferredUserRequest request)
        {
            var userId = GetUserId();
            _logger.Information("Attempting to create referred user {ReferredUserId} for {UserId}", request.ReferredUserId, userId);

            // Manual validation for referredUserId
            if (string.IsNullOrWhiteSpace(request.ReferredUserId))
            {
                return BadRequest(new { error = "ReferredUserId is required." });
            }

            try
            {
                var referredUser = await _referralService.CreateReferredUserAsync(userId, request.ReferredUserId);
                return Created($"/referrals/{referredUser.Id}", referredUser);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning("Failed to create referred user {ReferredUserId} for {UserId}: {Message}", request.ReferredUserId, userId, ex.Message);
                return Conflict(new { error = ex.Message });
            }
        }
    }
}
