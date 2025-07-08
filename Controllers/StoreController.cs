using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStoreRestfulApi.Datas;
using OnlineStoreRestfulApi.Models;
using System.Security.Claims;

namespace OnlineStoreRestfulApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StoreController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("Products")]
        public ActionResult<List<Product>> GetProducts()
        {
            return _db.Products.ToList();
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
        public ActionResult<bool> AddToCart([FromBody] List<AddToCartRequest> items)
        {
            int userId = int.Parse(User.FindFirstValue("userId"));

            foreach (var item in items)
            {
                _db.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }

            _db.SaveChanges();
            return true;
        }

        [Authorize]
        [HttpGet("GetCart")]
        public IActionResult GetCart()
        {
            int userId = int.Parse(User.FindFirstValue("userId"));

            var cartItems = _db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToList();

            var grouped = cartItems.GroupBy(c => c.ProductId);
            List<OrderItem> orderItems = new();
            decimal totalPrice = 0, discountAmount = 0;

            int airpodCount = cartItems
                .Where(c => c.Product.ProductName == "AirPods")
                .Sum(c => c.Quantity);

            bool iphoneInCart = cartItems.Any(c => c.Product.ProductName == "iPhone");

            foreach (var group in grouped)
            {
                var prod = group.First().Product;
                int quantity = group.Sum(x => x.Quantity);
                decimal price = prod.ProductPrice;
                decimal discount = 0;

                if (prod.ProductName == "iPad")
                    discount = price * 0.10M * quantity;
                else if (prod.ProductName == "AirPods")
                    discount = price * Math.Min(2, quantity);
                else if (iphoneInCart && prod.ProductName != "iPhone")
                    discount = price * 0.50M;

                var finalAmount = (price * quantity) - discount;

                orderItems.Add(new OrderItem
                {
                    ProductId = prod.ProductId,
                    ProductName = prod.ProductName,
                    ProductPrice = price,
                    Quantity = quantity,
                    DiscountAmount = discount,
                    FinalAmount = finalAmount
                });

                totalPrice += price * quantity;
                discountAmount += discount;
            }

            decimal finalPrice = totalPrice - discountAmount;

            return Ok(new
            {
                Products = orderItems,
                TotalPrice = totalPrice,
                DiscountAmount = discountAmount,
                FinalPrice = finalPrice
            });
        }

        [Authorize]
        [HttpPost("ProcessOrderItem")]
        public ActionResult<bool> ProcessOrder()
        {
            int userId = int.Parse(User.FindFirstValue("userId"));

            var cartItems = _db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToList();

            if (!cartItems.Any())
                return BadRequest("Cart is empty.");

            var grouped = cartItems.GroupBy(c => c.ProductId);
            List<OrderItem> orderItems = new();
            decimal totalPrice = 0, discountAmount = 0;

            int airpodCount = cartItems
                .Where(c => c.Product.ProductName == "AirPods")
                .Sum(c => c.Quantity);

            bool iphoneInCart = cartItems.Any(c => c.Product.ProductName == "iPhone");

            foreach (var group in grouped)
            {
                var prod = group.First().Product;
                int quantity = group.Sum(x => x.Quantity);
                decimal price = prod.ProductPrice;
                decimal discount = 0;

                if (prod.ProductName == "iPad")
                    discount = price * 0.10m * quantity;
                else if (prod.ProductName == "AirPods")
                    discount = price * Math.Min(2, quantity);
                else if (iphoneInCart && prod.ProductName != "iPhone")
                    discount = price * 0.50m;

                var finalAmount = (price * quantity) - discount;

                orderItems.Add(new OrderItem
                {
                    ProductId = prod.ProductId,
                    ProductName = prod.ProductName,
                    ProductPrice = price,
                    Quantity = quantity,
                    DiscountAmount = discount,
                    FinalAmount = finalAmount
                });

                totalPrice += price * quantity;
                discountAmount += discount;
            }

            var transaction = new Transaction
            {
                UserId = userId,
                TransactionDateTime = DateTime.Now,
                OrderItems = orderItems,
                TotalPrice = totalPrice,
                DiscountAmount = discountAmount,
                FinalPrice = totalPrice - discountAmount
            };

            _db.Transactions.Add(transaction);
            _db.CartItems.RemoveRange(cartItems);
            _db.SaveChanges();

            return true;
        }

        [Authorize]
        [HttpGet("GetTransactionHistory")]
        public IActionResult GetTransactionHistory()
        {
            int userId = int.Parse(User.FindFirstValue("userId"));

            var history = _db.Transactions
                .Include(t => t.OrderItems)
                .Where(t => t.UserId == userId)
                .ToList();

            return Ok(new
            {
                TransactionHistory = history,
                TotalPrice = history.Sum(h => h.TotalPrice),
                TotalDiscountAmount = history.Sum(h => h.DiscountAmount),
                TotalFinalPrice = history.Sum(h => h.FinalPrice)
            });
        }
    }
}