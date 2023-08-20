using System.ComponentModel.DataAnnotations;

namespace UserInfoService.Core.Models
{
    public class AddOrUpdateUserInfoRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
    }
}
