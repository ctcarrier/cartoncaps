using Swashbuckle.AspNetCore.Annotations;

namespace CartonCapsApi.Models
{
    [SwaggerSchema(Description = "Request body for creating a new referred user.")]
    public class CreateReferredUserRequest
    {
        [SwaggerSchema("The ID of the user being referred.")]
        public string ReferredUserId { get; set; } = string.Empty;
    }
}
