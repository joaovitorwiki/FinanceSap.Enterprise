import React, { useState } from 'react';
import type { MoneyRequest } from '../../types';
import api from '../../services/api';
import { X, DollarSign, ArrowUpCircle, AlertCircle, CheckCircle2 } from 'lucide-react';
import { handleApiError } from '../../utils/errorHandler';

interface DepositModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  accountId: string;
}

const DepositModal: React.FC<DepositModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  accountId,
}) => {
  const [amount, setAmount] = useState<string>('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  if (!isOpen) return null;

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

      const request: MoneyRequest = { amount: numericAmount };
      await api.post(`/api/accounts/${accountId}/deposit`, request);

      setSuccess('Depósito realizado com sucesso!');
      setAmount('');

      // Call onSuccess after a short delay to allow user to see the success message
      setTimeout(() => {
        onSuccess();
      }, 1500);
    } catch (err: unknown) {
      console.error('Error during deposit:', err);
      setError(handleApiError(err));
    } finally {
      setIsLoading(false);
    }
  };

  return (
     <div className="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center z-50 p-4">
       <div className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6 relative">
         <button
           onClick={onClose}
           className="absolute top-4 right-4 text-gray-400 hover:text-gray-600 transition-colors duration-150"
         >
           <X className="h-6 w-6" />
         </button>

         <div className="flex items-center mb-4">
           <div className="bg-green-100 p-2 rounded-lg">
             <ArrowUpCircle className="h-8 w-8 text-green-600" />
           </div>
           <h2 className="text-xl font-semibold text-gray-900 ml-3">Depositar Dinheiro</h2>
         </div>

         <p className="text-gray-600 mb-6">
           Insira o valor que deseja depositar em sua conta.
         </p>

         {error && (
           <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-4 rounded-lg">
             <div className="flex">
               <div className="flex-shrink-0">
                 <AlertCircle className="h-5 w-5 text-red-400" />
               </div>
               <div className="ml-3">
                 <h3 className="text-sm font-medium text-red-800">Erro</h3>
                 <p className="text-sm text-red-700 mt-1">{error}</p>
               </div>
             </div>
           </div>
         )}

         {success ? (
           <div className="bg-green-50 border-l-4 border-green-400 p-4 mb-4 rounded-lg">
             <div className="flex">
               <div className="flex-shrink-0">
                 <CheckCircle2 className="h-5 w-5 text-green-400" />
               </div>
               <div className="ml-3">
                 <h3 className="text-sm font-medium text-green-800">Sucesso!</h3>
                 <p className="text-sm text-green-700 mt-1">{success}</p>
               </div>
             </div>
           </div>
         ) : (
           <form onSubmit={handleSubmit}>
             <div className="mb-6">
               <label htmlFor="amount" className="block text-sm font-medium text-gray-700 mb-2">
                 Valor do Depósito
               </label>
               <div className="relative rounded-lg shadow-sm">
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
                   className="focus:ring-indigo-500 focus:border-indigo-500 block w-full pl-10 pr-16 sm:text-sm border-gray-300 rounded-lg transition duration-150 ease-in-out"
                   placeholder="0.00"
                   required
                 />
                 <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                   <span className="text-gray-500 sm:text-sm font-medium">BRL</span>
                 </div>
               </div>
             </div>

             <div className="flex justify-end space-x-3">
               <button
                 type="button"
                 onClick={onClose}
                 className="px-4 py-2.5 border border-gray-300 rounded-lg shadow-sm text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out"
               >
                 Cancelar
               </button>
               <button
                 type="submit"
                 disabled={isLoading}
                 className="px-6 py-2.5 border border-transparent text-sm font-medium rounded-lg shadow-sm text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 transition duration-150 ease-in-out disabled:opacity-50 disabled:cursor-not-allowed"
               >
                 {isLoading ? (
                   <>
                     <span className="animate-spin inline-block h-4 w-4 border-t-2 border-b-2 border-white rounded-full mr-2"></span>
                     Processando...
                   </>
                 ) : (
                   'Depositar'
                 )}
               </button>
             </div>
           </form>
         )}
      </div>
    </div>
  );
};

export default DepositModal;