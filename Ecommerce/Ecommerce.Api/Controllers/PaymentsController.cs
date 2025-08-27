using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

        const string feOrigin = "http://localhost:3000";   // your SPA origin

        var payloadJson = JsonSerializer.Serialize(new
        {
            type = "iyzico",
            ok,
            orderId = id,
            paymentId,
            reason = ok ? null : (err ?? "unknown")
        });

        var title = ok ? "Payment succeeded" : "Payment failed";
        var icon = ok ? "success" : "error";
        var text = ok ? "" : (err ?? "Unknown error");
        var redirectUrl = ok ? $"{feOrigin}/orders/{id}" : $"{feOrigin}/cart";
        if (ok)
        {
            await _orders.FinalizeOrderAfterPaymentAsync(id, paymentId);
        }
        else
        {
            await _orders.FailOrderAsync(id);
        }

        var html = $@"<!doctype html><html><head>
  <meta charset='utf-8'/>
  <script src='https://cdn.jsdelivr.net/npm/sweetalert2@11'></script>
</head><body>
<script>
(function () {{
  var data = {payloadJson};
  var fe   = '{feOrigin}';
  var sent = false;
  try {{
    // Prefer posting to opener/parent only (NOT window.top). Also ensure it's not ourselves.
    var target = (window.opener && window.opener !== window) ? window.opener
               : (window.parent  && window.parent  !== window) ? window.parent
               : null;
    if (target) {{
      target.postMessage(data, fe);
      sent = true;     // <-- IMPORTANT: prevent fallback navigation inside the iframe
    }}
  }} catch(e) {{ }}

  if (!sent) {{
    // Top-window fallback (bank navigated us here)
    var title = {JsonSerializer.Serialize(title)};
    var text  = {JsonSerializer.Serialize(text)};
    var icon  = {JsonSerializer.Serialize(icon)};
    var redirect = {JsonSerializer.Serialize(redirectUrl)};
    if (window.Swal) {{
      Swal.fire({{ title: title, text: text, icon: icon }})
          .then(function () {{ window.location.replace(redirect); }});
    }} else {{
      window.location.replace(redirect);
    }}
  }}
}})();
</script>
</body></html>";

        return Content(html, "text/html");
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
