import { Link, useNavigate } from "react-router-dom";
import { logout } from "../api/client/users-client";
import "../Navbar.css";

export default function Navbar() {
    const navigate = useNavigate();
    const user = localStorage.getItem("user");

    const handleLogout = async () => {
        try {
            await logout();
            localStorage.removeItem("user");
            localStorage.removeItem("authToken");
            localStorage.removeItem("votedPolls");
            navigate("/users/login");
        } catch (err) {
            console.error("Logout failed:", err);
        }
    };

    return (
        <nav className="navbar">

            <div className="nav-left">
            <Link to="/polls/active" className="nav-link">Active</Link>
            <Link to="/polls/closed" className="nav-link">Closed</Link>
            </div>
            <div className="nav-right">
                {user ? (
                    <button className="logout-button" onClick={handleLogout}>Logout</button>
                ) : (
                    <>
                        <Link to="/users/login" className="nav-link">Login</Link>
                        <Link to="/users/register" className="nav-link">Register</Link>
                    </>
                )}
            </div>
        </nav>
    );
}
