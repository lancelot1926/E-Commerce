import { useEffect, useState, useRef } from "react";
import Swal from "sweetalert2";
import { cartGet, cartSetQty, cartRemove, cartClear, cartAddOrUpdate } from "../cart/client";
import { checkout } from "../orders/client";
import { startIyzicoPayment } from "../payments/client";
import { getBuyerForPayment } from "../users/client";

export default function Cart() {
  // cart
  const [items, setItems] = useState([]);

  // payment modal + iyzico embed
  const [payOpen, setPayOpen] = useState(false);
  const [modalKey, setModalKey] = useState(0);        // force fresh mount each time
  const [currentOrderId, setCurrentOrderId] = useState(null);
  const [formContent, setFormContent] = useState(""); // iyzico checkoutFormContent snippet

  // we host iyzico inside our own iframe to isolate globals (fixes "second attempt" bug)
  const embedFrameRef = useRef(null);

  // ---- data ----
  const refresh = async () => setItems(await cartGet());
  useEffect(() => { refresh(); }, []);

  // ---- receive result from callback (iframe/popup OR top-window fallback) ----
  useEffect(() => {
    function onMessage(ev) {
      // IMPORTANT: match your API origin exactly (http vs https + port)
      if (ev.origin !== "https://localhost:55198") return;

      const data = ev.data || {};
      if (data.type !== "iyzico") return;

      closePaymentModal();

      if (data.ok) {
        Swal.fire("Payment succeeded", "", "success").then(() => {
          window.location.href = `/orders/${data.orderId}`;
        });
      } else {
        Swal.fire("Payment failed", data.reason || "Unknown error", "error");
      }
    }
    window.addEventListener("message", onMessage);
    return () => window.removeEventListener("message", onMessage);
  }, []);

  // ---- mount iyzico into our iframe when modal opens ----
  useEffect(() => {
    if (payOpen && formContent) {
      writeSnippetIntoOwnedIframe(formContent);
    }
  }, [payOpen, formContent]);

  // write iyzico snippet into a brand-new, owned iframe
  function writeSnippetIntoOwnedIframe(html) {
    const iframe = embedFrameRef.current;
    if (!iframe) return;

    const js = String(html || "").replace(/<\/?script[^>]*>/gi, ""); // extract the JS inside <script>...</script>

    const doc = iframe.contentWindow?.document;
    if (!doc) return;

    // simple document with the host div iyzico expects + the snippet JS
    doc.open();
    doc.write(`<!doctype html><html><head><meta charset="utf-8"></head>
<body>
  <div id="iyzipay-checkout-form" class="responsive"></div>
  <script type="text/javascript">
    ${js}
  </script>
</body></html>`);
    doc.close();
  }

  // open modal for a new attempt (cleanly)
  function openPaymentModal(snippet) {
    setModalKey(k => k + 1);    // force re-mount of the modal subtree (fresh iframe)
    setFormContent(snippet);
    setPayOpen(true);
  }

  // close modal + hard reset iframe
  function closePaymentModal() {
    setPayOpen(false);
    // reset the iframe completely (clear its browsing context)
    const iframe = embedFrameRef.current;
    if (iframe) {
      try { iframe.src = "about:blank"; } catch {}
    }
  }

  // ---- cart helpers ----
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
  const clear  = async () => { await cartClear(); await refresh(); };

  const total = items.reduce((s, i) => s + i.price * i.qty, 0);

  // ---- main checkout flow ----
  const doCheckout = async () => {
    try {
      // 1) create order (Pending/OnHold; do NOT clear cart here)
      const order = await checkout();
      setCurrentOrderId(order.id);

      // 2) basket lines for iyzico (line total = price * qty; sum must equal total)
      const startItems = items.map(i => ({
        Name: i.name,
        Category: i.category || "General",
        Price: Number((i.price * i.qty).toFixed(2)),
      }));

      // 3) buyer info
      const buyer = await getBuyerForPayment();

      // 4) init iyzico (server returns status + checkoutFormContent)
      const resp = await startIyzicoPayment({
        orderId: order.id,
        totalPrice: Number(total.toFixed(2)),
        email: buyer.email,
        name: buyer.name,
        surname: buyer.surname,
        items: startItems,
      });

      if (resp.status !== "success" || !resp.checkoutFormContent) {
        throw new Error(resp?.error || "Iyzico init failed");
      }

      // 5) open modal and render form inside our owned iframe
      openPaymentModal(resp.checkoutFormContent);

    } catch (e) {
      alert(e?.response?.data?.error || e?.message || "Checkout failed");
    }
  };

  // ---- render ----
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

      {/* Payment modal */}
      {payOpen && (
        <div
          key={modalKey}
          style={{
            position: "fixed", inset: 0, background: "rgba(0,0,0,.5)",
            display: "flex", alignItems: "center", justifyContent: "center", zIndex: 2000
          }}
        >
          <div
            style={{
              background: "#fff", width: "min(520px, 96vw)", borderRadius: 12,
              boxShadow: "0 10px 30px rgba(0,0,0,.2)", overflow: "hidden"
            }}
          >
            <div style={{
              padding: "12px 16px", borderBottom: "1px solid #eee",
              display: "flex", justifyContent: "space-between", alignItems: "center"
            }}>
              <strong>Secure Payment</strong>
              <button
                className="btn btn-sm btn-outline-secondary"
                onClick={closePaymentModal}
              >
                Close
              </button>
            </div>

            <div style={{ padding: 0 }}>
              {/* Our clean sandbox for iyzico */}
              <iframe
                ref={embedFrameRef}
                title="Iyzico Checkout"
                style={{ width: "100%", height: 720, border: 0, display: "block" }}
                src="about:blank"
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}