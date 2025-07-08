using Microsoft.AspNetCore.Mvc;
using OnlineStoreRestfulApi.Helpers;
using OnlineStoreRestfulApi.Models;
using OnlineStoreRestfulApi.Datas;
using Microsoft.AspNetCore.Authorization;

namespace OnlineStoreRestfulApi.Controllers;
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;

    public AuthController(AppDbContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }


    [HttpPost("Register")]
    public IActionResult Register(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return Ok(new { IsSuccess = true });
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var found = _context.Users.FirstOrDefault(x => x.Username == req.Username && x.Password == req.Password);
        if (found == null) return Unauthorized();

        var token = _jwtHelper.GenerateJwtToken(found.UserId);

        return Ok(new { JWT = token });
    }
}