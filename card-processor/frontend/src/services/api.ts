import axios from 'axios';
import { FileUploadResponse, ProcessingStatusResponse, TransactionListResponse, ReportResponse, DashboardStats, Transaction } from '../types';
import { tokenService } from './tokenService';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor to include JWT token
api.interceptors.request.use(
  async (config) => {
    try {
      const token = await tokenService.getToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    } catch (error) {
      console.error('Failed to get token for request:', error);
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add response interceptor to handle 401 errors
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Token might be expired, clear it and try to get a new one
      tokenService.clearToken();
      try {
        const token = await tokenService.getToken();
        if (token) {
          // Retry the original request with new token
          const originalRequest = error.config;
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        console.error('Failed to refresh token:', refreshError);
      }
    }
    return Promise.reject(error);
  }
);

export const fileUploadApi = {
  uploadFile: async (file: File, isRealData: boolean = false): Promise<FileUploadResponse> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('isRealData', isRealData.toString());
    
    const response = await api.post('/fileupload/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  processFile: async (fileId: string): Promise<ProcessingStatusResponse> => {
    const response = await api.post(`/fileupload/process/${fileId}`);
    return response.data;
  },

  getProcessingStatus: async (fileId: string): Promise<ProcessingStatusResponse> => {
    const response = await api.get(`/fileupload/status/${fileId}`);
    return response.data;
  },

  deleteFile: async (fileId: string): Promise<void> => {
    await api.delete(`/fileupload/${fileId}`);
  },
};

export const transactionApi = {
  getTransactions: async (params?: {
    page?: number;
    pageSize?: number;
    cardType?: string;
    isValid?: boolean;
    dateFrom?: string;
    dateTo?: string;
  }): Promise<TransactionListResponse> => {
    const response = await api.get('/transaction', { params });
    return response.data;
  },

  getRejectedTransactions: async (params?: {
    page?: number;
    pageSize?: number;
  }): Promise<TransactionListResponse> => {
    const response = await api.get('/transaction/rejected', { params });
    return response.data;
  },

  getTransactionById: async (id: string): Promise<Transaction> => {
    const response = await api.get(`/transaction/${id}`);
    return response.data;
  },
};

export const reportApi = {
  getReportByCard: async (): Promise<ReportResponse> => {
    const response = await api.get('/report/by-card');
    return response.data;
  },

  getReportByCardType: async (cardType: string): Promise<ReportResponse> => {
    const response = await api.get('/report/by-card-type', {
      params: { cardType }
    });
    return response.data;
  },

  getReportByDay: async (startDate: string, endDate: string): Promise<ReportResponse> => {
    const response = await api.get('/report/by-day', {
      params: { startDate, endDate }
    });
    return response.data;
  },

  getRejectedReport: async (): Promise<ReportResponse> => {
    const response = await api.get('/report/rejected');
    return response.data;
  },
  
  getDashboardStats: async (dateRange: string): Promise<DashboardStats> => {
    const response = await api.get('/report/dashboard', {
      params: { dateRange }
    });
    return response.data;
  },
};

