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
  document: string;
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

/**
 * Represents a loan in the system.
 */
export interface Loan {
  id: string;
  customerId: string;
  customerName: string;
  document: string;
  amount: number;
  installments: number;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Paid';
  requestDate: string;
  approvalDate?: string;
  rejectionReason?: string;
}

/**
 * Request payload for loan requests.
 */
export interface LoanRequest {
  amount: number;
  installments: number;
}

/**
 * Transaction type for the dashboard and transaction history.
 */
export type TransactionType = 'Credit' | 'Debit';

/**
 * Represents a financial transaction in the system.
 */
export interface Transaction {
  id: string;
  amount: number;
  type: TransactionType;
  description: string;
  date: string;
  accountId: string;
}

/**
 * Response from the account statement endpoint.
 */
export interface TransactionStatementResult {
  accountId: string;
  balance: number;
  totalCount: number;
  page: number;
  pageSize: number;
  transactions: Transaction[];
}

/**
 * Response from the recent transactions endpoint.
 */
export interface RecentTransactionsResponse {
  transactions: Transaction[];
}