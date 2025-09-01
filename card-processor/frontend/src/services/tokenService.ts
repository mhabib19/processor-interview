import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

export interface TokenResponse {
  token: string;
  expiresAt: string;
}

class TokenService {
  private token: string | null = null;
  private expiresAt: Date | null = null;

  // Get token from localStorage on initialization
  constructor() {
    this.loadTokenFromStorage();
  }

  private loadTokenFromStorage(): void {
    const storedToken = localStorage.getItem('jwt_token');
    const storedExpiresAt = localStorage.getItem('jwt_expires_at');
    
    if (storedToken && storedExpiresAt) {
      this.token = storedToken;
      this.expiresAt = new Date(storedExpiresAt);
    }
  }

  private saveTokenToStorage(token: string, expiresAt: string): void {
    localStorage.setItem('jwt_token', token);
    localStorage.setItem('jwt_expires_at', expiresAt);
    this.token = token;
    this.expiresAt = new Date(expiresAt);
  }

  private clearTokenFromStorage(): void {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('jwt_expires_at');
    this.token = null;
    this.expiresAt = null;
  }

  private isTokenExpired(): boolean {
    if (!this.token || !this.expiresAt) {
      return true;
    }
    
    // Add 5 minute buffer before expiration
    const bufferTime = 5 * 60 * 1000; // 5 minutes in milliseconds
    return this.expiresAt.getTime() - bufferTime < Date.now();
  }

  async getToken(): Promise<string> {
    // If we have a valid token, return it
    if (this.token && !this.isTokenExpired()) {
      return this.token;
    }

    // Token is expired or doesn't exist, get a new one
    try {
      const response = await axios.get<TokenResponse>(`${API_BASE_URL}/token`);
      this.saveTokenToStorage(response.data.token, response.data.expiresAt);
      return response.data.token;
    } catch (error) {
      console.error('Failed to get token:', error);
      this.clearTokenFromStorage();
      throw new Error('Failed to get authentication token');
    }
  }

  getStoredToken(): string | null {
    return this.token && !this.isTokenExpired() ? this.token : null;
  }

  clearToken(): void {
    this.clearTokenFromStorage();
  }
}

// Export a singleton instance
export const tokenService = new TokenService();
