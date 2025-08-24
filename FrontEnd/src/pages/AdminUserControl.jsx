import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { listUsersAdmin,handleBan } from "../users/client";

export default function AdminUserControl() {
  const [users, setUsers] = useState([]);

  const fetchUsers = async () => {
    const data = await listUsersAdmin();
    setUsers(data);
  };

  useEffect(() => { fetchUsers(); }, []);

  const onBanClick = async (u) => {
    try {
      const newValue = !u.isBanned;           // toggle
      await handleBan(u.id, newValue);        // API call
      setUsers(prev =>
        prev.map(x => x.id === u.id ? { ...x, isBanned: newValue } : x)
      );                                      // update row locally
    } catch (e) {
      console.error(e.response?.data || e.message);
    }
  };

  return (
    <div>
      <h1>Admin User Panel</h1>
      <table className="table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Is Banned</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.id}>
              <td>
                <Link to={`/users/${user.id}`}>{user.name}</Link>
              </td>
              <td>{user.email}</td>
              <td>{user.isBanned ? "Yes" : "No"}</td>
              <td>
                <Link to={`/admin/users/${user.id}/edit`} className="btn btn-sm btn-warning">Edit</Link>
                <button
                  className="btn btn-sm btn-danger"
                  onClick={() => onBanClick(user)}
                >
                  Ban
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}