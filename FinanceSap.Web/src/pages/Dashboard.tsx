// ============================================================
// FinanceSap.Web - Dashboard Page
// ============================================================
// This is a placeholder dashboard page that:
// - Shows a welcome message
// - Will be expanded in future phases
// ============================================================

import React from 'react';
import { useAuth } from '../contexts/AuthContext';

/**
 * Dashboard page component.
 */
const Dashboard: React.FC = () => {
  const { user, logout } = useAuth();

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex">
              <div className="flex-shrink-0 flex items-center">
                <h1 className="text-xl font-bold text-gray-900">FinanceSap</h1>
              </div>
            </div>
            <div className="flex items-center">
              <span className="text-sm text-gray-700 mr-4">Olá, {user?.name || 'Usuário'}</span>
              <button
                onClick={logout}
                className="px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
              >
                Sair
              </button>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="border-4 border-dashed border-gray-200 rounded-lg p-8 text-center">
            <h2 className="text-2xl font-semibold text-gray-900 mb-4">Bem-vindo ao FinanceSap Dashboard</h2>
            <p className="text-gray-600">Esta é a área protegida do sistema financeiro empresarial.</p>
            <p className="text-gray-500 mt-2">Mais funcionalidades serão adicionadas nas próximas fases.</p>
          </div>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;