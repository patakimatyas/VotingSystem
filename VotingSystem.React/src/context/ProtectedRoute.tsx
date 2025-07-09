import { Navigate } from "react-router-dom";
import { toast } from 'react-toastify';
import { useEffect } from "react";


type ProtectedRouteProps = {
    children: React.ReactNode;
};

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
    const user = localStorage.getItem("user");

    if (!user) {
        return <RedirectWithToast />;
    }

    return <>{children}</>;
}

function RedirectWithToast() {
  useEffect(() => {
    toast.info("You are not authorized to view this page. Please log in!");
  }, []);

  return <Navigate to="/users/login" />;
}