using System.ComponentModel.DataAnnotations;

namespace UserInfoService.Core.Dto
{
    public class AddOrUpdateUserInfoRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
    }
}
