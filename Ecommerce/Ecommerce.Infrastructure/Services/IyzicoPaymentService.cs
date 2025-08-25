using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;

public class IyzicoPaymentService : IPaymentService
{
    private readonly Iyzipay.Options _opt;
    private readonly IyzicoSettings _cfg;

    public IyzicoPaymentService(IOptions<IyzicoSettings> cfg)
    {
        _cfg = cfg.Value;
        _opt = new Iyzipay.Options
        {
            ApiKey = _cfg.ApiKey,
            SecretKey = _cfg.SecretKey,
            BaseUrl = _cfg.BaseUrl
        };
    }

    public async Task<(string? paymentPageUrl, string token, string? checkoutFormContent, string status, string? error)>
    StartCheckoutAsync(string conversationId, decimal totalPrice, string email, string name, string surname, string ip,
                       IEnumerable<(string id, string name, string category, decimal price, bool isPhysical)> items,
                       CancellationToken ct = default)
    {

        var req = new CreateCheckoutFormInitializeRequest
        {
            Locale = Locale.TR.ToString(),
            ConversationId = conversationId,
            Price = totalPrice.ToString("0.##", CultureInfo.InvariantCulture),
            PaidPrice = totalPrice.ToString("0.##", CultureInfo.InvariantCulture),
            Currency = Currency.TRY.ToString(),
            BasketId = conversationId,
            PaymentGroup = PaymentGroup.PRODUCT.ToString(),
            CallbackUrl = $"{_cfg.CallbackUrl}?orderId={Uri.EscapeDataString(conversationId)}",
            EnabledInstallments = new List<int> { 2, 3, 6, 9 }
        };

        var buyerId =
    string.IsNullOrWhiteSpace(email) ? conversationId : email; // use email if available, else orderId
        req.Buyer = new Iyzipay.Model.Buyer
        {
            Id = buyerId,                           // <— REQUIRED
            Name = string.IsNullOrWhiteSpace(name) ? "Name" : name,
            Surname = string.IsNullOrWhiteSpace(surname) ? "Surname" : surname,
            Email = string.IsNullOrWhiteSpace(email) ? "sandbox@example.com" : email,
            GsmNumber = "+905555555555",
            IdentityNumber = "11111111110",
            RegistrationAddress = "Test Address",
            City = "Istanbul",
            Country = "Turkey",
            ZipCode = "34000",
            Ip = string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip,
            RegistrationDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            LastLoginDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        req.ShippingAddress = new global::Iyzipay.Model.Address
        {
            ContactName = $"{name} {surname}",
            City = "Istanbul",
            Country = "Turkey",
            Description = "Test Address"
        };
        req.BillingAddress = new global::Iyzipay.Model.Address
        {
            ContactName = $"{name} {surname}",
            City = "Istanbul",
            Country = "Turkey",
            Description = "Test Address"
        };

        var basketItems = new List<BasketItem>();
        foreach (var it in items)
        {
            basketItems.Add(new BasketItem
            {
                Id = it.id,
                Name = it.name,
                Category1 = it.category,
                ItemType = it.isPhysical ? BasketItemType.PHYSICAL.ToString() : BasketItemType.VIRTUAL.ToString(),
                Price = it.price.ToString("0.##", CultureInfo.InvariantCulture)
            });
        }
        req.BasketItems = basketItems;

        CheckoutFormInitialize init = await CheckoutFormInitialize.Create(req, _opt);

        // Surface status & error so the client can show a message
        return (init.PaymentPageUrl, init.Token, init.CheckoutFormContent, init.Status, init.ErrorMessage);
    }

    public async Task<(bool success, string paymentId, string? error)> CompleteAsync(string token, string conversationId, CancellationToken ct = default)
    {
        var r = new RetrieveCheckoutFormRequest
        {
            Token = token,
            ConversationId = conversationId,
            Locale = Locale.TR.ToString()
        };

        CheckoutForm result = await CheckoutForm.Retrieve(r, _opt);

        if (result.Status == "success" && result.PaymentStatus == "SUCCESS")
            return (true, result.PaymentId, null);

        return (false, "", result.ErrorMessage ?? "payment failed");
    }


}
