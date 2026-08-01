import React, { useState, useEffect } from 'react';
import { getTransactions } from '../services/api';
import { useAuth } from '../contexts/AuthContext';

type TransactionType = 'Credit' | 'Debit';

interface Transaction {
  id: string;
  amount: number;
  type: TransactionType;
  description: string;
  date: string;
  accountId: string;
}

interface TransactionStatementResult {
  accountId: string;
  balance: number;
  totalCount: number;
  page: number;
  pageSize: number;
  transactions: Transaction[];
}

const Transactions: React.FC = () => {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [balance, setBalance] = useState<number>(0);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const { user } = useAuth();

  const fetchTransactions = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await getTransactions();
      setTransactions(data.transactions);
      setBalance(data.balance);
    } catch (err) {
      setError('Falha ao carregar transações. Tente novamente.');
      console.error('Error fetching transactions:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchTransactions();
  }, []);

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getTransactionTypeBadge = (type: Transaction['type']) => {
    return type === 'Credit' ? (
      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
        Crédito
      </span>
    ) : (
      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
        Débito
      </span>
    );
  };

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Extrato Bancário</h1>
        </div>

        {user && (
          <div className="mb-6 p-4 bg-white rounded-lg shadow">
            <div className="flex justify-between items-center">
              <div>
                <p className="text-sm text-gray-600">Saldo Atual</p>
                <p className="text-2xl font-bold text-gray-900">{formatCurrency(balance)}</p>
              </div>
              <div className="text-right">
                <p className="text-sm text-gray-600">Conta: {user.name}</p>
                <p className="text-sm text-gray-500">Documento: {user.document}</p>
              </div>
            </div>
          </div>
        )}

        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-sm text-red-700">{error}</p>
          </div>
        )}

        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="flex flex-col items-center">
              <svg className="animate-spin h-8 w-8 text-indigo-600 mb-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              <p className="text-gray-600">Carregando transações...</p>
            </div>
          </div>
        ) : transactions.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg border-2 border-dashed border-gray-200">
            <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
            </svg>
            <h3 className="mt-2 text-sm font-medium text-gray-900">Nenhuma transação encontrada</h3>
            <p className="mt-1 text-sm text-gray-500">As transações aparecerão aqui quando forem realizadas.</p>
          </div>
        ) : (
          <div className="bg-white shadow overflow-hidden sm:rounded-lg">
            <div className="px-4 py-5 sm:px-6">
              <h3 className="text-lg leading-6 font-medium text-gray-900">Histórico de Transações</h3>
              <p className="mt-1 max-w-2xl text-sm text-gray-500">
                {transactions.length} transação(s)
              </p>
            </div>
            <div className="border-t border-gray-200">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Data
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Descrição
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Tipo
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Valor
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {transactions.map((transaction) => (
                      <tr key={transaction.id}>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {formatDate(transaction.date)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {transaction.description}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {getTransactionTypeBadge(transaction.type)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium" style={{
                          color: transaction.type === 'Credit' ? '#16a34a' : '#dc2626'
                        }}>
                          {transaction.type === 'Credit' ? '+' : '-'}{' '}
                          {formatCurrency(transaction.amount)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Transactions;