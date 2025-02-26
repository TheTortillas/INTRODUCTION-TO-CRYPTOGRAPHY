import { Injectable } from '@angular/core';
import { TokenClaims } from '../../interfaces/TokenClaims.interface';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class StorageService {
  private isBrowser(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }

  getItem(key: string): string | null {
    if (this.isBrowser()) {
      return localStorage.getItem(key);
    }
    return null;
  }

  setItem(key: string, value: string): void {
    if (this.isBrowser()) {
      localStorage.setItem(key, value);
    }
  }

  removeItem(key: string): void {
    if (this.isBrowser()) {
      localStorage.removeItem(key);
    }
  }

  clear(): void {
    if (this.isBrowser()) {
      localStorage.clear();
    }
  }

  getTokenClaims(token: string): TokenClaims | null {
    try {
      const decoded: any = jwtDecode(token);
      return {
        id: decoded.id,
        firstname: decoded.firstname,
        lastname: decoded.lastname,
        secondlastname: decoded.secondlastname,
        email: decoded.email,
      };
    } catch (error) {
      console.error('Token inválido:', error);
      return null;
    }
  }

  isTokenValid(token: string): boolean {
    try {
      const decoded: any = jwtDecode(token);
      const currentTime = Math.floor(Date.now() / 1000);
      return decoded.exp > currentTime;
    } catch (error) {
      console.error('Error al decodificar el token:', error);
      return false;
    }
  }
}
