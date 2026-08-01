import React, { useState, useEffect } from 'react';

import api from '../../services/api';
import { X, DollarSign, Send, User } from 'lucide-react';
import { handleApiError } from '../../utils/errorHandler';

interface TransferModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  accountId: string;
  currentBalance: number;
}

const TransferModal: React.FC<TransferModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  accountId,
  currentBalance,
}) => {
  const [amount, setAmount] = useState<string>('');
  const [targetAccountId, setTargetAccountId] = useState<string>('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [targetAccountName, setTargetAccountName] = useState<string | null>(null);
  const [isValidatingAccount, setIsValidatingAccount] = useState(false);
  const [validationError, setValidationError] = useState<string | null>(null);

  if (!isOpen) return null;

  // Validate target account when account ID changes
  useEffect(() => {
    const validateAccount = async () => {
      if (targetAccountId.length < 3) {
        setTargetAccountName(null);
        setValidationError(null);
        return;
      }

      try {
        setIsValidatingAccount(true);
        setValidationError(null);

        // In a real app, you would call an API to validate the account
        // For now, we'll simulate a validation
        await new Promise(resolve => setTimeout(resolve, 500));

        if (targetAccountId === accountId) {
          setValidationError('Você não pode transferir para a mesma conta.');
          setTargetAccountName(null);
          return;
        }

        // Simulate finding an account
        setTargetAccountName('Conta de Destino');
      } catch (err) {
        setValidationError('Conta não encontrada.');
        setTargetAccountName(null);
      } finally {
        setIsValidatingAccount(false);
      }
    };

    const debounceTimer = setTimeout(() => {
      validateAccount();
    }, 500);

    return () => clearTimeout(debounceTimer);
  }, [targetAccountId, accountId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      const numericAmount = parseFloat(amount);
      if (isNaN(numericAmount) || numericAmount <= 0) {
        setError('Por favor, insira um valor válido.');
        return;
      }

      if (numericAmount > currentBalance) {
        setError('Saldo insuficiente para realizar esta operação.');
        return;
      }

      if (!targetAccountId) {
        setError('Por favor, informe o ID da conta de destino.');
        return;
      }

      if (targetAccountId === accountId) {
        setError('Você não pode transferir para a mesma conta.');
        return;
      }

      if (!targetAccountName) {
        setError('Conta de destino inválida.');
        return;
      }

        const request = {
          amount: numericAmount,
          targetAccountId: targetAccountId
        };

      await api.post(`/api/accounts/${accountId}/transfer`, request);

      setSuccess('Transferência realizada com sucesso!');
      setAmount('');
      setTargetAccountId('');

      // Call onSuccess after a short delay to allow user to see the success message
      setTimeout(() => {
        onSuccess();
      }, 1500);
    } catch (err: unknown) {
      console.error('Error during transfer:', err);
      setError(handleApiError(err));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6 relative">
        <button
          onClick={onClose}
          className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
        >
          <X className="h-6 w-6" />
        </button>

        <div className="flex items-center mb-4">
          <Send className="h-8 w-8 text-blue-600 mr-3" />
          <h2 className="text-xl font-semibold text-gray-900">Transferir Dinheiro</h2>
        </div>

        <p className="text-gray-600 mb-6">
          Insira o valor e a conta de destino. Seu saldo atual é de{' '}
          {currentBalance.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}.
        </p>

        {error && (
          <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-4">
            <div className="flex">
              <div className="flex-shrink-0">
                <svg className="h-5 w-5 text-red-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                </svg>
              </div>
              <div className="ml-3">
                <p className="text-sm text-red-700">{error}</p>
              </div>
            </div>
          </div>
        )}

        {success ? (
          <div className="bg-green-50 border-l-4 border-green-400 p-4 mb-4">
            <div className="flex">
              <div className="flex-shrink-0">
                <svg className="h-5 w-5 text-green-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                </svg>
              </div>
              <div className="ml-3">
                <p className="text-sm text-green-700">{success}</p>
              </div>
            </div>
          </div>
        ) : (
          <form onSubmit={handleSubmit}>
            <div className="space-y-4">
              <div>
                <label htmlFor="amount" className="block text-sm font-medium text-gray-700 mb-1">
                  Valor da Transferência
                </label>
                <div className="relative rounded-md shadow-sm">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <DollarSign className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    type="number"
                    id="amount"
                    name="amount"
                    min="0.01"
                    step="0.01"
                    value={amount}
                    onChange={(e) => setAmount(e.target.value)}
                    className="focus:ring-blue-500 focus:border-blue-500 block w-full pl-10 pr-12 sm:text-sm border-gray-300 rounded-md"
                    placeholder="0.00"
                    required
                  />
                  <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                    <span className="text-gray-500 sm:text-sm">BRL</span>
                  </div>
                </div>
              </div>

              <div>
                <label htmlFor="targetAccountId" className="block text-sm font-medium text-gray-700 mb-1">
                  ID da Conta de Destino
                </label>
                <div className="relative rounded-md shadow-sm">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <User className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    type="text"
                    id="targetAccountId"
                    name="targetAccountId"
                    value={targetAccountId}
                    onChange={(e) => setTargetAccountId(e.target.value)}
                    className="focus:ring-blue-500 focus:border-blue-500 block w-full pl-10 sm:text-sm border-gray-300 rounded-md"
                    placeholder="Digite o ID da conta"
                    required
                  />
                </div>

                {isValidatingAccount && (
                  <div className="mt-2 flex items-center">
                    <div className="animate-spin rounded-full h-4 w-4 border-t-2 border-b-2 border-blue-500 mr-2"></div>
                    <span className="text-sm text-gray-500">Validando conta...</span>
                  </div>
                )}

                {validationError && (
                  <p className="mt-2 text-sm text-red-600">{validationError}</p>
                )}

                {targetAccountName && !validationError && (
                  <p className="mt-2 text-sm text-green-600">{targetAccountName}</p>
                )}
              </div>
            </div>

            <div className="flex justify-end space-x-3 mt-6">
              <button
                type="button"
                onClick={onClose}
                className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isLoading || isValidatingAccount || !!validationError}
                className="px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? (
                  <>
                    <span className="animate-spin inline-block h-4 w-4 border-t-2 border-b-2 border-white rounded-full mr-2"></span>
                    Processando...
                  </>
                ) : (
                  'Transferir'
                )}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};

export default TransferModal;