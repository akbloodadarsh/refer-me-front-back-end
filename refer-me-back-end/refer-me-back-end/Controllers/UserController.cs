using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using refer_me_back_end.Repositories;
using Microsoft.AspNetCore.Http;
using refer_me_back_end.Models;

namespace refer_me_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private IUserRepositories _userRepositories;
        public UserController(IUserRepositories usersRepositories)
        {
            _userRepositories = usersRepositories;
        }

        // For deleting user
        [Authorize]
        [HttpDelete("delete-user")]
        public async Task<ActionResult<User>> DeleteUser(string userId)
        {
            await _userRepositories.DeleteUser(userId);
            return Ok("User deleted successfully");
        }

        // For updating user
        [Authorize]
        [HttpPut("update-user")]
        public async Task<ActionResult<User>> UpdateUser(User user)
        {
            await _userRepositories.UpdateUser(user);
            return Ok("user updated successfully");
        }

        // For getting all users
        [AllowAnonymous]
        [HttpGet("get-users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _userRepositories.GetUsers();
        }

        // For getting single user
        [AllowAnonymous]
        [HttpGet("get-user")]
        public async Task<ActionResult<User>> GetUser(string userId)
        {
            return await _userRepositories.GetUser(userId);
        }

        // For creating user
        [AllowAnonymous]
        [HttpPost("create-user")]
        public async Task<string> CreateUser(User user)
        {
            await _userRepositories.CreateUser(user);
            return JsonConvert.SerializeObject(new { data = "User Created" });
        }

        // For Authentication
        [AllowAnonymous]
        [HttpPost("authenticate-user")]
        public Task<string> AuthenticateUser([FromBody] User user)
        {
            string response = _userRepositories.AuthenticateUser(user.userName, user.password).Result;
            return Task.FromResult(response);
        }
    }
}
