import { useEffect, useState } from "react";
import { getActivePolls } from "../api/client/polls-client";
import type { PollResponseDto } from "../api/models/PollResponseDto";
import { Link} from "react-router-dom";
import '../HomePage.css';


export default function HomePage() {
    const [polls, setPolls] = useState<PollResponseDto[]>([]);

    useEffect(() => {
        getActivePolls()
            .then(polls => {
                // console.log(polls);
                setPolls(polls);
            })
            .catch(console.error);
    }, []);

    return (
        <div className="active-polls-page">
            <h1>Active polls</h1>
            <ul>
                {polls.map((poll) => (
                    <li key={poll.id} className="active-cards">
                        <Link to={`/polls/${poll.id}`} className="poll-link">
                            <h3>
                                {poll.question}
                                {poll.hasVoted && <div className="votedmsg"> ✅ Voted</div>}
                            </h3>
                            <p>Start date: {new Date(poll.startDate).toLocaleDateString()}</p>
                            <p>End date: {new Date(poll.endDate).toLocaleDateString()}</p>
                        </Link>
                    </li>
                ))}
            </ul>
        </div>
    );
}
