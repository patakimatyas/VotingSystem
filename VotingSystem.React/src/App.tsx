import ActivePollsPage from "./pages/ActivePollsPage";
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import PollDetailsPage from "./pages/PollDetailsPage";
import ClosedPollsPage from "./pages/ClosedPollsPage";
import PollResultPage from "./pages/PollResultPage";
import RegisterPage from "./pages/RegisterPage";
import LoginPage from "./pages/LoginPage";
import ProtectedRoute from "./context/ProtectedRoute";
import Navbar from "./components/Navbar";
import HomePage from "./pages/HomePage";
import {ToastContainer} from "react-toastify";
import 'react-toastify/dist/ReactToastify.css';


function App() {
    return (
        <BrowserRouter>
            <Navbar/>
            <Routes>
                <Route path="/" element={<HomePage/>}/>
                <Route path="/polls/active" element={<ProtectedRoute><ActivePollsPage /></ProtectedRoute>} />
                <Route path="/polls/closed" element={<ProtectedRoute><ClosedPollsPage /></ProtectedRoute>} />
                <Route path="/polls/:id" element={<ProtectedRoute><PollDetailsPage /></ProtectedRoute>} />
                <Route path="/polls/closed/:id" element={<ProtectedRoute><PollResultPage /></ProtectedRoute>} />
                <Route path="/users/register" element={<RegisterPage />} />
                <Route path="/users/login" element={<LoginPage />} />
            </Routes>
            <ToastContainer position="bottom-right" autoClose={2500} />
        </BrowserRouter>
    );
}

export default App;
