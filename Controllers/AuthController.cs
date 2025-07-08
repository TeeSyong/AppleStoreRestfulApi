using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreRestfulApi.Models;
using OnlineStoreRestfulApi.Services;

namespace OnlineStoreRestfulApi.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("Register")]
        public IActionResult Register(User user)
        {
            var success = _auth.RegisterUser(user);
            return Ok(new { IsSuccess = success });
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var token = _auth.Login(req.Username, req.Password);
            if (token == null)
                return Unauthorized();

            return Ok(new { JWT = token });
        }
    }
}