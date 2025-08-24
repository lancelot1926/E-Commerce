import { useEffect, useState } from "react";
import { Link,useParams } from "react-router-dom";
import { userDetails } from "../users/client";

export default function UserProfilePage() {
  const [user, setUser] = useState(null);
  const { id } = useParams();

  useEffect(() => {
    const fetchUser = async () => {
      const userData = await userDetails(id);
      setUser(userData);
    };
    fetchUser();
  }, [id]);

 

  if (!user) return <div>Loading...</div>;

  return (
    <div>
      <h1>{user.name}</h1>
      <p>Email: {user.email}</p>
      <p>Phone Number: {user.phoneNumber}</p>
      <p>Address: {user.addressLine1}</p>      
      <p>Joined: {new Date(user.createdAt).toLocaleDateString()}</p>
      <p>IsBanned? {user.isBanned ? "Yes" : "No"}</p>

      <Link to={`/admin/users/${user.id}/edit`} className="btn btn-warning">Edit</Link>
    </div>
  );
}