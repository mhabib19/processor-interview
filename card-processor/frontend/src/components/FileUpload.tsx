import React, { useState, useRef, useEffect } from 'react';
import { fileUploadApi, api } from '../services/api';
import { FileUploadResponse, ProcessingStatusResponse } from '../types';

const FileUpload: React.FC = () => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isRealData, setIsRealData] = useState(false);
  const [uploadResponse, setUploadResponse] = useState<FileUploadResponse | null>(null);
  const [processingStatus, setProcessingStatus] = useState<ProcessingStatusResponse | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [maxFileSizeMB, setMaxFileSizeMB] = useState(10);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const statusIntervalRef = useRef<NodeJS.Timeout | null>(null);

  // Fetch max file size from backend on component mount
  useEffect(() => {
    const fetchMaxSize = async () => {
      try {
        const response = await api.get('/fileupload/max-size');
        setMaxFileSizeMB(response.data);
      } catch (error: any) {
        console.warn('Using default file size limit');
      }
    };
    fetchMaxSize();
  }, []);

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setSelectedFile(file);
      setError(null);
      setUploadResponse(null);
      setProcessingStatus(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      setError('Please select a file first');
      return;
    }

    setIsUploading(true);
    setError(null);

    try {
      const response = await fileUploadApi.uploadFile(selectedFile, isRealData);
      setUploadResponse(response);

      if (response.success && response.fileId) {
        // Start processing immediately after upload
        await handleProcessFile(response.fileId);
      } else {
        setError(response.errorMessage || 'Upload failed');
      }
    } catch (err) {
      setError('Upload failed. Please try again.');
      console.error('Upload error:', err);
    } finally {
      setIsUploading(false);
    }
  };

  const handleProcessFile = async (fileId: string) => {
    setIsProcessing(true);
    setError(null);

    try {
      // Start processing
      const processResponse = await fileUploadApi.processFile(fileId);
      setProcessingStatus(processResponse);

      if (processResponse.status === 'processing' || processResponse.status === 'completed') {
        // Poll for status updates
        startStatusPolling(fileId);
      } else if (processResponse.status === 'failed') {
        setError(processResponse.errorMessage || 'Processing failed');
        setIsProcessing(false);
      }
    } catch (err) {
      setError('Processing failed. Please try again.');
      console.error('Processing error:', err);
      setIsProcessing(false);
    }
  };

  const startStatusPolling = (fileId: string) => {
    const pollStatus = async () => {
      try {
        const status = await fileUploadApi.getProcessingStatus(fileId);
        setProcessingStatus(status);

        if (status.status === 'completed' || status.status === 'failed') {
          if (statusIntervalRef.current) {
            clearInterval(statusIntervalRef.current);
            statusIntervalRef.current = null;
          }
          setIsProcessing(false);
        }
      } catch (err) {
        console.error('Status polling error:', err);
      }
    };

    // Poll every 2 seconds
    statusIntervalRef.current = setInterval(pollStatus, 2000);
  };

  const handleReset = () => {
    setSelectedFile(null);
    setIsRealData(false);
    setUploadResponse(null);
    setProcessingStatus(null);
    setError(null);
    setIsUploading(false);
    setIsProcessing(false);
    
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }

    if (statusIntervalRef.current) {
      clearInterval(statusIntervalRef.current);
      statusIntervalRef.current = null;
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getStatusColor = (status: string): string => {
    switch (status) {
      case 'pending': return 'text-yellow-600 bg-yellow-100';
      case 'processing': return 'text-blue-600 bg-blue-100';
      case 'completed': return 'text-success-600 bg-success-100';
      case 'failed': return 'text-error-600 bg-error-100';
      default: return 'text-gray-600 bg-gray-100';
    }
  };

  return (
    <div className="max-w-2xl mx-auto p-6 card animate-fade-in">
      <h2 className="text-2xl font-bold mb-6 text-gray-800">Upload Transaction File</h2>
      
      {/* File Selection */}
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Select File (CSV, JSON, or XML)
        </label>
        <input
          ref={fileInputRef}
          type="file"
          accept=".csv,.json,.xml"
          onChange={handleFileSelect}
          className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-medium file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
        />
        <p className="mt-1 text-sm text-gray-500">
          Maximum file size: {maxFileSizeMB}MB
        </p>
      </div>

      {/* Real Data Checkbox */}
      <div className="mb-6">
        <label className="flex items-center">
          <input
            type="checkbox"
            checked={isRealData}
            onChange={(e) => setIsRealData(e.target.checked)}
            className="rounded border-gray-300 text-primary-600 shadow-sm focus:border-primary-300 focus:ring focus:ring-primary-200 focus:ring-opacity-50"
          />
          <span className="ml-2 text-sm text-gray-700">
            Real credit card data (enables strict Luhn algorithm validation)
          </span>
        </label>
        <p className="mt-1 text-sm text-gray-500">
          Uncheck for test data (relaxed validation)
        </p>
      </div>

      {/* Selected File Info */}
      {selectedFile && (
        <div className="mb-6 p-4 bg-gray-50 rounded-lg animate-slide-up">
          <h3 className="font-medium text-gray-800 mb-2">Selected File:</h3>
          <div className="space-y-1 text-sm text-gray-600">
            <p><strong>Name:</strong> {selectedFile.name}</p>
            <p><strong>Size:</strong> {formatFileSize(selectedFile.size)}</p>
            <p><strong>Type:</strong> {selectedFile.type || 'Unknown'}</p>
          </div>
        </div>
      )}

      {/* Upload Response */}
      {uploadResponse && (
        <div className="mb-6 p-4 bg-primary-50 rounded-lg animate-slide-up">
          <h3 className="font-medium text-primary-800 mb-2">Upload Result:</h3>
          <div className="space-y-1 text-sm text-primary-700">
            <p><strong>File ID:</strong> {uploadResponse.fileId}</p>
            <p><strong>Status:</strong> {uploadResponse.success ? 'Success' : 'Failed'}</p>
          </div>
          {uploadResponse.errorMessage && (
            <p className="text-sm text-error-600 mt-2">
              <strong>Error:</strong> {uploadResponse.errorMessage}
            </p>
          )}
        </div>
      )}

      {/* Processing Status */}
      {processingStatus && (
        <div className="mb-6 p-4 bg-gray-50 rounded-lg animate-slide-up">
          <h3 className="font-medium text-gray-800 mb-2">Processing Status:</h3>
          <div className={`inline-block px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(processingStatus.status)}`}>
            {processingStatus.status.charAt(0).toUpperCase() + processingStatus.status.slice(1)}
          </div>
          
          {processingStatus.status === 'processing' && (
            <div className="mt-3">
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div className="bg-primary-600 h-2 rounded-full animate-pulse-slow" style={{ width: '100%' }}></div>
              </div>
              <p className="text-sm text-gray-600 mt-2">Processing transactions...</p>
            </div>
          )}

          {processingStatus.status === 'completed' && (
            <div className="mt-3 space-y-2">
              <p className="text-sm text-gray-600">
                <strong>Total Records:</strong> {processingStatus.totalRecords}
              </p>
              <p className="text-sm text-success-600">
                <strong>Valid Records:</strong> {processingStatus.validRecords}
              </p>
              <p className="text-sm text-error-600">
                <strong>Rejected Records:</strong> {processingStatus.rejectedRecords}
              </p>
              {processingStatus.processingTime && (
                <p className="text-sm text-gray-600">
                  <strong>Processing Time:</strong> {processingStatus.processingTime}
                </p>
              )}
            </div>
          )}

          {processingStatus.status === 'failed' && (
            <div className="mt-3">
              <p className="text-sm text-error-600">
                <strong>Error:</strong> {processingStatus.errorMessage}
              </p>
            </div>
          )}
        </div>
      )}

      {/* Error Display */}
      {error && (
        <div className="mb-6 p-4 bg-error-50 border border-error-200 rounded-lg animate-slide-up">
          <p className="text-sm text-error-600">{error}</p>
        </div>
      )}

      {/* Action Buttons */}
      <div className="flex gap-4">
        <button
          onClick={handleUpload}
          disabled={!selectedFile || isUploading || isProcessing}
          className={`btn-primary ${(!selectedFile || isUploading || isProcessing) ? 'opacity-50 cursor-not-allowed' : ''}`}
        >
          {isUploading ? (
            <span className="flex items-center">
              <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Uploading...
            </span>
          ) : (
            'Upload & Process'
          )}
        </button>
        
        <button
          onClick={handleReset}
          className="btn-secondary"
        >
          Reset
        </button>
      </div>
    </div>
  );
};

export default FileUpload;
