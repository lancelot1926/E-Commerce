namespace Ecommerce.Api.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
