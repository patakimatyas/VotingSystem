import { Navigate } from "react-router-dom";
type ProtectedRouteProps = {
    children: React.ReactNode;
};

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
    const user = localStorage.getItem("user");

    if (!user) {
        return <Navigate to="/users/login" />;
    }

    return <>{children}</>;
}