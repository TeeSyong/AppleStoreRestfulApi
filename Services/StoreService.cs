using Microsoft.EntityFrameworkCore;
using OnlineStoreRestfulApi.Datas;
using OnlineStoreRestfulApi.Models;

namespace OnlineStoreRestfulApi.Services
{
    public class StoreService
    {
        private readonly AppDbContext _db;

        public StoreService(AppDbContext db)
        {
            _db = db;
        }

        public List<Product> GetAllProducts() => _db.Products.ToList();

        public async Task AddToCartAsync(int userId, List<AddToCartRequest> items)
        {
            foreach (var item in items)
            {
                _db.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }

            await _db.SaveChangesAsync();
        }

        public object GetCartSummary(int userId)
        {
            var cartItems = _db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToList();

            var (orderItems, total, discount, final) = CalculateOrderSummary(cartItems);

            return new
            {
                Products = orderItems,
                TotalPrice = total,
                DiscountAmount = discount,
                FinalPrice = final
            };
        }

        public async Task<bool> ProcessOrderAsync(int userId)
        {
            var cartItems = _db.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToList();

            if (!cartItems.Any()) return false;

            var (orderItems, total, discount, final) = CalculateOrderSummary(cartItems);

            var transaction = new Transaction
            {
                UserId = userId,
                TransactionDateTime = DateTime.Now,
                OrderItems = orderItems,
                TotalPrice = total,
                DiscountAmount = discount,
                FinalPrice = final
            };

            _db.Transactions.Add(transaction);
            _db.CartItems.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            return true;
        }

        public object GetTransactionHistory(int userId)
        {
            var history = _db.Transactions
                .Include(t => t.OrderItems)
                .Where(t => t.UserId == userId)
                .ToList();

            return new
            {
                TransactionHistory = history,
                TotalPrice = history.Sum(h => h.TotalPrice),
                TotalDiscountAmount = history.Sum(h => h.DiscountAmount),
                TotalFinalPrice = history.Sum(h => h.FinalPrice)
            };
        }

        private (List<OrderItem> orderItems, decimal totalPrice, decimal discountAmount, decimal finalAmount)
            CalculateOrderSummary(List<CartItem> cartItems)
        {
            List<OrderItem> orderItems = new List<OrderItem>();
            decimal totalPrice = 0;
            decimal discountAmount = 0;

            bool hasIphone = false;
            OrderItem? cheapestItem = null;

            // Compute the total for everything first, then minus back the discount to get final price 
            foreach (var cart in cartItems)
            {
                var product = cart.Product;
                int quantity = cart.Quantity;
                decimal pricePerItem = product.ProductPrice;
                decimal subtotal = quantity * pricePerItem;

                OrderItem orderItem;

                if (product.ProductName == "iPhone")
                {
                    hasIphone = true;
                }
                // Airpods B1F1 logics at here, if customer have airpod in cart, straight up add 1/2 free airpods(depends on the airpods quantity in cart maximum is 2) 
                if (product.ProductName == "AirPods")
                {
                    int freeAirPods = Math.Min(2, quantity);
                    quantity += freeAirPods;
                    decimal discount = pricePerItem * freeAirPods;
                    discountAmount += discount;
                    subtotal = quantity * pricePerItem;

                    orderItem = new OrderItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        ProductPrice = pricePerItem,
                        Quantity = quantity,
                        DiscountAmount = discount,
                        FinalAmount = subtotal - discount
                    };
                }
                else
                {
                    orderItem = new OrderItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        ProductPrice = pricePerItem,
                        Quantity = quantity,
                        DiscountAmount = 0,
                        FinalAmount = subtotal
                    };
                }

                orderItems.Add(orderItem);
                totalPrice += subtotal;

                // Track cheapest item (excluding iPhone)
                if (product.ProductName != "iPhone")
                {
                    // Set first/any item to be cheapest item first
                    if (cheapestItem == null)
                    {
                        cheapestItem = orderItem;
                    }
                    else
                    {
                        // Then update the cheapest item by comparing cheapest item with other item in cart
                        if (pricePerItem < cheapestItem.ProductPrice)
                        {
                            cheapestItem = orderItem;
                        }
                    }
                }
            }

            // Ipad discount logic
            foreach (var item in orderItems)
            {
                if (item.ProductName == "iPad")
                {
                    decimal discount = item.ProductPrice * 0.10m * item.Quantity;
                    item.DiscountAmount += discount;
                    item.FinalAmount -= discount;
                    discountAmount += discount;
                }
            }

            // Iphone discount logic 
            if (hasIphone && cheapestItem != null)
            {
                decimal iphoneDiscount = cheapestItem.ProductPrice * 0.50m;
                cheapestItem.DiscountAmount += iphoneDiscount;
                cheapestItem.FinalAmount -= iphoneDiscount;
                discountAmount += iphoneDiscount;
            }

            decimal finalAmount = totalPrice - discountAmount;

            return (orderItems, totalPrice, discountAmount, finalAmount);
        }
    }
}