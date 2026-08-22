import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import type { Customer, Account, Transaction } from '../types';
import api from '../services/api';
import {
  User,
  CreditCard,
  ArrowUpRight,
  ArrowDownLeft,
  Send,
  LogOut,
  AlertCircle,
  Clock,
  DollarSign,
  TrendingUp,
  TrendingDown,
  ArrowRight
} from 'lucide-react';
import DepositModal from '../components/transactions/DepositModal';
import WithdrawModal from '../components/transactions/WithdrawModal';
import TransferModal from '../components/transactions/TransferModal';

/**
 * Dashboard page component.
 */
const Dashboard: React.FC = () => {
  const { logout, user: authUser } = useAuth();
  const [customer, setCustomer] = useState<Customer | null>(null);
  const [account, setAccount] = useState<Account | null>(null);
  const [recentTransactions, setRecentTransactions] = useState<Transaction[]>([]);
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

      // /customers/me may 404 for admin users — fall back to auth context user
      let customerData: Customer | null = null;
      try {
        const customerResponse = await api.get<Customer>('/customers/me');
        const raw = customerResponse.data as any;
        customerData = {
          ...customerResponse.data,
          name: raw.fullName || customerResponse.data.name || authUser?.name || 'Não informado',
          document: typeof raw.document === 'object' && raw.document !== null
            ? (raw.document as { value: string }).value
            : raw.document,
          email: typeof raw.email === 'object' && raw.email !== null
            ? (raw.email as { value: string }).value
            : raw.email,
        };
      } catch {
        if (authUser) {
          customerData = {
            id: authUser.id,
            name: authUser.name,
            email: authUser.email,
            document: authUser.document ?? '',
            createdAt: '',
            updatedAt: '',
          };
        }
      }

      // Fetch primary account
      let accountData: Account | null = null;
      try {
        const accountResponse = await api.get<Account>('/accounts/primary');
        const raw = accountResponse.data as any;
        accountData = 'value' in raw ? raw.value : raw;
      } catch {
        accountData = {
          id: '0',
          customerId: customerData?.id ?? '0',
          accountNumber: '—',
          balance: 0,
          createdAt: '',
          updatedAt: '',
        };
      }

      // Fetch recent transactions
      let transactionsData: Transaction[] = [];
      try {
        const txResponse = await api.get<any>('/transactions/recent');
        const payload = txResponse.data;
        transactionsData = Array.isArray(payload)
          ? payload
          : Array.isArray(payload?.transactions)
            ? payload.transactions
            : [];
      } catch {
        try {
          const txResponse = await api.get<any>('/transactions');
          const payload = txResponse.data;
          transactionsData = Array.isArray(payload)
            ? payload
            : Array.isArray(payload?.transactions)
              ? payload.transactions
              : [];
        } catch {
          transactionsData = [];
        }
      }

      setCustomer(customerData);
      setAccount(accountData);
      setRecentTransactions(transactionsData);
    } catch (err: unknown) {
      console.error('Error fetching dashboard data:', err);
      // Only set error for truly unexpected failures — not 404s on optional endpoints
      setError('Não foi possível carregar os dados. Tente novamente.');
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

  const getTransactionTypeIcon = (type: Transaction['type']) => {
    return type === 'Credit' ? (
      <TrendingUp className="h-5 w-5 text-green-500" />
    ) : (
      <TrendingDown className="h-5 w-5 text-red-500" />
    );
  };

  const getTransactionTypeText = (type: Transaction['type']) => {
    return type === 'Credit' ? 'Crédito' : 'Débito';
  };

  const getTransactionAmountColor = (type: Transaction['type']) => {
    return type === 'Credit' ? 'text-green-600' : 'text-red-600';
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

  // customer/account always have fallback values — only block render on a true unexpected error
  if (error && !customer && !account) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <div className="bg-red-50 border-l-4 border-red-400 p-4 rounded-lg shadow-sm">
          <div className="flex">
            <AlertCircle className="h-5 w-5 text-red-400 shrink-0" />
            <div className="ml-3">
              <h3 className="text-sm font-medium text-red-800">Erro ao carregar dados</h3>
              <p className="text-sm text-red-700 mt-1">{error}</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Guarantee non-null for JSX below — both always set by fetchData
  if (!customer || !account) return null;

  // Format document for display
  const formatDocument = (document: string) => {
    if (!document) return 'Não informado';
    // Format CPF: 000.000.000-00
    if (document.length === 11) {
      return document.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    }
    return document;
  };

  // Format member since date
  const formatMemberSince = (dateString: string) => {
    if (!dateString) return 'Não informado';
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', {
      month: 'long',
      year: 'numeric'
    });
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
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

        {/* Main Content */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Balance Card and Quick Actions */}
          <div className="lg:col-span-2">
            {/* Balance Card */}
            <div className="bg-gradient-to-br from-indigo-600 to-blue-800 rounded-2xl shadow-lg p-6 text-white mb-6">
              <div className="flex justify-between items-start">
                <div>
                  <p className="text-sm font-medium opacity-90">Saldo Atual</p>
                  <p className="text-4xl font-bold mt-2">
                    {formatCurrency(account.balance)}
                  </p>
                  <p className="text-sm mt-1 opacity-80">Conta: {account.accountNumber}</p>
                </div>
                <div className="bg-white bg-opacity-10 rounded-lg p-3">
                  <CreditCard className="h-8 w-8 text-white" />
                </div>
              </div>

              {/* Quick Action Buttons */}
              <div className="mt-8 flex flex-wrap gap-3">
                <button
                  onClick={() => setIsDepositModalOpen(true)}
                  className="flex flex-col items-center justify-center p-3 w-24 bg-white/20 hover:bg-white/30 rounded-xl transition-colors border border-white/20"
                >
                  <ArrowDownLeft className="h-6 w-6 text-white" />
                  <span className="text-sm font-medium text-white mt-2">Depositar</span>
                </button>
                <button
                  onClick={() => setIsWithdrawModalOpen(true)}
                  className="flex flex-col items-center justify-center p-3 w-24 bg-white/20 hover:bg-white/30 rounded-xl transition-colors border border-white/20"
                >
                  <ArrowUpRight className="h-6 w-6 text-white" />
                  <span className="text-sm font-medium text-white mt-2">Sacar</span>
                </button>
                <button
                  onClick={() => setIsTransferModalOpen(true)}
                  className="flex flex-col items-center justify-center p-3 w-24 bg-white/20 hover:bg-white/30 rounded-xl transition-colors border border-white/20"
                >
                  <Send className="h-6 w-6 text-white" />
                  <span className="text-sm font-medium text-white mt-2">Transferir</span>
                </button>
              </div>
            </div>

            {/* Recent Activity */}
            <div className="bg-white rounded-xl shadow-lg p-6 border border-gray-100">
              <div className="flex items-center mb-6">
                <div className="bg-indigo-100 p-2 rounded-lg">
                  <Clock className="h-6 w-6 text-indigo-600" />
                </div>
                <h3 className="text-xl font-semibold text-gray-900 ml-3">Atividade Recente</h3>
              </div>

              {recentTransactions.length === 0 ? (
                <div className="text-center py-8">
                  <div className="mx-auto h-12 w-12 text-gray-400 mb-4">
                    <Clock className="h-12 w-12 mx-auto" />
                  </div>
                  <h3 className="text-sm font-medium text-gray-900">Nenhuma transação recente</h3>
                  <p className="text-sm text-gray-500 mt-1">Suas transações aparecerão aqui</p>
                </div>
              ) : (
                <div className="space-y-4">
                  {recentTransactions.slice(0, 5).map((transaction) => (
                    <div key={transaction.id} className="flex items-center justify-between p-3 hover:bg-gray-50 rounded-lg transition-colors duration-150">
                      <div className="flex items-center">
                        <div className="mr-3">
                          {getTransactionTypeIcon(transaction.type)}
                        </div>
                        <div>
                          <p className="text-sm font-medium text-gray-900">{transaction.description || 'Transação'}</p>
                          <p className="text-xs text-gray-500">{formatDate(transaction.date)}</p>
                        </div>
                      </div>
                      <div className="flex items-center">
                        <span className={`text-sm font-medium ${getTransactionAmountColor(transaction.type)}`}>
                          {transaction.type === 'Credit' ? '+' : '-'}{' '}
                          {formatCurrency(transaction.amount)}
                        </span>
                        <span className="ml-2 text-xs text-gray-400">
                          {getTransactionTypeText(transaction.type)}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {recentTransactions.length > 0 && (
                <div className="mt-6">
                  <button
                    onClick={() => window.location.href = '/transactions'}
                    className="flex items-center justify-center w-full px-4 py-2 border border-gray-300 rounded-lg shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out"
                  >
                    Ver todas as transações
                    <ArrowRight className="h-4 w-4 ml-2" />
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Right Sidebar */}
          <div className="space-y-6">
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
                  <p className="text-sm font-medium text-gray-900 mt-1">{formatDocument(customer.document)}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-gray-500">Email</p>
                  <p className="text-sm font-medium text-gray-900 mt-1">{customer.email}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-gray-500">Membro desde</p>
                  <p className="text-sm font-medium text-gray-900 mt-1">
                    {formatMemberSince(customer.createdAt)}
                  </p>
                </div>
              </div>
            </div>

            {/* Financial Summary */}
            <div className="bg-white rounded-xl shadow-lg p-6 border border-gray-100">
              <div className="flex items-center mb-6">
                <div className="bg-indigo-100 p-2 rounded-lg">
                  <DollarSign className="h-6 w-6 text-indigo-600" />
                </div>
                <h3 className="text-xl font-semibold text-gray-900 ml-3">Resumo Financeiro</h3>
              </div>
              <div className="space-y-4">
                <div className="flex justify-between items-center">
                  <div className="flex items-center">
                    <TrendingUp className="h-5 w-5 text-green-500 mr-2" />
                    <span className="text-sm font-medium text-gray-700">Recebimentos</span>
                  </div>
                  <span className="text-sm font-medium text-green-600">
                    {formatCurrency(recentTransactions
                      .filter(t => t.type === 'Credit')
                      .reduce((sum, t) => sum + t.amount, 0))}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <div className="flex items-center">
                    <TrendingDown className="h-5 w-5 text-red-500 mr-2" />
                    <span className="text-sm font-medium text-gray-700">Pagamentos</span>
                  </div>
                  <span className="text-sm font-medium text-red-600">
                    {formatCurrency(recentTransactions
                      .filter(t => t.type === 'Debit')
                      .reduce((sum, t) => sum + t.amount, 0))}
                  </span>
                </div>
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