import React, { useState, useEffect } from 'react';
import FileUpload from './components/FileUpload';
import TransactionList from './components/TransactionList';
import Dashboard from './components/Dashboard';
import { tokenService } from './services/tokenService';

type TabType = 'dashboard' | 'upload' | 'transactions';

function App() {
  const [activeTab, setActiveTab] = useState<TabType>('dashboard');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const tabs = [
    { id: 'dashboard' as TabType, name: 'Dashboard', icon: '📊' },
    { id: 'upload' as TabType, name: 'Upload Files', icon: '📁' },
    { id: 'transactions' as TabType, name: 'Transactions', icon: '💳' }
  ];

  // Initialize token on app startup
  useEffect(() => {
    const initializeToken = async () => {
      try {
        setIsLoading(true);
        await tokenService.getToken();
        setError(null);
      } catch (err) {
        setError('Failed to initialize authentication. Please refresh the page.');
        console.error('Token initialization failed:', err);
      } finally {
        setIsLoading(false);
      }
    };

    initializeToken();
  }, []);

  const renderContent = () => {
    switch (activeTab) {
      case 'dashboard':
        return <Dashboard />;
      case 'upload':
        return <FileUpload />;
      case 'transactions':
        return <TransactionList />;
      default:
        return <Dashboard />;
    }
  };

  // Show loading state
  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-500 mx-auto"></div>
          <p className="mt-4 text-gray-600">Initializing...</p>
        </div>
      </div>
    );
  }

  // Show error state
  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="text-red-500 text-lg mb-4">⚠️</div>
          <p className="text-gray-600 mb-4">{error}</p>
          <button 
            onClick={() => window.location.reload()} 
            className="bg-primary-500 text-white px-4 py-2 rounded hover:bg-primary-600"
          >
            Refresh Page
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center">
              <h1 className="text-2xl font-bold text-gray-900">
                Card Processor
              </h1>
            </div>
            <div className="flex items-center space-x-4">
              <span className="text-sm text-gray-500">
                Processing credit card transactions
              </span>
              <span className="text-xs text-green-600 bg-green-100 px-2 py-1 rounded">
                🔐 Secured
              </span>
            </div>
          </div>
        </div>
      </header>

      {/* Navigation Tabs */}
      <nav className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex space-x-8">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`py-4 px-1 border-b-2 font-medium text-sm flex items-center space-x-2 transition-colors duration-200 ${
                  activeTab === tab.id
                    ? 'border-primary-500 text-primary-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                <span>{tab.icon}</span>
                <span>{tab.name}</span>
              </button>
            ))}
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {renderContent()}
      </main>

      {/* Footer */}
      <footer className="bg-white border-t border-gray-200 mt-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="text-center text-sm text-gray-500">
            <p>Card Processor - Secure transaction processing system</p>
            <p className="mt-1">Built with React, TypeScript, and Tailwind CSS</p>
          </div>
        </div>
      </footer>
    </div>
  );
}

export default App;
