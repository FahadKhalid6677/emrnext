using System.ComponentModel.DataAnnotations;

namespace EMRNext.Core.Models
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
