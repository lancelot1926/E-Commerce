import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { listOrdersAdmin,setOrderStatus } from "../orders/client";

export default function AdminOrders() {
  const [orders, setOrders] = useState([]);
  const [q, setQ] = useState("");
  const [err, setErr] = useState("");
  const [saving, setSaving] = useState(new Set());

  useEffect(() => {
    listOrdersAdmin()
      .then(setOrders)
      .catch(e => setErr(e.response?.data?.error || e.message));
  }, []);

  const filtered = orders.filter(o =>
    q ? String(o.id).includes(q) || String(o.userId).includes(q) : true
  );

  const setStatDet=(s)=>{
    switch (Number(s)) {
    case 1: return "Paid";
    case 2: return "Cancelled";
    case 0:
    default: return "OnHold";
  }
  }

  const STATUS_OPTIONS = [
  { value: 0, label: "On hold" },
  { value: 1, label: "Paid" },
  { value: 2, label: "Cancelled" },
];

const statusLabel = s =>
  STATUS_OPTIONS.find(x => x.value === Number(s))?.label ?? "Unknown";

  const onChangeStatus = async (orderId, newValue) => {
  // optimistic UI
  setOrders(prev =>
    prev.map(o => (o.id === orderId ? { ...o, status: newValue } : o))
  );

  const prevSaving = new Set(saving);
  prevSaving.add(orderId);
  setSaving(prevSaving);

  try {
    // IMPORTANT: make sure your backend route & body name match!
    // If your controller is PUT /orders/{id}/state with body { state: int }
    // then call: await setOrderStatus(orderId, newValue); and have the client send { state: newValue }
    await setOrderStatus(orderId, newValue);
  } catch (e) {
    // revert on error
    setOrders(prev =>
      prev.map(o => (o.id === orderId ? { ...o, status: o.status /* revert if you stored old value */ } : o))
    );
    setErr(e.response?.data?.error || e.message);
  } finally {
    const next = new Set(saving);
    next.delete(orderId);
    setSaving(next);
  }
};

  const totalOf = (o) => o.items?.reduce((s, i) => s + i.unitPrice * i.quantity, 0) ?? o.total ?? 0;

  return (
    <div className="container py-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="m-0">Admin • All Orders</h1>
        <input
          className="form-control" style={{maxWidth: 260}}
          placeholder="Search by Order ID or User ID"
          value={q} onChange={e => setQ(e.target.value)}
        />
      </div>

      {err && <div className="alert alert-danger">{err}</div>}

      <div className="table-responsive">
        <table className="table align-middle">
          <thead>
            <tr>
              <th>Order #</th>
              <th>User ID</th>
              <th>Status</th>
              <th>Items</th>
              <th>Total</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {filtered.map(o => (
              <tr key={o.id}>
                <td>{o.id}</td>
                <td>{o.userId}</td>
                <td style={{ minWidth: 160 }}>
  <select
    className="form-select form-select-sm"
    value={Number(o.status)}
    onChange={e => onChangeStatus(o.id, Number(e.target.value))}
    disabled={saving.has(o.id)}
  >
    {STATUS_OPTIONS.map(opt => (
      <option key={opt.value} value={opt.value}>{opt.label}</option>
    ))}
  </select>
  <div className="small text-muted mt-1">Now: {statusLabel(o.status)}</div>
</td>
                <td>{o.items?.length ?? 0}</td>
                <td>₺{totalOf(o).toFixed(2)}</td>
                <td className="text-end">
                  <Link to={`/orders/${o.id}`} className="btn btn-outline-secondary btn-sm">
                    View
                  </Link>
                </td>
              </tr>
            ))}
            {filtered.length === 0 && (
              <tr><td colSpan="6" className="text-center text-muted py-4">No orders found.</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
