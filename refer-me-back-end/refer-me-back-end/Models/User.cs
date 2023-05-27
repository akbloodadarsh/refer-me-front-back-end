using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace refer_me_back_end.Models
{
    public class User
    {
        [Key]
        [JsonProperty(PropertyName = "id")]
        public string? userId { get; set; }
        public string? userName { get; set; }
        public string? password { get; set; }
        public string? company { get; set; }
        public string? dateOfBirth { get; set; }
        public string? gender { get; set; }
        public string? profilePic { get; set; }
        public string? gmail { get; set; }
        public string? resume { get; set; }
    }
}
