using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs.Users;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
    public UserDto Customer { get; set; }
}

