import { BrowserRouter, Routes, Route, Link } from "react-router-dom";
import 'bootstrap/dist/css/bootstrap.min.css';

import Home from "./pages/Home";
import ProductDetails from "./pages/ProductDetails";
import Cart from "./pages/Cart";
import Login from "./pages/Login";
import Register from "./pages/Register";
import UserDetails from "./pages/UserProfilePage";
import AdminProducts from "./pages/AdminProducts";
import AdminEditProduct from "./pages/AdminEditProduct";
import AdminCreateProduct from "./pages/AdminCreateProduct";
import Orders from "./pages/Orders";
import OrderDetails from "./pages/OrderDetails";
import AdminOrders from "./pages/AdminOrders";
import AdminUserControl from "./pages/AdminUserControl";
import ProtectedRoute from "./components/ProtectedRoute";
import { isAdmin as isAdminFromToken } from "./auth/token";
import { useEffect } from "react";
import { startUserHub, stopUserHub} from "./realtime/userHub";
import { useAuth } from "./auth/AuthContext";


function NavBar() {
  const { isLoggedIn, logout } = useAuth();
  const showAdmin = isLoggedIn && isAdminFromToken();
  // add link when logged in:
{isLoggedIn && <Link className="btn btn-outline-secondary me-2" to="/orders">Orders</Link>}
  return (
    <nav className="navbar navbar-expand-lg bg-light">
      <div className="container">
        <Link className="navbar-brand" to="/">Ecommerce</Link>
        <div className="ms-auto">
          <Link className="btn btn-outline-secondary me-2" to="/cart">Cart</Link>
          {isLoggedIn && (
            <Link to="/orders" className="btn btn-outline-secondary ms-2"> Orders </Link>
            )}
          {showAdmin && (
             <>
    <Link className="btn btn-outline-dark me-2" to="/admin/products">Admin Products</Link>
    <Link className="btn btn-outline-dark me-2" to="/admin/orders">Admin Orders</Link>
    <Link className="btn btn-outline-dark me-2" to="/admin/users">Admin Users</Link>
  </>
          )}
          {showAdmin && (
            <Link className="btn btn-outline-dark me-2" to="/admin/products">Admin</Link>
          )}
          {!isLoggedIn ? (
            <>
              <Link className="btn btn-outline-primary me-2" to="/login">Login</Link>
              <Link className="btn btn-primary" to="/register">Register</Link>
            </>
          ) : (
            <button className="btn btn-outline-danger" onClick={logout}>Logout</button>
          )}
        </div>
      </div>
    </nav>
  );
}
export default function App() {
    const { isLoggedIn } = useAuth();

  useEffect(() => {
    if (isLoggedIn) {
      startUserHub();          // opens SignalR connection with current JWT
    } else{
      stopUserHub();       // closes SignalR connection
    }
  }, [isLoggedIn]);
  return (
    <BrowserRouter>
      <NavBar />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/product/:id" element={<ProductDetails />} />
        <Route path="/cart" element={<Cart />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/users/:id" element={<UserDetails />} />
         {/* Admin routes */}
        <Route path="/admin/products" element={
          <ProtectedRoute requireAdmin>
            <AdminProducts />
          </ProtectedRoute>
        }/>
        <Route path="/admin/products/create" element={
          <ProtectedRoute requireAdmin>
            <AdminCreateProduct />
          </ProtectedRoute>
        }/>
        <Route path="/admin/products/:id/edit" element={
          <ProtectedRoute requireAdmin>
            <AdminEditProduct />
          </ProtectedRoute>
        }/>
        <Route path="/admin/orders" element={
          <ProtectedRoute requireAdmin>
            <AdminOrders />
          </ProtectedRoute>
        }/>
        <Route path="/orders" element={
          <ProtectedRoute>
            <Orders />
          </ProtectedRoute>
        }/>
        <Route path="/orders/:id" element={
          <ProtectedRoute>
            <OrderDetails />
          </ProtectedRoute>
        }/>
        <Route path="/admin/users" element={
          <ProtectedRoute requireAdmin>
            <AdminUserControl />
          </ProtectedRoute>
        }/>
      </Routes>
    </BrowserRouter>
  );
}