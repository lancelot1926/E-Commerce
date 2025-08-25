import { useEffect, useState } from "react";
import { cartGet, cartSetQty, cartRemove, cartClear, cartAddOrUpdate } from "../cart/client";
import { checkout } from "../orders/client";
import { startIyzicoPayment } from "../payments/client";  // <-- new
import { getBuyerForPayment } from "../users/client";     // <-- optional helper above

export default function Cart() {
  const [items, setItems] = useState([]);

  const refresh = async () => setItems(await cartGet());
  useEffect(() => { refresh(); }, []);

  const inc = async (id) => {
    const p = items.find(i => i.id === id); if (!p) return;
    try { await cartAddOrUpdate({ id, name: p.name, price: p.price }, 1); }
    catch (err) {
      const msg = err?.response?.data?.error || err?.message || "Could not increase quantity";
      alert(msg);
    }
    await refresh();
  };

  const dec = async (id) => {
    const p = items.find(i => i.id === id); if (!p) return;
    if (p.qty > 1) {
      try { await cartAddOrUpdate({ id, name: p.name, price: p.price }, -1); }
      catch (err) {
        const msg = err?.response?.data?.error || err?.message || "Could not decrease quantity";
        alert(msg);
      }
    }
    await refresh();
  };

  const setQty = async (id, q) => { await cartSetQty(id, q); await refresh(); };
  const remove = async (id) => { await cartRemove(id); await refresh(); };
  const clear = async () => { await cartClear(); await refresh(); };

  const total = items.reduce((s, i) => s + i.price * i.qty, 0);

  const doCheckout = async () => {
  try {
    // 1) create order (DO NOT clear cart on server here)
    const order = await checkout();

    // 2) build basket lines as (price * qty)
    const startItems = items.map(i => ({
      Name: i.name,
      Category: i.category || "General",
      Price: Number((i.price * i.qty).toFixed(2)),
    }));

    // 3) buyer
    const buyer = await getBuyerForPayment();

    // 4) start payment (make sure this calls https://localhost:55198/… or use a proxy)
    const { paymentPageUrl, checkoutFormContent, status } = await startIyzicoPayment({
      orderId: order.id,
      totalPrice: Number(total.toFixed(2)),
      email: buyer.email,
      name: buyer.name,
      surname: buyer.surname,
      items: startItems,
    });

    if (status !== "success") throw new Error("Iyzico init failed");

    // 5) prefer hosted page
    if (paymentPageUrl) {
      window.location.href = paymentPageUrl;
      return;
    }

    // 6) fall back to embedded HTML (opens new tab with the form)
    const w = window.open("", "_blank");
    if (!w) throw new Error("Popup blocked. Allow popups for this site.");
    w.document.open();
    w.document.write(checkoutFormContent || "<h3>Unable to load iyzico form</h3>");
    w.document.close();

  } catch (e) {
    alert(e?.response?.data?.error || e?.message || "Checkout failed");
  }
};

  return (
    <div className="container py-4">
      <h1 className="mb-3">Cart</h1>
      {items.length === 0 && <div>Your cart is empty.</div>}

      {items.map(i => (
        <div key={i.id} className="d-flex align-items-center justify-content-between border rounded p-2 mb-2">
          <div>
            <div className="fw-semibold">{i.name}</div>
            <div>₺{i.price}</div>
          </div>
          <div className="d-flex align-items-center">
            <button className="btn btn-outline-secondary me-2" onClick={() => dec(i.id)}>-</button>
            <input
              type="number" min="1" value={i.qty}
              onChange={e => setQty(i.id, Number(e.target.value))}
              className="form-control form-control-sm me-2" style={{ width: 70 }}
            />
            <button className="btn btn-outline-secondary me-3" onClick={() => inc(i.id)}>+</button>
            <button className="btn btn-outline-danger" onClick={() => remove(i.id)}>Remove</button>
          </div>
        </div>
      ))}

      {items.length > 0 && (
        <div className="mt-3 d-flex justify-content-between align-items-center">
          <h4 className="m-0">Total: ₺{total.toFixed(2)}</h4>
          <div>
            <button className="btn btn-outline-danger me-2" onClick={clear}>Clear</button>
            <button className="btn btn-success" onClick={doCheckout} disabled={!items.length}>Checkout</button>
          </div>
        </div>
      )}
    </div>
  );
}
