// src/app/core/models/auth.model.ts

export interface UserLoginDto {
  username: string;
  password: string;
}

export interface UserRegisterDto {
  username: string;
  email: string;
  password: string;
}

export interface AuthResponseDto {
  token: string;
}

export interface TokenPayload {
  username: string;
  role: string;
  exp: number; 
}