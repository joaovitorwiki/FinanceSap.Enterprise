// ============================================================
// FinanceSap.Web - Main Application Component
// ============================================================
// This file configures the application routing with:
// - React Router v6
// - AuthProvider for global state
// - Public and protected routes
// ============================================================

import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import AuthLayout from './components/AuthLayout';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Loans from './pages/Loans';
import LoanApprovals from './pages/Admin/LoanApprovals';

/**
 * Main application component with routing configuration.
 */
const App: React.FC = () => {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<Login />} />

          {/* Protected routes wrapped in AuthLayout */}
          <Route element={
            <ProtectedRoute>
              <AuthLayout />
            </ProtectedRoute>
          }>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/transactions" element={<div>Transactions Page</div>} />
            <Route path="/loans" element={<Loans />} />
            <Route path="/admin/loans" element={<LoanApprovals />} />
          </Route>

          {/* Catch-all route - redirect to dashboard */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
};

export default App;