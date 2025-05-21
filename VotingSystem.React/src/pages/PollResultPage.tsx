import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getClosedPollResult } from "../api/client/polls-client";
import type { ClosedPollResponseDto } from "../api/models/ClosedPollResponseDto";

import "../PollResultPage.css";

export default function PollResultPage() {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const [poll, setPoll] = useState<ClosedPollResponseDto | null>(null);

    useEffect(() => {
        if (id) {
            getClosedPollResult(parseInt(id))
                .then(setPoll)
                .catch(console.error);
        }
    }, [id]);

    if (!poll) return <p>Loading...</p>;

    return (
        <div className="container">
            <h1>{poll.question}</h1>

            <div className="results">
                {poll.options.map((option) => (
                    <div key={option.id} className="result-option">
                        <div className="option-text">{option.text}</div>
                        <div className="vote-count">{option.voteCount} votes</div>
                        <div className="progress-container">
                            <div className="progress-bar">
                                <div className="progress-fill" style={{ width: `${option.votePercentage}%` }}></div>
                            </div>
                            <div className="percentage-text">{option.votePercentage.toFixed(1)}%</div>
                        </div>

                    </div>
                ))}
            </div>

            <button onClick={() => navigate("/polls/closed")} className="back-button">
                Back
            </button>
        </div>
    );
}
