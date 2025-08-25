using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/payments/iyzico")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _pay;
    private readonly IOrderService _orders;
    

    public PaymentsController(IPaymentService pay, IOrderService orders)
    {
        _pay = pay;
        _orders = orders;
    }

    // 1) Start checkout: client posts orderId, we reply with iyzico payment page URL to redirect
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartPaymentDto dto)
    {
        var items = dto.Items.Select((x, i) => (id: $"BI{i + 1}", name: x.Name, category: x.Category ?? "General", price: x.Price, isPhysical: true));
        var (url, token, html, status, err) = await _pay.StartCheckoutAsync(
            conversationId: dto.OrderId.ToString(),
            totalPrice: dto.TotalPrice,
            buyerEmail: dto.Email, buyerName: dto.Name, buyerSurname: dto.Surname,
            ip: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            items: items);

        if (!string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = err ?? "Iyzico initialization failed." });

        // Return both; client will prefer url and fall back to html.
        return Ok(new
        {
            paymentPageUrl = url,
            token,
            checkoutFormContent = html,
            status
        });
    }

    // 2) Callback: iyzico posts { token } to this URL after the customer completes the payment page
    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromForm] string token, [FromQuery] string orderId)
    {
        var (ok, paymentId, err) = await _pay.CompleteAsync(token, orderId ?? "unknown");

        int id = 0; int.TryParse(orderId, out id);

        if (ok)
        {
            // finalize: decrement stock + clear cart + mark Paid
            await _orders.FinalizeOrderAfterPaymentAsync(id, paymentId);
            var redirect = $"http://localhost:3000/checkout/result?status=success&orderId={orderId}";
            var html = $"<html><head><meta http-equiv='refresh' content='0;url={redirect}' /></head><body>Redirecting…</body></html>";
            return Content(html, "text/html");
        }
        else
        {
            // mark Failed; leave cart alone
            await _orders.FailOrderAsync(id);
            var redirect = $"http://localhost:3000/checkout/result?status=failed&orderId={orderId}&reason={Uri.EscapeDataString(err ?? "unknown")}";
            var html = $"<html><head><meta http-equiv='refresh' content='0;url={redirect}' /></head><body>Redirecting…</body></html>";
            return Content(html, "text/html");
        }
    }

    public sealed class StartPaymentDto
    {
        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public List<StartItem> Items { get; set; } = new();
    }
    public sealed class StartItem { public string Name { get; set; } = ""; public string? Category { get; set; } public decimal Price { get; set; } }
}
