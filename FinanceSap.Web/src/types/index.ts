// ============================================================
// FinanceSap.Web - Type Definitions
// ============================================================
// This file contains all TypeScript interfaces for API payloads,
// responses, and the User object used across the application.
// ============================================================

/**
 * Represents an authenticated user in the system.
 */
export interface User {
  id: string;
  email: string;
  name: string;
  roles: string[];
}

/**
 * Request payload for the login endpoint.
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Request payload for the refresh token endpoint.
 */
export interface RefreshRequest {
  expiredToken: string;
  refreshToken: string;
}

/**
 * Response payload returned by the login and refresh endpoints.
 */
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

/**
 * RFC 7807 ProblemDetails response returned by the backend on errors.
 * @see https://datatracker.ietf.org/doc/html/rfc7807
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

/**
 * Shape of the auth tokens stored in localStorage.
 */
export interface StoredAuthTokens {
  accessToken: string;
  refreshToken: string;
}

/**
 * Shape of the auth data stored in localStorage (tokens + user).
 */
export interface StoredAuthData extends StoredAuthTokens {
  user: User;
}

/**
 * Represents a customer in the system.
 */
export interface Customer {
  id: string;
  name: string;
  document: string;
  email: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * Represents a bank account in the system.
 */
export interface Account {
  id: string;
  accountNumber: string;
  balance: number;
  customerId: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * Request payload for deposit and withdraw operations.
 */
export interface MoneyRequest {
  amount: number;
}

/**
 * Request payload for transfer operations.
 */
export interface TransferRequest {
  destinationAccountId: string;
  amount: number;
}

/**
 * Response from account balance endpoint.
 */
export interface AccountBalanceResponse {
  balance: number;
}