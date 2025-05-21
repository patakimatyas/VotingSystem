import { postAsJsonWithoutResponse } from "./http";
import { VoteRequestDto } from "../models/VoteRequestDto";

export async function vote(voteRequest: VoteRequestDto): Promise<void> {
    return await postAsJsonWithoutResponse<VoteRequestDto>(`votes/`, voteRequest);
}