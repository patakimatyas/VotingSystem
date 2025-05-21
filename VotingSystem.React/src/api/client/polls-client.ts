import { get } from "./http";
//import { postAsJson } from "./http";
import { PollResponseDto } from "../models/PollResponseDto";
import { ClosedPollResponseDto } from "../models/ClosedPollResponseDto";

export async function getActivePolls(): Promise<PollResponseDto[]> {
    return await get<PollResponseDto[]>("polls/active");
}

export async function getPollById(id: number): Promise<PollResponseDto> {
    return await get<PollResponseDto>(`polls/${id}`);
}
export async function getClosedPollResult(id: number): Promise<ClosedPollResponseDto> {
    return await get<PollResponseDto>(`polls/closed/${id}`);
}
export async function getClosedPolls(params?: Record<string, string>) {
    return get<PollResponseDto[]>("polls/closed", params);
}