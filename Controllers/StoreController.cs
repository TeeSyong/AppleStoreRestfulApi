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
        [HttpPost("AddToCart")]
        public async Task<ActionResult<bool>> AddToCart([FromBody] List<AddToCartRequest> items)
        {
            int userId = int.Parse(User.FindFirstValue("userId")!);
            await _store.AddToCartAsync(userId, items);
            return Ok(new
            {
                isSuccess = true,
                message = "Items successfully added to cart!"
            });
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
            if (!success)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Your cart is empty, nothing to checkout."
                });
            }

            return Ok(new
            {
                isSuccess = true,
                message = "Order successfully processed!"
            });
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