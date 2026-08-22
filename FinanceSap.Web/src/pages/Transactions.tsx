import React, { useState, useEffect } from 'react';
import type { Transaction } from '../types';
import { getTransactions } from '../services/api';

const Transactions: React.FC = () => {
  const [transactions, setTransactions] = useState<Transaction[]>([]);

  useEffect(() => {
    getTransactions()
      .then((data) => setTransactions(data.transactions ?? []))
      .catch(() => setTransactions([]));
  }, []);

  const formatCurrency = (value: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);

  const formatDate = (dateString: string) =>
    new Date(dateString).toLocaleDateString('pt-BR');

  return (
    <div className="p-8 max-w-6xl mx-auto w-full">
      <h2 className="text-2xl font-bold text-gray-800 mb-6">Histórico de Transações</h2>
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        {transactions.length === 0 ? (
          <div className="p-12 text-center text-gray-500 font-medium">
            Nenhuma transação encontrada
          </div>
        ) : (
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="p-4 text-sm font-semibold text-gray-600">Tipo</th>
                <th className="p-4 text-sm font-semibold text-gray-600">Descrição</th>
                <th className="p-4 text-sm font-semibold text-gray-600">Data</th>
                <th className="p-4 text-sm font-semibold text-gray-600">Valor</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {transactions.map((tx) => (
                <tr key={tx.id} className="hover:bg-gray-50 transition-colors">
                  <td className="p-4 text-sm text-gray-700">{tx.type}</td>
                  <td className="p-4 text-sm text-gray-700">{tx.description || '-'}</td>
                  <td className="p-4 text-sm text-gray-700">{formatDate(tx.date)}</td>
                  <td className={`p-4 text-sm font-bold ${tx.type === 'Credit' ? 'text-green-600' : 'text-gray-900'}`}>
                    {formatCurrency(tx.amount)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Transactions;
