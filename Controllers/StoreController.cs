using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreRestfulApi.Models;
using OnlineStoreRestfulApi.Services;
using System.Security.Claims;

namespace OnlineStoreRestfulApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly StoreService _store;

        public StoreController(StoreService store)
        {
            _store = store;
        }

        [HttpGet("Products")]
        public ActionResult<List<Product>> GetProducts()
        {
            return _store.GetAllProducts();
        }

        [Authorize]
        [HttpGet("test-auth")]
        public IActionResult Test()
        {
            var userId = User.FindFirstValue("userId");
            return Ok(new { message = "Authenticated!", userId });
        }

        [Authorize]
        [HttpPost("AddToCart")]
        public async Task<ActionResult<bool>> AddToCart([FromBody] List<AddToCartRequest> items)
        {
            int userId = int.Parse(User.FindFirstValue("userId")!);
            await _store.AddToCartAsync(userId, items);
            return true;
        }

        [Authorize]
        [HttpGet("GetCart")]
        public IActionResult GetCart()
        {
            int userId = int.Parse(User.FindFirstValue("userId")!);
            return Ok(_store.GetCartSummary(userId));
        }

        [Authorize]
        [HttpPost("ProcessOrderItem")]
        public async Task<ActionResult<bool>> ProcessOrder()
        {
            int userId = int.Parse(User.FindFirstValue("userId")!);
            var success = await _store.ProcessOrderAsync(userId);
            if (!success) return BadRequest("Cart is empty.");
            return true;
        }

        [Authorize]
        [HttpGet("GetTransactionHistory")]
        public IActionResult GetTransactionHistory()
        {
            int userId = int.Parse(User.FindFirstValue("userId")!);
            return Ok(_store.GetTransactionHistory(userId));
        }
    }
}