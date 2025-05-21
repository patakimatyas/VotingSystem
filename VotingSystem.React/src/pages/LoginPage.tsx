import { useState } from "react";
import { login } from "../api/client/users-client";
import type { LoginRequestDto } from "../api/models/LoginRequestDto";
import { useNavigate } from "react-router-dom";
import "../FormPage.css";

export default function LoginPage() {
    const navigate = useNavigate();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError("");

        const loginDto: LoginRequestDto = {
            email,
            password
        };

        try {
            const response = await login(loginDto);
            localStorage.setItem("authToken", response.authToken);
            localStorage.setItem("user", JSON.stringify(response));
            navigate("/polls/active");
        } catch (err) {
            setError("Incorrect email or password.");
            console.error(err);
        }
    };

    return (
        <div className="container">
            <h1>Login</h1>
            <form onSubmit={handleSubmit} className="form">
                <div className="form-group">
                    <label htmlFor="email">Email</label>
                    <input
                        id="email"
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="password">Password</label>
                    <input
                        id="password"
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>

                <p className="error">{error ?? "\u00A0"}</p>

                <button type="submit">Login</button>
            </form>
        </div>
    );
}
