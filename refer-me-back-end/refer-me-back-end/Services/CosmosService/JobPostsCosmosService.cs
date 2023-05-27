using Microsoft.Azure.Cosmos;
using System.Collections.Concurrent;
using System.ComponentModel;
using refer_me_back_end.Models;
using System.Net;
using Container = Microsoft.Azure.Cosmos.Container;
using User = refer_me_back_end.Models.User;

namespace refer_me_back_end.Services.CosmosService
{
    public class JobPostsCosmosService
    {
        public interface IJobPostsCosmosDbService
        {
            Task<IEnumerable<JobPost>> GetJobPosts();
            Task<JobPost> GetJobPostAsync(string jobPostId);
            Task CreateJobPostAsync(JobPost jobPost);
            Task UpdateJobPostAsync(JobPost jobPost);
            Task DeleteJobPostAsync(string user_id);
            //Task<string> AuthenticateUserAsync(string user_name, string password);
        }

        public class JobPostsCosmosDbService : IJobPostsCosmosDbService
        {
            private Container _container;
            public JobPostsCosmosDbService(CosmosClient cosmosDbClient, string dataBaseName, string containerName)
            {
                _container = cosmosDbClient.GetContainer(dataBaseName, containerName);
            }

            // For deleting JobPost
            public async Task DeleteJobPostAsync(string jobPostId)
            {
                await _container.DeleteItemAsync<JobPost>(jobPostId, new PartitionKey(jobPostId));
            }

            // For updating JobPost
            public async Task UpdateJobPostAsync(JobPost jobPost)
            {
                await _container.UpsertItemAsync(jobPost, new PartitionKey(jobPost.postId));
            }

            // For getting all JobPost info
            public async Task<IEnumerable<JobPost>> GetJobPosts()
            {
                try
                {
                    var query = _container.GetItemQueryIterator<JobPost>(new QueryDefinition("SELECT * FROM JobPosts"));
                    var results = new List<JobPost>();

                    while (query.HasMoreResults)
                    {
                        var respose = await query.ReadNextAsync();
                        results.AddRange(respose.ToList());
                    }
                    return results;
                }
                catch (CosmosException ex)
                {
                    return null;
                }
            }

            // For getting single JobPost info
            public async Task<JobPost> GetJobPostAsync(string jobPostId)
            {
                try
                {
                    var respose = await _container.ReadItemAsync<JobPost>(jobPostId, new PartitionKey(jobPostId));
                    return respose.Resource;
                }
                catch (CosmosException ex)
                {
                    return null;
                }
            }

            // For Adding JobPost
            public async Task CreateJobPostAsync(JobPost jobPost)
            {
                await _container.CreateItemAsync(jobPost);
            }
        }
    }
}
