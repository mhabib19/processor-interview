export interface Transaction {
  id: string;
  cardNumber: string; // This is already masked from the backend
  cardType: string;
  amount: number;
  timestamp: string;
  isValid: boolean;
  rejectionReason?: string;
  createdAt: string;
  updatedAt: string;
}

export interface FileUploadResponse {
  success: boolean;
  fileId?: string;
  errorMessage?: string;
  fileName: string;
  fileSize: number;
  fileType?: string;
}

export interface ProcessingStatusResponse {
  success: boolean;
  status: 'pending' | 'processing' | 'completed' | 'failed';
  totalRecords: number;
  validRecords: number;
  rejectedRecords: number;
  errorMessage?: string;
  processingTime?: string;
  startedAt?: string;
  completedAt?: string;
}

export interface TransactionListResponse {
  transactions: Transaction[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ReportResponse {
  summary: {
    totalCount: number;
    totalAmount: number;
    validTransactions: number;
    rejectedTransactions: number;
  };
  transactions: Transaction[];
}

export interface CardTypeDistribution {
  cardType: string;
  count: number;
  percentage: number;
}

export interface DashboardStats {
  totalTransactions: number;
  validTransactions: number;
  invalidTransactions: number;
  totalAmount: number;
  averageAmount: number;
  cardTypeDistribution: CardTypeDistribution[];
  recentTransactions: Transaction[];
}

