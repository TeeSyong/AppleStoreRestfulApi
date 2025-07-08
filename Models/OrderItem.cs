namespace OnlineStoreRestfulApi.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }

        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
