using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace refer_me_back_end.Models
{
    public enum ReferType
    {
        canRefer,
        needRefer
    };

    public class JobPost
    {
        [Key]
        [JsonProperty(PropertyName = "id")]
        public string? postId { get; set; }
        public ReferType referType { get; set; }
        public string? userId { get; set; }
        public string? jobId { get; set; }
        public string? jobDescription { get; set; }
        public string? jobLink { get; set; }
        DateTime deleteOn { get; set; }
        public string? forCompany { get; set; }
        public string? roleName { get; set; }
        public string? jobType { get; set; }
        public int experienceRequired { get; set; }
        public string? jobLocationCountry { get; set; }
    }
}
