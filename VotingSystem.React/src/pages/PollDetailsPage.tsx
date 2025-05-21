import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { PollResponseDto } from "../api/models/PollResponseDto";
import { getPollById } from "../api/client/polls-client";
import { vote } from "../api/client/votes-client";
import { useNavigate } from "react-router-dom";
import '../PollDetailsPage.css';

export default function PollDetailsPage() {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const [poll, setPoll] = useState<PollResponseDto | null>(null);
    const [selectedOptionId, setSelectedOptionId] = useState<number | null>(null);

    useEffect(() => {
        if (id) {
            getPollById(parseInt(id))
                .then(setPoll)
                .catch(console.error);
        }
    }, [id]);

    const handleVote = async () => {
        if (!selectedOptionId || !id) return;

        try {
            await vote({
                pollId: parseInt(id),
                optionId: selectedOptionId!,
            });
            alert("Thank you for voting!");
            navigate("/polls/active");
        } catch (error) {
            console.error("Voting failed:", error);
            alert("Voting failed. Please try again.");
        }
    };

    if (!poll) return <p>Loading...</p>;

    return (
        <div className="container">
            <h1>{poll.question}</h1>
            <form>
                {poll.options.map(option => (
                    <label key={option.id} className="option">
                        <input
                            type="radio"
                            name="option"
                            value={option.id}
                            checked={selectedOptionId === option.id}
                            onChange={() => setSelectedOptionId(option.id)}
                            disabled={poll.hasVoted}
                        />
                        <span>{option.text}
                              {poll.votedOptionId === option.id && " ✅"}
                        </span>
                    </label>
                ))}

                <button
                    type="button"
                    onClick={handleVote}
                    disabled={poll.hasVoted || !selectedOptionId}
                >
                    {poll.hasVoted ? "You already voted" : "Vote"}
                </button>
            </form>
        </div>
    );
}
