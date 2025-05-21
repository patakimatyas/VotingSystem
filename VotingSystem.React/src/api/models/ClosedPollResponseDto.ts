import type { OptionResponseDto } from "./OptionResponseDto";

export interface ClosedPollResponseDto {
    id: number;
    question: string;
    startDate: string;
    endDate: string;
    options: OptionResponseDto[];
}