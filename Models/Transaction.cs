namespace OnlineStoreRestfulApi.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public DateTime TransactionDateTime { get; set; } = DateTime.Now;

        public List<OrderItem> OrderItems { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
