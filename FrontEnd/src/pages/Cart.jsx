import { useEffect, useState } from "react";
import { cartGet, cartSetQty, cartRemove, cartClear, cartAddOrUpdate } from "../cart/client";

export default function Cart() {
  const [items, setItems] = useState([]);

  const refresh = async () => setItems(await cartGet());

  useEffect(() => { refresh(); }, []);

  const inc = async (id) => {
    const p = items.find(i => i.id === id);
    await cartAddOrUpdate({ id, name: p.name, price: p.price }, 1);
    await refresh();
  };
  const dec = async (id) => {
    const p = items.find(i => i.id === id);
    if (p.qty > 1) {
      await cartAddOrUpdate({ id, name: p.name, price: p.price }, -1);
      await refresh();
    }
  };
  const setQty = async (id, q) => { await cartSetQty(id, q); await refresh(); };
  const remove = async (id) => { await cartRemove(id); await refresh(); };
  const clear = async () => { await cartClear(); await refresh(); };

  const total = items.reduce((s, i) => s + i.price * i.qty, 0);

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
              className="form-control form-control-sm me-2" style={{width: 70}}
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
            <button className="btn btn-success" disabled>Checkout (coming soon)</button>
          </div>
        </div>
      )}
    </div>
  );
}
