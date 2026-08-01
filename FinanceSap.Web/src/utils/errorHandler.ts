/**
 * Utility function to handle Axios errors and extract ProblemDetails messages
 * @param error - The error object from Axios catch
 * @returns A user-friendly error message
 */
export const handleApiError = (error: unknown): string => {
  if (!error) {
    return 'Ocorreu um erro desconhecido. Por favor, tente novamente.';
  }

  // Axios error structure
  if (typeof error === 'object' && error && 'response' in error) {
    const axiosError = error as {
      response?: {
        data?: {
          detail?: string;
          title?: string;
          [key: string]: unknown;
        };
        status?: number;
      };
    };

    if (axiosError.response?.data?.detail) {
      return axiosError.response.data.detail;
    }

    if (axiosError.response?.data?.title) {
      return axiosError.response.data.title;
    }

    if (axiosError.response?.status === 400) {
      return 'Requisição inválida. Por favor, verifique os dados informados.';
    }

    if (axiosError.response?.status === 401) {
      return 'Sessão expirada. Por favor, faça login novamente.';
    }

    if (axiosError.response?.status === 403) {
      return 'Você não tem permissão para realizar esta operação.';
    }

    if (axiosError.response?.status === 404) {
      return 'Recurso não encontrado.';
    }

    if (axiosError.response?.status && axiosError.response.status >= 500) {
      return 'Erro no servidor. Por favor, tente novamente mais tarde.';
    }
  }

  // Generic error
  if (error instanceof Error) {
    return error.message || 'Ocorreu um erro desconhecido.';
  }

  return 'Ocorreu um erro desconhecido. Por favor, tente novamente.';
};