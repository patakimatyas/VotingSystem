import '../HomePage.css';
import { useNavigate } from "react-router-dom";

export default function HomePage(){
    const navigate = useNavigate();
    const user = localStorage.getItem("user");

    return(
        <div className="home-page">
            <h1>Welcome to VotingSystem</h1>
            <p>Vote on open polls or check out the outcome of closed ones</p>
            <div className="admin-section">
            <p>Navigate to the admin app if you want to create polls too</p> 
            <button onClick={() => window.location.href = 'https://localhost:7044'} className="admin-page-btn">
                VotingSystem.Admin
            </button>
            </div>
            <div className='home-btn-group'>
            {!user ? (
                <><button onClick={() => navigate("/users/login")}>Login</button>
                <button onClick={() => navigate("/users/register")}>Register</button></>
            ):(
                <><button onClick={() => navigate("/polls/active")}>Active</button>
                <button onClick={() => navigate("/polls/closed")}>Closed</button></>
            )}
            </div>
        </div>
    );
}2