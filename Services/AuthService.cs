using OnlineStoreRestfulApi.Datas;
using OnlineStoreRestfulApi.Helpers;
using OnlineStoreRestfulApi.Models;

namespace OnlineStoreRestfulApi.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthService(AppDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        public bool RegisterUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return true;
        }

        public string? Login(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Username == username && x.Password == password);

            if (user == null)
                return null;

            return _jwtHelper.GenerateJwtToken(user.UserId);
        }
    }
}