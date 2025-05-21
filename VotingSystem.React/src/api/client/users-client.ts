import { postAsJson, postAsJsonWithoutResponse, get } from "./http";
import type { LoginRequestDto} from "../models/LoginRequestDto";
import type { LoginResponseDto} from "../models/LoginResponseDto";
import type { UserResponseDto} from "../models/UserResponseDto";
import type { UserRequestDto} from "../models/UserRequestDto";

export async function login(loginDto: LoginRequestDto): Promise<LoginResponseDto> {
    return await postAsJson<LoginRequestDto, LoginResponseDto>("users/login", loginDto);
}

export async function logout(): Promise<void> {
    await postAsJsonWithoutResponse("users/logout");
}

export async function refresh(refreshToken: string): Promise<LoginResponseDto> {
    return await postAsJson<string, LoginResponseDto>("users/refresh", refreshToken);
}

export async function createUser(data: UserRequestDto): Promise<UserResponseDto> {
    return await postAsJson<UserRequestDto, UserResponseDto>("users/register", data);
}

export async function getUserById(id: string): Promise<UserResponseDto> {
    return get<UserResponseDto>(`users/${id}`);
}