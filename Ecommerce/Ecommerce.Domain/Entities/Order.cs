using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public enum OrderStatus { Pending = 0, Paid = 1, Cancelled = 2 }

public class Order : BaseEntity
{
    public int UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public User User { get; set; } = default!;
    public string? PaymentId { get; set; }
}

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;   // <— add this
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty; // snapshot
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

