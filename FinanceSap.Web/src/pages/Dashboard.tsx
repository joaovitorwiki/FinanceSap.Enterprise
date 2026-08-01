// ============================================================
// FinanceSap.Web - Dashboard Page
// ============================================================
// Dashboard page that displays:
// - User profile information
// - Account balance
// - Financial transaction options
// ============================================================

import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import type { Customer, Account } from '../types';
import api from '../services/api';
import { handleApiError } from '../utils/errorHandler';
import { User, CreditCard, ArrowUpCircle, ArrowDownCircle, Send, LogOut, AlertCircle } from 'lucide-react';
import DepositModal from '../components/transactions/DepositModal';
import WithdrawModal from '../components/transactions/WithdrawModal';
import TransferModal from '../components/transactions/TransferModal';

/**
 * Dashboard page component.
 */
const Dashboard: React.FC = () => {
  const { logout } = useAuth();
  const [customer, setCustomer] = useState<Customer | null>(null);
  const [account, setAccount] = useState<Account | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modal states
  const [isDepositModalOpen, setIsDepositModalOpen] = useState(false);
  const [isWithdrawModalOpen, setIsWithdrawModalOpen] = useState(false);
  const [isTransferModalOpen, setIsTransferModalOpen] = useState(false);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Fetch customer and account data in parallel
      const [customerResponse, accountResponse] = await Promise.all([
        api.get<Customer>('/api/customers/me'),
        api.get<Account>('/api/accounts/primary')
      ]);

      setCustomer(customerResponse.data);
      setAccount(accountResponse.data);
    } catch (err: unknown) {
      console.error('Error fetching data:', err);
      setError(handleApiError(err));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleTransactionSuccess = () => {
    // Refresh data after successful transaction
    fetchData();
    // Close all modals
    setIsDepositModalOpen(false);
    setIsWithdrawModalOpen(false);
    setIsTransferModalOpen(false);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <div className="flex flex-col items-center space-y-4">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-indigo-600"></div>
          <p className="text-sm text-gray-600">Carregando dados...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <div className="bg-red-50 border-l-4 border-red-400 p-4 rounded-lg shadow-sm">
          <div className="flex">
            <div className="flex-shrink-0">
              <AlertCircle className="h-5 w-5 text-red-400" />
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-red-800">Erro ao carregar dados</h3>
              <p className="text-sm text-red-700 mt-1">{error}</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!customer || !account) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 rounded-lg shadow-sm">
          <div className="flex">
            <div className="flex-shrink-0">
              <AlertCircle className="h-5 w-5 text-yellow-400" />
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-yellow-800">Dados não encontrados</h3>
              <p className="text-sm text-yellow-700 mt-1">Não foi possível carregar as informações do cliente.</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex justify-between items-center mb-12">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
            <p className="mt-1 text-sm text-gray-500">Gerencie suas finanças com segurança e eficiência</p>
          </div>
          <button
            onClick={logout}
            className="flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-lg text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition duration-150 ease-in-out shadow-sm"
          >
            <LogOut className="h-4 w-4 mr-2" />
            Sair
          </button>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Balance Card */}
          <div className="lg:col-span-2">
            <div className="bg-gradient-to-br from-indigo-600 to-blue-800 rounded-xl shadow-xl p-8 text-white">
              <div className="flex justify-between items-start">
                <div>
                  <p className="text-sm font-medium opacity-90">Saldo Atual</p>
                  <p className="text-4xl font-bold mt-2">
                    {account.balance.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                  </p>
                  <p className="text-sm mt-1 opacity-80">Conta: {account.accountNumber}</p>
                </div>
                <div className="bg-white bg-opacity-10 rounded-lg p-3">
                  <CreditCard className="h-8 w-8 text-white" />
                </div>
              </div>
              <div className="mt-8 flex flex-wrap gap-3">
                <button
                  onClick={() => setIsDepositModalOpen(true)}
                  className="flex items-center px-4 py-2.5 bg-white bg-opacity-20 hover:bg-opacity-30 text-white text-sm font-medium rounded-lg transition-all duration-150 ease-in-out shadow-sm hover:shadow-md"
                >
                  <ArrowUpCircle className="h-4 w-4 mr-2" />
                  Depositar
                </button>
                <button
                  onClick={() => setIsWithdrawModalOpen(true)}
                  className="flex items-center px-4 py-2.5 bg-white bg-opacity-20 hover:bg-opacity-30 text-white text-sm font-medium rounded-lg transition-all duration-150 ease-in-out shadow-sm hover:shadow-md"
                >
                  <ArrowDownCircle className="h-4 w-4 mr-2" />
                  Sacar
                </button>
                <button
                  onClick={() => setIsTransferModalOpen(true)}
                  className="flex items-center px-4 py-2.5 bg-white bg-opacity-20 hover:bg-opacity-30 text-white text-sm font-medium rounded-lg transition-all duration-150 ease-in-out shadow-sm hover:shadow-md"
                >
                  <Send className="h-4 w-4 mr-2" />
                  Transferir
                </button>
              </div>
            </div>
          </div>

          {/* Profile Card */}
          <div className="bg-white rounded-xl shadow-lg p-6 border border-gray-100">
            <div className="flex items-center mb-6">
              <div className="bg-indigo-100 p-2 rounded-lg">
                <User className="h-6 w-6 text-indigo-600" />
              </div>
              <h3 className="text-xl font-semibold text-gray-900 ml-3">Perfil</h3>
            </div>
            <div className="space-y-4">
              <div>
                <p className="text-sm font-medium text-gray-500">Nome Completo</p>
                <p className="text-sm font-medium text-gray-900 mt-1">{customer.name}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-gray-500">Documento</p>
                <p className="text-sm font-medium text-gray-900 mt-1">{customer.document}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-gray-500">Email</p>
                <p className="text-sm font-medium text-gray-900 mt-1">{customer.email}</p>
              </div>
            </div>
          </div>
        </div>

        {/* Transaction Modals */}
        <DepositModal
          isOpen={isDepositModalOpen}
          onClose={() => setIsDepositModalOpen(false)}
          onSuccess={handleTransactionSuccess}
          accountId={account.id}
        />

        <WithdrawModal
          isOpen={isWithdrawModalOpen}
          onClose={() => setIsWithdrawModalOpen(false)}
          onSuccess={handleTransactionSuccess}
          accountId={account.id}
          currentBalance={account.balance}
        />

        <TransferModal
          isOpen={isTransferModalOpen}
          onClose={() => setIsTransferModalOpen(false)}
          onSuccess={handleTransactionSuccess}
          accountId={account.id}
          currentBalance={account.balance}
        />
      </div>
    </div>
  );
};

export default Dashboard;