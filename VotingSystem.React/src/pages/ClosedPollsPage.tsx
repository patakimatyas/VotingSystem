import { useEffect, useState } from "react";
import { getClosedPolls } from "../api/client/polls-client";
import type { PollResponseDto } from "../api/models/PollResponseDto";
import { Link } from "react-router-dom";
import "../ClosedPollsPage.css";


export default function ClosedPollsPage() {
    const [polls, setPolls] = useState<PollResponseDto[]>([]);
    const [text, setText] = useState("");
    const [from, setFrom] = useState("");
    const [to, setTo] = useState("");

    const fetchPolls = () => {
        const params: Record<string, string> = {};
        if (text) params.text = text;
        if (from) params.from = from;
        if (to) params.to = to;

        getClosedPolls(params)
            .then(setPolls)
            .catch(console.error);
    };

    useEffect(() => {
        fetchPolls();
    }, []);

    return (
        <div className="closed-polls-page">
            <h1>Closed Polls</h1>

            <div className="filters">
                <div className="filter-row">
                    <input
                        type="text"
                        placeholder="Search question..."
                        value={text}
                        onChange={e => setText(e.target.value)}
                        className="text-filter"
                    />
                </div>
                <div className="filter-row">
                    <label>
                        Start date
                        <input type="date" value={from} onChange={e => setFrom(e.target.value)} />
                    </label>
                    <label>
                        End date
                        <input type="date" value={to} onChange={e => setTo(e.target.value)} />
                    </label>
                </div>
                <button onClick={fetchPolls}>Filter</button>
            </div>  

            <ul className="poll-list">
                {polls.map(poll => (
                    <li key={poll.id} className="poll-card-closed">
                        <Link to={`/polls/closed/${poll.id}`} className="poll-link">
                            <h3>{poll.question}</h3>
                            <p>Start: {new Date(poll.startDate).toLocaleDateString()}</p>
                            <p>End: {new Date(poll.endDate).toLocaleDateString()}</p>
                        </Link>
                    </li>
                ))}
            </ul>
        </div>
    );
}
