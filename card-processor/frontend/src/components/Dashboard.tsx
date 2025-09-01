import React, { useState, useEffect } from 'react';
import { reportApi } from '../services/api';
import { DashboardStats } from '../types';

const Dashboard: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [dateRange, setDateRange] = useState('7d'); // 7d, 30d, 90d, custom
  const [customDateRange, setCustomDateRange] = useState({
    startDate: '',
    endDate: ''
  });
  const [showCustomRange, setShowCustomRange] = useState(false);

  useEffect(() => {
    fetchDashboardStats();
  }, [dateRange, customDateRange]);

  const fetchDashboardStats = async () => {
    setLoading(true);
    setError(null);

    try {
      let dateRangeParam = dateRange;
      
      // If custom range is selected but dates are not set, don't make the API call yet
      if (dateRange === 'custom') {
        if (!customDateRange.startDate || !customDateRange.endDate) {
          setLoading(false);
          return; // Don't make API call until both dates are selected
        }
        
        // Validate that end date is not before start date
        if (new Date(customDateRange.endDate) < new Date(customDateRange.startDate)) {
          setError('End date must be after start date');
          setLoading(false);
          return;
        }
        
        dateRangeParam = `${customDateRange.startDate}|${customDateRange.endDate}`;
      }
      
      const response = await reportApi.getDashboardStats(dateRangeParam);
      setStats(response);
    } catch (err) {
      setError('Failed to fetch dashboard statistics');
      console.error('Dashboard error:', err);
    } finally {
      setLoading(false);
    }
  };

  const getCardTypeColor = (cardType: string): string => {
    switch (cardType.toLowerCase()) {
      case 'visa': return 'bg-blue-500';
      case 'mastercard': return 'bg-red-500';
      case 'americanexpress': return 'bg-green-500';
      case 'discover': return 'bg-orange-500';
      default: return 'bg-gray-500';
    }
  };

  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  if (loading) {
    return (
      <div className="card animate-fade-in">
        <div className="flex items-center justify-center py-12">
          <svg className="animate-spin h-8 w-8 text-primary-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <span className="ml-3 text-lg text-gray-600">Loading dashboard...</span>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="card animate-fade-in">
        <div className="p-6 bg-error-50 border border-error-200 rounded-lg">
          <p className="text-error-600">{error}</p>
        </div>
      </div>
    );
  }

  // Show message when custom range is selected but dates aren't set
  if (dateRange === 'custom' && (!customDateRange.startDate || !customDateRange.endDate)) {
    return (
      <div className="space-y-6 animate-fade-in">
        {/* Header */}
        <div className="flex justify-between items-center">
          <h2 className="text-2xl font-bold text-gray-800">Dashboard</h2>
          <div className="flex items-center space-x-4">
            <select
              value={dateRange}
              onChange={(e) => {
                setDateRange(e.target.value);
                setShowCustomRange(e.target.value === 'custom');
              }}
              className="input-field w-auto"
            >
              <option value="7d">Last 7 days</option>
              <option value="30d">Last 30 days</option>
              <option value="90d">Last 90 days</option>
              <option value="custom">Custom Range</option>
            </select>
            
            {showCustomRange && (
              <div className="flex items-center space-x-2">
                <input
                  type="date"
                  value={customDateRange.startDate}
                  onChange={(e) => setCustomDateRange(prev => ({ ...prev, startDate: e.target.value }))}
                  className="input-field w-auto"
                  placeholder="Start Date"
                />
                <span className="text-gray-500">to</span>
                <input
                  type="date"
                  value={customDateRange.endDate}
                  onChange={(e) => setCustomDateRange(prev => ({ ...prev, endDate: e.target.value }))}
                  className="input-field w-auto"
                  placeholder="End Date"
                />
              </div>
            )}
          </div>
        </div>

        {/* Message */}
        <div className="card">
          <div className="p-6 bg-blue-50 border border-blue-200 rounded-lg">
            <p className="text-blue-600">Please select both start and end dates to view dashboard statistics for the custom range.</p>
          </div>
        </div>
      </div>
    );
  }

  if (!stats) {
    return null;
  }

  const successRate = stats.totalTransactions > 0 
    ? ((stats.validTransactions / stats.totalTransactions) * 100).toFixed(1)
    : '0';

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-800">Dashboard</h2>
        <div className="flex items-center space-x-4">
          <select
            value={dateRange}
            onChange={(e) => {
              setDateRange(e.target.value);
              setShowCustomRange(e.target.value === 'custom');
            }}
            className="input-field w-auto"
          >
            <option value="7d">Last 7 days</option>
            <option value="30d">Last 30 days</option>
            <option value="90d">Last 90 days</option>
            <option value="custom">Custom Range</option>
          </select>
          
          {showCustomRange && (
            <div className="flex items-center space-x-2">
              <input
                type="date"
                value={customDateRange.startDate}
                onChange={(e) => setCustomDateRange(prev => ({ ...prev, startDate: e.target.value }))}
                className="input-field w-auto"
                placeholder="Start Date"
              />
              <span className="text-gray-500">to</span>
              <input
                type="date"
                value={customDateRange.endDate}
                onChange={(e) => setCustomDateRange(prev => ({ ...prev, endDate: e.target.value }))}
                className="input-field w-auto"
                placeholder="End Date"
              />
            </div>
          )}
        </div>
      </div>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="card">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <div className="w-8 h-8 bg-primary-100 rounded-lg flex items-center justify-center">
                <svg className="w-5 h-5 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
              </div>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Total Transactions</p>
              <p className="text-2xl font-bold text-gray-900">{stats.totalTransactions.toLocaleString()}</p>
            </div>
          </div>
        </div>

        <div className="card">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <div className="w-8 h-8 bg-success-100 rounded-lg flex items-center justify-center">
                <svg className="w-5 h-5 text-success-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Valid Transactions</p>
              <p className="text-2xl font-bold text-gray-900">{stats.validTransactions.toLocaleString()}</p>
            </div>
          </div>
        </div>

        <div className="card">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <div className="w-8 h-8 bg-error-100 rounded-lg flex items-center justify-center">
                <svg className="w-5 h-5 text-error-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Invalid Transactions</p>
              <p className="text-2xl font-bold text-gray-900">{stats.invalidTransactions.toLocaleString()}</p>
            </div>
          </div>
        </div>

        <div className="card">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <div className="w-8 h-8 bg-green-100 rounded-lg flex items-center justify-center">
                <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
                </svg>
              </div>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-500">Total Amount</p>
              <p className="text-2xl font-bold text-gray-900">{formatCurrency(stats.totalAmount)}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Success Rate and Average Amount */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="card">
          <h3 className="text-lg font-medium text-gray-800 mb-4">Success Rate</h3>
          <div className="flex items-center">
            <div className="flex-1">
              <div className="w-full bg-gray-200 rounded-full h-3">
                <div 
                  className="bg-success-600 h-3 rounded-full transition-all duration-500"
                  style={{ width: `${successRate}%` }}
                ></div>
              </div>
            </div>
            <span className="ml-4 text-2xl font-bold text-gray-900">{successRate}%</span>
          </div>
          <p className="text-sm text-gray-500 mt-2">
            {stats.validTransactions} out of {stats.totalTransactions} transactions processed successfully
          </p>
        </div>

        <div className="card">
          <h3 className="text-lg font-medium text-gray-800 mb-4">Average Transaction Amount</h3>
          <div className="text-3xl font-bold text-gray-900 mb-2">
            {formatCurrency(stats.averageAmount)}
          </div>
          <p className="text-sm text-gray-500">
            Average amount per transaction
          </p>
        </div>
      </div>

      {/* Card Type Distribution */}
      <div className="card">
        <h3 className="text-lg font-medium text-gray-800 mb-4">Card Type Distribution</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {stats.cardTypeDistribution.map((item) => (
            <div key={item.cardType} className="flex items-center p-3 bg-gray-50 rounded-lg">
              <div className={`w-4 h-4 rounded-full ${getCardTypeColor(item.cardType)} mr-3`}></div>
              <div className="flex-1">
                <p className="text-sm font-medium text-gray-900">{item.cardType}</p>
                <p className="text-xs text-gray-500">{item.count} transactions ({item.percentage.toFixed(1)}%)</p>
              </div>
            </div>
          ))}
        </div>
      </div>



      {/* Recent Transactions */}
      {stats.recentTransactions.length > 0 && (
        <div className="card">
          <h3 className="text-lg font-medium text-gray-800 mb-4">Recent Transactions</h3>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Card Number
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Card Type
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Amount
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Date
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {stats.recentTransactions.map((transaction) => (
                  <tr key={transaction.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-900">
                      {transaction.cardNumber}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getCardTypeColor(transaction.cardType)}`}>
                        {transaction.cardType}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {formatCurrency(transaction.amount)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {transaction.isValid ? (
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-success-100 text-success-800">
                          Valid
                        </span>
                      ) : (
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-error-100 text-error-800">
                          Invalid
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {new Date(transaction.timestamp).toLocaleDateString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default Dashboard;
