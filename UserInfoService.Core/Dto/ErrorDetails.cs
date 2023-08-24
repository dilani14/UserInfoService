using System.Text.Json;

namespace UserInfoService.Core.Dto
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public static class ErrorMsg
    {
        public const string INVALID_NAME_ERR_MSG = "An entry with the identical name already exists. Please select a different name.";
        public const string INVALID_ID_ERR_MSG = "The provided Id does not have associated User Information. Please provide a valid Id.";
    }
}
