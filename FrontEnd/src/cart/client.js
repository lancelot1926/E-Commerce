import api from "../api/axios";
import { getUserIdFromToken } from "../auth/token";

const GUEST_KEY = "cart_guest";

function readGuest() {
  try { return JSON.parse(localStorage.getItem(GUEST_KEY) || "[]"); }
  catch { return []; }
}
function writeGuest(items) {
  localStorage.setItem(GUEST_KEY, JSON.stringify(items));
}

function isLoggedIn() {
  return !!getUserIdFromToken();
}

// Normalize server items -> { id, name, price, qty }
function normalizeServerItems(cartDto) {
  // cartDto: { userId, items: [{ productId, name, unitPrice, quantity, lineTotal }] }
  return (cartDto.items || []).map(x => ({
    id: x.productId,
    name: x.name,
    price: x.unitPrice,
    qty: x.quantity
  }));
}

// ---------- Public API used by pages ----------

// Get current cart items
export async function cartGet() {
  if (!isLoggedIn()) return readGuest();
  const { data } = await api.get("/cart");
  return normalizeServerItems(data);
}

// Always add by first reading the current qty, then setting the new absolute qty
export async function cartAddOrUpdate(product, qtyDelta = 1) {
  // Guest mode
  if (!isLoggedIn()) {
    // guest path unchanged
    const items = readGuest();
    const i = items.findIndex(it => it.id === product.id);
    if (i >= 0) items[i].qty = Math.max(1, items[i].qty + qtyDelta);
    else items.push({ id: product.id, name: product.name, price: product.price, qty: Math.max(1, qtyDelta) });
    writeGuest(items);
    return items;
  }
  try {
    const current = await cartGet();
    const cur = current.find(i => i.id === product.id)?.qty ?? 0;
    const newQty = Math.max(1, cur + qtyDelta);
    await api.post("/cart/items", { productId: product.id, quantity: newQty });  // ABSOLUTE qty
    const { data } = await api.get("/cart");
    return normalizeServerItems(data);
  } catch (e) {
    if (e.response?.status === 401) {
      localStorage.removeItem("token");
      return cartAddOrUpdate(product, qtyDelta);
    }
    // 👇 turn 400 into a friendly error
    if (e.response?.status === 400) {
      throw new Error(e.response?.data?.error || "Could not add to cart");
    }
    throw e;
  }
}

export async function cartSetQty(productId, qty) {
  qty = Math.max(1, qty);

  if (!isLoggedIn()) {
    const items = readGuest();
    const i = items.findIndex(it => it.id === productId);
    if (i >= 0) items[i].qty = qty; else items.push({ id: productId, name: "", price: 0, qty });
    writeGuest(items);
    return items;
  }
  try {
    await api.post("/cart/items", { productId, quantity: qty });
    const { data } = await api.get("/cart");
    return normalizeServerItems(data);
  } catch (e) {
    if (e.response?.status === 401) {
      localStorage.removeItem("token");
      return cartSetQty(productId, qty);
    }
    // 👇 surface stock message on 400
    if (e.response?.status === 400) {
      throw new Error(e.response?.data?.error || "Could not update quantity");
    }
    throw e;
  }
}

export async function cartRemove(productId) {
  if (!isLoggedIn()) {
    const items = readGuest().filter(i => i.id !== productId);
    writeGuest(items);
    return items;
  }
  await api.delete(`/cart/items/${productId}`);
  const { data } = await api.get("/cart");
  return normalizeServerItems(data);
}

export async function cartClear() {
  if (!isLoggedIn()) { writeGuest([]); return []; }
  await api.delete("/cart");
  return [];
}

// Merge guest cart into server cart after login
export async function cartMergeGuestIntoServer() {
  if (!isLoggedIn()) return;
  const guest = readGuest();
  if (!guest.length) return;
  // push each guest item as an AddOrUpdate delta
  await Promise.all(guest.map(g =>
    api.post("/cart/items", { productId: g.id, quantity: g.qty })
  ));
  writeGuest([]); // clear guest
}
