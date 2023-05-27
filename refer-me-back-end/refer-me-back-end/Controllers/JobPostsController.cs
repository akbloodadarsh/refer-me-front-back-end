using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using refer_me_back_end.Models;
using refer_me_back_end.Repositories;

namespace refer_me_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPostsController : ControllerBase
    {
        private IJobPostsRepositories _jobPostRepositories;
        public JobPostsController(IJobPostsRepositories jobPostRepositories)
        {
            _jobPostRepositories = jobPostRepositories;
        }

        // For deleting jobPost
        [Authorize]
        [HttpDelete("delete-job-post")]
        public async Task<ActionResult<JobPost>> DeleteJobPost(string jobPostId)
        {
            await _jobPostRepositories.DeleteJobPost(jobPostId);
            return Ok("JobPost deleted successfully");
        }

        // For updating jobPost
        [Authorize]
        [HttpPut("update-job-post")]
        public async Task<ActionResult<JobPost>> UpdateJobPost(JobPost jobPost)
        {
            await _jobPostRepositories.UpdateJobPost(jobPost);
            return Ok("jobPost updated successfully");
        }

        // For getting all jobPost
        [HttpGet("get-job-posts")]
        public async Task<ActionResult<IEnumerable<JobPost>>> GetJobPosts()
        {
            return await _jobPostRepositories.GetJobPosts();
        }

        // For getting single jobPost
        [HttpGet("get-job-post")]
        public async Task<ActionResult<JobPost>> GetJobPost(string jobPostId)
        {
            return await _jobPostRepositories.GetJobPost(jobPostId);
        }

        // For creating jobPost
        [Authorize]
        [HttpPost("create-job-post")]
        public async Task<string> CreateJobPost(JobPost jobPost)
        {
            await _jobPostRepositories.CreateJobPost(jobPost);
            return JsonConvert.SerializeObject(new { data = "JobPost Created" });
        }
    }
}
