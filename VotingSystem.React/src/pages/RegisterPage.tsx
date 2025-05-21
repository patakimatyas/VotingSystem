// src/pages/RegisterPage.tsx
import { useState } from "react";
import { createUser } from "../api/client/users-client";
import type { UserRequestDto } from "../api/models/UserRequestDto";
import { useNavigate } from "react-router-dom";
import '../FormPage.css';

export default function RegisterPage() {
    const navigate = useNavigate();
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError("");

        const user: UserRequestDto = {
            name,
            email,
            password
        };

        try {
            await createUser(user);
            navigate("/users/login");
        } catch (err: any) {
            if (err instanceof Error) {
                setError(err.message);
            } else {
                setError("Unknown Error :( ");
            }
        }
    };

    return (
        <div className="container">
            <h1>Register</h1>
            <form onSubmit={handleSubmit} className="form">
                <div className="form-group">
                    <label htmlFor="name">Name</label>
                    <input
                        id="name"
                        type="text"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        required
                    />
                </div>

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

                <button type="submit">Register</button>
            </form>
        </div>
    );


}
