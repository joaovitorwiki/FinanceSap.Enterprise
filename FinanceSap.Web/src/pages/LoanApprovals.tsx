import React, { useState, useEffect } from 'react';
import type { Loan } from '../types';
import { getPendingLoans, approveLoan, rejectLoan } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import { AlertCircle, ShieldAlert } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

const LoanApprovals: React.FC = () => {
  const [loans, setLoans] = useState<Loan[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const { user } = useAuth();
  const navigate = useNavigate();

  // Check if user has admin privileges
  const hasAdminPrivileges = user && (user.roles?.includes('Admin') || user.roles?.includes('Manager'));
  const userName = user ? (typeof user.name === 'string' ? user.name : (user as any).fullName || 'Usuário') : 'Usuário';
  const userDocument = user ? (typeof user.document === 'string' ? user.document : (user.document as any)?.value || 'N/A') : 'N/A';
  const userRole = user ? (user.roles?.includes('Admin') ? 'Administrador' : user.roles?.includes('Manager') ? 'Gerente' : 'Cliente') : 'Cliente';

  const fetchPendingLoans = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await getPendingLoans();
      setLoans(data);
    } catch (err: any) {
      // Check if error is due to unauthorized access (401 or 403)
      if (err.response?.status === 401 || err.response?.status === 403) {
        setError('Acesso não autorizado. Você não tem permissão para acessar esta página.');
      } else {
        setError('Falha ao carregar empréstimos pendentes. Tente novamente.');
      }
      console.error('Error fetching pending loans:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (!hasAdminPrivileges) {
      // If user doesn't have admin privileges, show access denied message
      setIsLoading(false);
      return;
    }
    fetchPendingLoans();
  }, [hasAdminPrivileges]);

  const handleApprove = async (loanId: string) => {
    try {
      setActionLoading(loanId);
      await approveLoan(loanId);
      alert('Empréstimo aprovado com sucesso!');
      await fetchPendingLoans(); // Refresh the list
    } catch (err) {
      alert('Falha ao aprovar empréstimo.');
      console.error('Error approving loan:', err);
    } finally {
      setActionLoading(null);
    }
  };

  const handleReject = async (loanId: string) => {
    const reason = prompt('Informe o motivo da rejeição:');
    if (!reason) return;

    try {
      setActionLoading(loanId);
      await rejectLoan(loanId, reason);
      alert('Empréstimo rejeitado com sucesso!');
      await fetchPendingLoans(); // Refresh the list
    } catch (err) {
      alert('Falha ao rejeitar empréstimo.');
      console.error('Error rejecting loan:', err);
    } finally {
      setActionLoading(null);
    }
  };

  const getStatusBadge = (status: Loan['status']) => {
    switch (status) {
      case 'Pending':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
            Pendente
          </span>
        );
      case 'Approved':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
            Aprovado
          </span>
        );
      case 'Rejected':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
            Rejeitado
          </span>
        );
      case 'Paid':
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
            Pago
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
            {status}
          </span>
        );
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR');
  };

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Aprovação de Empréstimos</h1>
        </div>

        {user && (
          <div className="mb-4 p-3 bg-blue-50 rounded-lg">
            <p className="text-sm text-blue-700">
              <span className="font-medium">{userName}</span> • {userRole}
            </p>
          </div>
        )}

        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
            <div className="flex">
              <AlertCircle className="h-5 w-5 text-red-400 mr-2" />
              <p className="text-sm text-red-700">{error}</p>
            </div>
          </div>
        )}

        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="flex flex-col items-center">
              <svg className="animate-spin h-8 w-8 text-indigo-600 mb-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              <p className="text-gray-600">Carregando empréstimos pendentes...</p>
            </div>
          </div>
        ) : !hasAdminPrivileges ? (
          <div className="text-center py-12 bg-white rounded-lg border-2 border-dashed border-gray-200">
            <div className="mx-auto h-16 w-16 text-red-400 mb-4 flex items-center justify-center">
              <ShieldAlert className="h-16 w-16" />
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">Acesso Restrito</h3>
            <p className="text-sm text-gray-500 mb-4">Esta página é restrita a usuários com privilégios administrativos.</p>
            <button
              onClick={() => navigate('/dashboard')}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              Voltar ao Dashboard
            </button>
          </div>
        ) : loans.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg border-2 border-dashed border-gray-200">
            <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
            </svg>
            <h3 className="mt-2 text-sm font-medium text-gray-900">Nenhum empréstimo pendente</h3>
            <p className="mt-1 text-sm text-gray-500">Não há empréstimos aguardando aprovação.</p>
          </div>
        ) : (
          <div className="bg-white shadow overflow-hidden sm:rounded-lg">
            <div className="px-4 py-5 sm:px-6">
              <h3 className="text-lg leading-6 font-medium text-gray-900">Empréstimos Pendentes</h3>
              <p className="mt-1 max-w-2xl text-sm text-gray-500">
                {loans.length} empréstimo(s) aguardando aprovação
              </p>
            </div>
            <div className="border-t border-gray-200">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Cliente
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Documento
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Valor
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Parcelas
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Data Solicitação
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Ações
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {loans.map((loan) => (
                      <tr key={loan.id}>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                          {loan.customerName}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {loan.document}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {formatCurrency(loan.amount)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {loan.installments}x
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {formatDate(loan.requestDate)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                          <div className="flex space-x-2">
                            <button
                              onClick={() => handleApprove(loan.id)}
                              disabled={actionLoading === loan.id}
                              className="px-3 py-1 border border-transparent text-xs font-medium rounded-md shadow-sm text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                              {actionLoading === loan.id ? (
                                <svg className="animate-spin h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                </svg>
                              ) : 'Aprovar'}
                            </button>
                            <button
                              onClick={() => handleReject(loan.id)}
                              disabled={actionLoading === loan.id}
                              className="px-3 py-1 border border-transparent text-xs font-medium rounded-md shadow-sm text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                              {actionLoading === loan.id ? (
                                <svg className="animate-spin h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                </svg>
                              ) : 'Rejeitar'}
                            </button>
                          </div>
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

export default LoanApprovals;