import type { OptionResponseDto } from "./OptionResponseDto";


export interface PollResponseDto {
    id: number;
    question: string;
    startDate: string;
    endDate: string;
    options: OptionResponseDto[];
    hasVoted: boolean;
    votedOptionId: number | null;
}
