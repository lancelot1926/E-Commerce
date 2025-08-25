// src/payments/client.js
export async function startIyzicoPayment(params) {
  // params = { orderId, totalPrice, email, name, surname, items: [{Name, Category?, Price}] }
  const res = await fetch("https://localhost:55198/api/payments/iyzico/start", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      OrderId: params.orderId,
      TotalPrice: Number(params.totalPrice.toFixed(2)),
      Email: params.email,
      Name: params.name,
      Surname: params.surname,
      Items: params.items,
    }),
  });

  if (!res.ok) {
    let errText = `Payment start failed (${res.status})`;
    try { const j = await res.json(); if (j && j.error) errText = j.error; } catch {}
    throw new Error(errText);
  }

  return await res.json(); // { paymentPageUrl, token }
}
