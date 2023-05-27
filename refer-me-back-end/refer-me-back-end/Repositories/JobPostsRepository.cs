using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using refer_me_back_end.Models;
using System.Linq;
using static refer_me_back_end.Services.CosmosService.JobPostsCosmosService;

namespace refer_me_back_end.Repositories
{
    public interface IJobPostsRepositories
    {
        Task<ActionResult<IEnumerable<JobPost>>> GetJobPosts();
        Task<ActionResult<JobPost>> GetJobPost(string jobPostId);
        Task<ActionResult<string>> CreateJobPost(JobPost jobPost);
        Task<ActionResult<string>> UpdateJobPost(JobPost jobPost);
        Task<ActionResult<string>> DeleteJobPost(string jobPostId);
    }

    public class JobPostsRepositories : IJobPostsRepositories
    {
        private readonly IJobPostsCosmosDbService _jobPostsCosmosDbService;
        private IEnumerable<JobPost> jobPosts;
        //private IDistributedCache _cache;
        //public JobPostsRepositories(IJobPostsCosmosDbService jobPostsCosmosDbService, IDistributedCache cache)
        //{
        //    _jobPostsCosmosDbService = jobPostsCosmosDbService;
        //    _cache = cache;
        //}
        public JobPostsRepositories(IJobPostsCosmosDbService jobPostsCosmosDbService)
        {
            _jobPostsCosmosDbService = jobPostsCosmosDbService;
        }

        // For deleting jobPost
        public async Task<ActionResult<string>> DeleteJobPost(string jobPostId)
        {
            await _jobPostsCosmosDbService.DeleteJobPostAsync(jobPostId);
            return "success";
        }

        // For updating jobPost
        public async Task<ActionResult<string>> UpdateJobPost(JobPost jobPost)
        {
            await _jobPostsCosmosDbService.UpdateJobPostAsync(jobPost);
            return "success";
        }

        // For getting all jobPosts info
        public async Task<ActionResult<IEnumerable<JobPost>>> GetJobPosts()
        {
            //var cache_job_posts = _cache.GetString("all_posts_list");
            //if (cache_job_posts == null)
            //{
                jobPosts = await _jobPostsCosmosDbService.GetJobPosts();
            return jobPosts.ToList<JobPost>();
                //_cache.SetString("all_posts_list", JsonConvert.SerializeObject(jobPosts));
            //}
            //else
            //{
            //    jobPosts = JsonConvert.DeserializeObject<IEnumerable<JobPost>>(cache_job_posts);
            //}

            //return jobPosts.ToList();
        }

        // For getting single jobPost info
        public async Task<ActionResult<JobPost>> GetJobPost(string jobPostId)
        {
            //var job_post_data = _cache.GetString(jobPostId);
            //if (job_post_data == null)
            //{
                var job_post = await _jobPostsCosmosDbService.GetJobPostAsync(jobPostId);
                //_cache.SetString(jobPostId, JsonConvert.SerializeObject(job_post));
                return job_post;
            //}
            //return JsonConvert.DeserializeObject<JobPost>(job_post_data);
        }

        // For Adding JobPost
        public async Task<ActionResult<string>> CreateJobPost(JobPost jobPost)
        {
            jobPost.postId = Guid.NewGuid().ToString();
            await _jobPostsCosmosDbService.CreateJobPostAsync(jobPost);
            return "success";
        }
    }
}
