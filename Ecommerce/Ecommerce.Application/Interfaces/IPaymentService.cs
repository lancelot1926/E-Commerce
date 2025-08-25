public interface IPaymentService
{
    Task<(string? paymentPageUrl, string token, string? checkoutFormContent, string status, string? error)>
        StartCheckoutAsync(
            string conversationId,
            decimal totalPrice,
            string buyerEmail,
            string buyerName,
            string buyerSurname,
            string ip,
            IEnumerable<(string id, string name, string category, decimal price, bool isPhysical)> items,
            CancellationToken ct = default);

    Task<(bool success, string paymentId, string? error)> CompleteAsync(string token, string conversationId, CancellationToken ct = default);
}
