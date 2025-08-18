using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Cart : BaseEntity
{
    public int UserId { get; set; }
    public List<CartItem> Items { get; set; } = new();
}

public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // snapshot at add
}
