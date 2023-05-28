using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using refer_me_back_end.Models;
using refer_me_back_end.Services.JWTAuthService;
using System.Net;
using static refer_me_back_end.Services.CosmosService.UserCosmosService;

namespace refer_me_back_end.Repositories
{
    public interface IUserRepositories
    {
        Task<ActionResult<IEnumerable<User>>> GetUsers();
        Task<ActionResult<User>> GetUser(string user_id);
        Task<ActionResult<string>> CreateUser(User user);
        Task<ActionResult<string>> UpdateUser(User user);
        Task<ActionResult<string>> DeleteUser(string user_id);
        Task<string> AuthenticateUser(string user_name, string password);
    }

    public class UserRepository : IUserRepositories
    {
        private readonly IUserCosmosDbService _userCosmosDbService;
        private IEnumerable<User> users;
        private IJwtTokenManager _jwtTokenManager;
        private IDistributedCache _cache;
        public UserRepository(IUserCosmosDbService usersCosmosDbService, IJwtTokenManager jwtTokenManager, IDistributedCache cache)
        {
            _userCosmosDbService = usersCosmosDbService;
            _jwtTokenManager = jwtTokenManager;
            _cache = cache;
        }
        //public UserRepository(IUserCosmosDbService userCosmosDbService, IJwtTokenManager jwtTokenManager)
        //{
        //    _userCosmosDbService = userCosmosDbService;
        //    _jwtTokenManager = jwtTokenManager;
        //}

        // For deleting user
        public async Task<ActionResult<string>> DeleteUser(string userId)
        {
            await _userCosmosDbService.DeleteUserAsync(userId);
            return "success";
        }

        // For updating user
        public async Task<ActionResult<string>> UpdateUser(User user)
        {
            await _userCosmosDbService.UpdateUserAsync(user);
            return "success";
        }

        // For getting all users info
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var cache_users = _cache.GetString("all_users_list");
            if (cache_users == null)
            {
                users = await _userCosmosDbService.GetUsers();
                //return users.ToList<User>();
                _cache.SetString("all_users_list", JsonConvert.SerializeObject(users));
            }
            else
            {
                users = JsonConvert.DeserializeObject<IEnumerable<User>>(cache_users);
            }
            return users.ToList();
        }

        // For getting single user info
        public async Task<ActionResult<User>> GetUser(string userId)
        {
            var user_data = _cache.GetString(userId);
            if (user_data == null)
            {
                var user = await _userCosmosDbService.GetUserAsync(userId);
                //return user;
                _cache.SetString(userId, JsonConvert.SerializeObject(user));
            }

            return JsonConvert.DeserializeObject<User>(user_data);
        }

        // For Adding User
        public async Task<ActionResult<string>> CreateUser(User user)
        {
            user.userId = Guid.NewGuid().ToString();
            await _userCosmosDbService.CreateUserAsync(user);
            return "success";
        }

        // For Authentication
        public Task<string> AuthenticateUser(string user_name, string password)
        {
            var token = _jwtTokenManager.Authenticate(user_name, password).Result;
            if (string.IsNullOrEmpty(token))
                return Task.FromResult(HttpStatusCode.Unauthorized.ToString());
            return Task.FromResult(token);
        }
    }
}
