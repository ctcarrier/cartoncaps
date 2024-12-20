using System;
using Swashbuckle.AspNetCore.Annotations;

namespace CartonCapsApi.Data.Entities
{
    [SwaggerSchema(Description = "Represents a user that was referred by another user.")]
    public class ReferredUser
    {
        [SwaggerSchema("The primary key of this referred user record.")]
        public int Id { get; set; }

        [SwaggerSchema("The ID of the user who made the referral.")]
        public string UserId { get; set; } = string.Empty;

        [SwaggerSchema("The ID of the user that was referred.")]
        public string ReferredUserId { get; set; } = string.Empty;

        [SwaggerSchema("The UTC date/time when this referred user record was created.")]
        public DateTime CreatedDate { get; set; }
    }
}
