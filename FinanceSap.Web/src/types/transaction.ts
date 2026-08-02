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
 * Response from the recent transactions endpoint.
 */
export interface RecentTransactionsResponse {
  transactions: Transaction[];
}