import React, { useState, useEffect } from 'react';
import type { Loan } from '../types';
import { getPendingLoans, approveLoan, rejectLoan } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import { AlertCircle } from 'lucide-react';

const LoanApprovals: React.FC = () => {
  const [loans, setLoans] = useState<Loan[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isForbidden, setIsForbidden] = useState<boolean>(false);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const { user } = useAuth();

  const fetchPendingLoans = async () => {
    try {
      setIsLoading(true);
      setError(null);
      setIsForbidden(false);
      const data = await getPendingLoans();
      setLoans(data);
    } catch (err: any) {
      if (err?.response?.status === 403) {
        setIsForbidden(true);
        return;
      }
      setError('Falha ao carregar empréstimos pendentes. Tente novamente.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchPendingLoans();
  }, []);

  const handleApprove = async (loanId: string) => {
    try {
      setActionLoading(loanId);
      await approveLoan(loanId);
      await fetchPendingLoans();
    } catch (err) {
      setError('Falha ao aprovar empréstimo.');
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
      await fetchPendingLoans();
    } catch (err) {
      setError('Falha ao rejeitar empréstimo.');
    } finally {
      setActionLoading(null);
    }
  };

  const formatCurrency = (value: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);

  const formatDate = (dateString: string) =>
    new Date(dateString).toLocaleDateString('pt-BR');

  if (isForbidden) {
    return (
      <div className="w-full h-full flex items-center justify-center p-10 mt-10">
        <div className="bg-red-50 text-red-800 p-8 rounded-2xl border border-red-200 max-w-lg text-center shadow-sm">
          <h2 className="text-2xl font-bold mb-3">Acesso Restrito</h2>
          <p className="text-base text-red-700">
            Você não possui permissão de administrador para acessar a área de aprovações.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Aprovação de Empréstimos</h1>
        </div>

        {user && (
          <div className="mb-4 p-3 bg-blue-50 rounded-lg">
            <p className="text-sm text-blue-700">
              <span className="font-medium">{user.name}</span>
            </p>
          </div>
        )}

        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-2">
            <AlertCircle className="h-5 w-5 text-red-400 shrink-0" />
            <p className="text-sm text-red-700">{error}</p>
          </div>
        )}

        {isLoading ? (
          <div className="flex flex-col items-center justify-center py-12">
            <svg className="animate-spin h-8 w-8 text-indigo-600 mb-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
            </svg>
            <p className="text-gray-600">Carregando empréstimos pendentes...</p>
          </div>
        ) : loans.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg border-2 border-dashed border-gray-200">
            <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
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
            <div className="border-t border-gray-200 overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    {['Cliente', 'Documento', 'Valor', 'Parcelas', 'Data Solicitação', 'Ações'].map((h) => (
                      <th key={h} className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        {h}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {loans.map((loan) => (
                    <tr key={loan.id}>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{loan.customerName}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{loan.document}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatCurrency(loan.amount)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{loan.installments}x</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{formatDate(loan.requestDate)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        <div className="flex space-x-2">
                          <button
                            onClick={() => handleApprove(loan.id)}
                            disabled={actionLoading === loan.id}
                            className="px-3 py-1 text-xs font-medium rounded-md text-white bg-green-600 hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            {actionLoading === loan.id ? '...' : 'Aprovar'}
                          </button>
                          <button
                            onClick={() => handleReject(loan.id)}
                            disabled={actionLoading === loan.id}
                            className="px-3 py-1 text-xs font-medium rounded-md text-white bg-red-600 hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            {actionLoading === loan.id ? '...' : 'Rejeitar'}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default LoanApprovals;
