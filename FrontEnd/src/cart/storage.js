import { getUserIdFromToken } from "../auth/token";

const GUEST_KEY = "cart_guest";
const USER_KEY = (userId) => `cart_user_${userId}`;

function read(key) {
  try { return JSON.parse(localStorage.getItem(key) || "[]"); }
  catch { return []; }
}

function write(key, items) {
  localStorage.setItem(key, JSON.stringify(items));
}

export function getActiveCartKey() {
  const uid = getUserIdFromToken();
  return uid ? USER_KEY(uid) : GUEST_KEY;
}

export function getCart() {
  return read(getActiveCartKey());
}

export function setCart(items) {
  write(getActiveCartKey(), items);
}

export function addToCartItem(product, qty = 1) {
  const key = getActiveCartKey();
  const items = read(key);
  const i = items.findIndex(x => x.id === product.id);
  if (i >= 0) items[i].qty += qty;
  else items.push({ id: product.id, name: product.name, price: product.price, qty });
  write(key, items);
}

export function updateQty(id, qty) {
  const key = getActiveCartKey();
  let items = read(key);
  items = items.map(i => i.id === id ? { ...i, qty: Math.max(1, qty) } : i);
  write(key, items);
}

export function removeItem(id) {
  const key = getActiveCartKey();
  const items = read(key).filter(i => i.id !== id);
  write(key, items);
}

export function clearCart() {
  write(getActiveCartKey(), []);
}

/** Merge guest cart into the logged-in user's cart once (call after successful login) */
export function mergeGuestCartIntoUser() {
  const uid = getUserIdFromToken();
  if (!uid) return;
  const guest = read(GUEST_KEY);
  if (guest.length === 0) return;
  const userKey = USER_KEY(uid);
  const userItems = read(userKey);

  const map = new Map();
  [...userItems, ...guest].forEach(i => {
    const prev = map.get(i.id);
    map.set(i.id, prev ? { ...i, qty: prev.qty + i.qty } : i);
  });

  write(userKey, Array.from(map.values()));
  write(GUEST_KEY, []); // clear guest
}
