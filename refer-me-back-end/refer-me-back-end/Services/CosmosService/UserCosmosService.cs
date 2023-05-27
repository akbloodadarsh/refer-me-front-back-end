using Microsoft.Azure.Cosmos;
using System.Collections.Concurrent;
using System.ComponentModel;
using refer_me_back_end.Models;
using System.Net;
using Container = Microsoft.Azure.Cosmos.Container;
using User = refer_me_back_end.Models.User;

namespace refer_me_back_end.Services.CosmosService
{
    public class UserCosmosService
    {
        public interface IUserCosmosDbService
        {
            Task<IEnumerable<User>> GetUsers();
            Task<User> GetUserAsync(string userId);
            Task CreateUserAsync(User user);
            Task UpdateUserAsync(User user);
            Task DeleteUserAsync(string userId);
            Task<HttpStatusCode> AuthenticateUserAsync(string user_name, string password);
        }

        public class UserCosmosDbService : IUserCosmosDbService
        {
            private Container _container;
            const string userContainerName = "Users";
            public UserCosmosDbService(CosmosClient cosmosDbClient, string dataBaseName, string containerName)
            {
                _container = cosmosDbClient.GetContainer(dataBaseName, containerName);
            }

            // For Authenticating user
            public async Task<HttpStatusCode> AuthenticateUserAsync(string userName, string password)
            {
                var query = _container.GetItemQueryIterator<User>(new QueryDefinition($"SELECT * FROM Users As user WHERE user.userName='{userName}'"));

                var results = new List<User>();
                while (query.HasMoreResults)
                {
                    var respose = await query.ReadNextAsync();
                    results.AddRange(respose.ToList());
                }

                if (results.Count == 0)
                    return HttpStatusCode.NotFound;
                else if (results[0].password == password)
                    return HttpStatusCode.OK;
                return HttpStatusCode.Unauthorized;
            }

            // For deleting user
            public async Task DeleteUserAsync(string userId)
            {
                await _container.DeleteItemAsync<User>(userId, new PartitionKey(userId));
            }

            // For updating user
            public async Task UpdateUserAsync(User user)
            {
                await _container.UpsertItemAsync(user, new PartitionKey(user.userId));
            }

            // For getting all user info
            public async Task<IEnumerable<User>> GetUsers()
            {
                try
                {
                    var query = _container.GetItemQueryIterator<User>(new QueryDefinition($"SELECT * FROM {userContainerName}"));
                    var results = new List<User>();

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

            // For getting single user info
            public async Task<User> GetUserAsync(string userId)
            {
                try
                {
                    var respose = await _container.ReadItemAsync<User>(userId, new PartitionKey(userId));
                    return respose.Resource;
                }
                catch (CosmosException ex)
                {
                    return null;
                }
            }

            // For Adding User
            public async Task CreateUserAsync(User user)
            {
                await _container.CreateItemAsync(user);
            }
        }
    }
}
