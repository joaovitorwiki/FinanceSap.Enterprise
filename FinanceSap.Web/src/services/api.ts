// ============================================================
// FinanceSap.Web - API Service with Axios
// ============================================================
// This file contains the core API service with:
// - Axios instance configuration
// - JWT request interceptor
// - Refresh token response interceptor
// - Request queue for handling concurrent 401 errors
// ============================================================

import axios, { AxiosError } from 'axios';
import type { AxiosResponse, InternalAxiosRequestConfig } from 'axios';
import type { AuthResponse, ProblemDetails, RefreshRequest, StoredAuthData, User, Loan, LoanRequest } from '../types';

// Constants
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5153/api';
const AUTH_STORAGE_KEY = 'financesap:auth';
const REFRESH_ENDPOINT = '/auth/refresh';
const LOGIN_ENDPOINT = '/auth/login';

// Create Axios instance
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Queue for failed requests during token refresh
let requestQueue: Array<{
  resolve: (value: AxiosResponse) => void;
  reject: (reason?: any) => void;
  config: InternalAxiosRequestConfig;
}> = [];

// Flag to prevent multiple simultaneous refresh calls
let isRefreshing = false;

/**
 * Saves auth data to localStorage.
 */
const saveAuthData = (data: StoredAuthData): void => {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(data));
};

/**
 * Clears auth data from localStorage.
 */
export const clearAuthData = (): void => {
  localStorage.removeItem(AUTH_STORAGE_KEY);
};

/**
 * Gets auth data from localStorage.
 */
const getAuthData = (): StoredAuthData | null => {
  const data = localStorage.getItem(AUTH_STORAGE_KEY);
  return data ? JSON.parse(data) : null;
};

/**
 * Processes the request queue with the new access token.
 */
const processQueue = (error: AxiosError | null, token: string | null = null): void => {
  requestQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      if (prom.config.headers) {
        prom.config.headers.Authorization = `Bearer ${token}`;
      }
      api(prom.config)
        .then(prom.resolve)
        .catch(prom.reject);
    }
  });

  requestQueue = [];
};

/**
 * Request interceptor to inject the JWT token.
 */
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
    const authData = getAuthData();
    if (authData?.accessToken && config.headers) {
      config.headers.Authorization = `Bearer ${authData.accessToken}`;
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

/**
 * Response interceptor to handle 401 errors and refresh token.
 */
api.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: AxiosError<ProblemDetails>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // If the error is not 401 or it's a refresh request, reject immediately
    if (error.response?.status !== 401 || originalRequest._retry || originalRequest.url?.includes(REFRESH_ENDPOINT)) {
      return Promise.reject(error);
    }

    // If we're already refreshing, add the request to the queue
    if (isRefreshing) {
      return new Promise<AxiosResponse>((resolve, reject) => {
        requestQueue.push({ resolve, reject, config: originalRequest });
      });
    }

    // Set the flag to prevent multiple refresh calls
    isRefreshing = true;
    originalRequest._retry = true;

    try {
      const authData = getAuthData();
      if (!authData) {
        throw new Error('No auth data available');
      }

      // Call refresh endpoint
      const refreshPayload: RefreshRequest = {
        expiredToken: authData.accessToken,
        refreshToken: authData.refreshToken,
      };

      const response = await axios.post<AuthResponse>(`${API_BASE_URL}${REFRESH_ENDPOINT}`, refreshPayload);

      // Update auth data
      const newAuthData: StoredAuthData = {
        accessToken: response.data.accessToken,
        refreshToken: response.data.refreshToken,
        user: response.data.user,
      };

      saveAuthData(newAuthData);

      // Process queued requests with the new token
      processQueue(null, newAuthData.accessToken);

      // Retry the original request
      if (originalRequest.headers) {
        originalRequest.headers.Authorization = `Bearer ${newAuthData.accessToken}`;
      }
      return api(originalRequest);

    } catch (refreshError) {
      // Clear auth data and process queue with error
      clearAuthData();
      processQueue(refreshError as AxiosError);

      // Redirect to login
      window.location.href = '/login';
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);

/**
 * Login function to authenticate the user.
 */
export const login = async (email: string, password: string): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>(LOGIN_ENDPOINT, { email, password });
  const authData: StoredAuthData = {
    accessToken: response.data.accessToken,
    refreshToken: response.data.refreshToken,
    user: response.data.user,
  };
  saveAuthData(authData);
  return response.data;
};

/**
 * Logout function to clear auth data.
 */
export const logout = (): void => {
  clearAuthData();
  window.location.href = '/login';
};

/**
 * Gets the current authenticated user.
 */
export const getCurrentUser = (): User | null => {
  const authData = getAuthData();
  return authData?.user || null;
};

/**
 * Checks if the user is authenticated.
 */
export const isAuthenticated = (): boolean => {
  const authData = getAuthData();
  return !!authData?.accessToken;
};

/**
 * Gets the current user's loans.
 */
export const getMyLoans = async (): Promise<Loan[]> => {
  const response = await api.get<Loan[]>('/loans/my-loans');
  return response.data;
};

/**
 * Requests a new loan.
 */
export const requestLoan = async (loanData: LoanRequest): Promise<Loan> => {
  const response = await api.post<Loan>('/loans/request', loanData);
  return response.data;
};

/**
 * Gets all pending loans (for admin).
 */
export const getPendingLoans = async (): Promise<Loan[]> => {
  const response = await api.get<Loan[]>('/loans/pending');
  return response.data;
};

/**
 * Approves a loan.
 */
export const approveLoan = async (loanId: string): Promise<Loan> => {
  const response = await api.post<Loan>(`/loans/${loanId}/approve`);
  return response.data;
};

/**
 * Rejects a loan.
 */
export const rejectLoan = async (loanId: string, reason?: string): Promise<Loan> => {
  const response = await api.post<Loan>(`/loans/${loanId}/reject`, { reason });
  return response.data;
};

export default api;
