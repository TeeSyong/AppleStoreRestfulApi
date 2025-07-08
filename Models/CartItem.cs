namespace OnlineStoreRestfulApi.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int UserId { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public Product Product { get; set; } = null!;
    }
}
