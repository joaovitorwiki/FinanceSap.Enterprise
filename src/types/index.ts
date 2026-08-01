export interface User {
  id: string;
  name: string;
  email: string;
  document: string;
  role: 'Customer' | 'Admin';
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
  type: 'Deposit' | 'Withdrawal' | 'Transfer';
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