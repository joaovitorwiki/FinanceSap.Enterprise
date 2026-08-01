// ============================================================
// FinanceSap.Web - Login Page
// ============================================================
// This page provides a secure login interface with:
// - Email and password fields
// - Form validation
// - Error handling for RFC 7807 ProblemDetails responses
// - Integration with AuthContext
// - Modern Tailwind CSS layout
// ============================================================

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import type { ProblemDetails } from '../types';
import { AlertCircle, Loader2 } from 'lucide-react';

/**
 * Login page component with form and error handling.
 */
const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [generalError, setGeneralError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  /**
   * Handles form submission and login process.
   */
   const handleSubmit = async (e: React.FormEvent) => {
     e.preventDefault();
     setIsSubmitting(true);
     setErrors({});
     setGeneralError(null);

     try {
       await login(email, password);
       // Small delay to ensure auth state is updated before navigation
       await new Promise(resolve => setTimeout(resolve, 100));
       navigate('/dashboard', { replace: true });
     } catch (error: any) {
      // Handle RFC 7807 ProblemDetails responses
      if (error.response?.data) {
        const problemDetails: ProblemDetails = error.response.data;

        if (problemDetails.status === 400 && problemDetails.errors) {
          // Handle validation errors
          const validationErrors: Record<string, string> = {};
          Object.keys(problemDetails.errors).forEach((key) => {
            validationErrors[key] = problemDetails.errors?.[key][0] || 'Erro de validação';
          });
          setErrors(validationErrors);
        } else if (problemDetails.status === 401) {
          // Handle authentication errors
          setGeneralError(problemDetails.detail || 'Credenciais inválidas. Por favor, tente novamente.');
        } else {
          // Handle other errors
          setGeneralError(problemDetails.detail || 'Ocorreu um erro ao fazer login. Por favor, tente novamente.');
        }
      } else {
        // Handle network errors or other unexpected errors
        setGeneralError('Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div className="sm:mx-auto sm:w-full sm:max-w-md">
        <div className="text-center">
          <h2 className="text-3xl font-bold text-gray-900">
            FinanceSap
          </h2>
          <p className="mt-2 text-sm text-gray-600">
            Sistema Financeiro Empresarial
          </p>
        </div>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <div className="bg-white py-8 px-4 shadow-xl sm:rounded-2xl sm:px-10">
          <div className="text-center mb-8">
            <h3 className="text-2xl font-semibold text-gray-900">Acesse sua conta</h3>
            <p className="mt-1 text-sm text-gray-500">Gerencie suas finanças com segurança e eficiência</p>
          </div>

          <form className="space-y-6" onSubmit={handleSubmit}>
            {generalError && (
              <div className="rounded-md bg-red-50 p-4 border-l-4 border-red-400">
                <div className="flex">
                  <div className="flex-shrink-0">
                    <AlertCircle className="h-5 w-5 text-red-400" aria-hidden="true" />
                  </div>
                  <div className="ml-3">
                    <h3 className="text-sm font-medium text-red-800">{generalError}</h3>
                  </div>
                </div>
              </div>
            )}

            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700">
                Endereço de e-mail
              </label>
              <div className="mt-1">
                <input
                  id="email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className={`appearance-none block w-full px-4 py-3 border ${
                    errors.email ? 'border-red-300' : 'border-gray-300'
                  } rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150 ease-in-out sm:text-sm`}
                  placeholder="seu@email.com"
                />
                {errors.email && (
                  <p className="mt-2 text-sm text-red-600">{errors.email}</p>
                )}
              </div>
            </div>

            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700">
                Senha
              </label>
              <div className="mt-1 relative">
                <input
                  id="password"
                  name="password"
                  type="password"
                  autoComplete="current-password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className={`appearance-none block w-full px-4 py-3 border ${
                    errors.password ? 'border-red-300' : 'border-gray-300'
                  } rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150 ease-in-out sm:text-sm`}
                  placeholder="••••••••"
                />
                {errors.password && (
                  <p className="mt-2 text-sm text-red-600">{errors.password}</p>
                )}
              </div>
            </div>

            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <input
                  id="remember-me"
                  name="remember-me"
                  type="checkbox"
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                />
                <label htmlFor="remember-me" className="ml-2 block text-sm text-gray-900">
                  Lembrar-me
                </label>
              </div>

              <div className="text-sm">
                <a href="#" className="font-medium text-indigo-600 hover:text-indigo-500 transition duration-150 ease-in-out">
                  Esqueceu sua senha?
                </a>
              </div>
            </div>

            <div>
              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full flex justify-center py-3 px-4 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSubmitting ? (
                  <>
                    <Loader2 className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" />
                    Entrando...
                  </>
                ) : (
                  'Entrar'
                )}
              </button>
            </div>
          </form>

          <div className="mt-6 text-center text-sm text-gray-500">
            <p>Não tem uma conta? <a href="#" className="font-medium text-indigo-600 hover:text-indigo-500">Solicite acesso</a></p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;