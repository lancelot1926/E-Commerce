namespace Ecommerce.Api.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ShoppingCartId { get; set; }
        public ShoppingCart Cart { get; set; } = default!;

        // We'll add Product later; for now keep a placeholder
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // snapshot price for the item

    }
}
