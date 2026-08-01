export interface User {
  id: string;
  name: string;
  email: string;
  document: string;
  role: 'Customer' | 'Admin' | 'Manager';
}

export interface Account {
  id: string;
  accountNumber: string;
  balance: number;
  customerId: string;
}

export interface Transaction {
  id: string;
  amount: number;
  type: 'Credit' | 'Debit';
  description: string;
  date: string;
  accountId: string;
}

export interface ProblemDetails {
  type?: string;
  title: string;
  status?: number;
  detail?: string;
  instance?: string;
  [key: string]: any;
}

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

export interface LoanRequest {
  amount: number;
  installments: number;
}

export interface TransactionStatementResult {
  accountId: string;
  balance: number;
  totalCount: number;
  page: number;
  pageSize: number;
  transactions: Transaction[];
}

export interface StoredAuthData {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface RefreshRequest {
  expiredToken: string;
  refreshToken: string;
}