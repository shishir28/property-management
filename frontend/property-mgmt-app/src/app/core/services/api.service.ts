import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Lease, MaintenanceRequest, Payment, Property, Tenant } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = '/api';

  // Properties
  getProperties(): Observable<Property[]> { return this.http.get<Property[]>(`${this.base}/properties`); }
  createProperty(body: Omit<Property, 'id'>): Observable<{ id: string }> { return this.http.post<{ id: string }>(`${this.base}/properties`, body); }

  // Tenants
  getTenants(): Observable<Tenant[]> { return this.http.get<Tenant[]>(`${this.base}/tenants`); }
  createTenant(body: { firstName: string; lastName: string; email: string; phone?: string }): Observable<{ id: string }> { return this.http.post<{ id: string }>(`${this.base}/tenants`, body); }
  activateTenant(id: string): Observable<void> { return this.http.post<void>(`${this.base}/tenants/${id}/activate`, {}); }

  // Leases
  getLeases(): Observable<Lease[]> { return this.http.get<Lease[]>(`${this.base}/leases`); }
  getExpiringLeases(withinDays = 60): Observable<Lease[]> { return this.http.get<Lease[]>(`${this.base}/leases/expiring?withinDays=${withinDays}`); }
  createLease(body: Omit<Lease, 'id' | 'status'>): Observable<{ id: string }> { return this.http.post<{ id: string }>(`${this.base}/leases`, body); }
  activateLease(id: string): Observable<void> { return this.http.post<void>(`${this.base}/leases/${id}/activate`, {}); }
  triggerRenewal(id: string): Observable<void> { return this.http.post<void>(`${this.base}/leases/${id}/trigger-renewal-workflow`, {}); }

  // Maintenance
  getMaintenance(): Observable<MaintenanceRequest[]> { return this.http.get<MaintenanceRequest[]>(`${this.base}/maintenance/open/routine`); }
  getMaintenanceByLease(leaseId: string): Observable<MaintenanceRequest[]> { return this.http.get<MaintenanceRequest[]>(`${this.base}/maintenance/by-lease/${leaseId}`); }
  createMaintenance(body: Omit<MaintenanceRequest, 'id' | 'status' | 'reportedAt' | 'resolvedAt'>): Observable<{ id: string }> { return this.http.post<{ id: string }>(`${this.base}/maintenance`, body); }
  resolveMaintenance(id: string): Observable<void> { return this.http.post<void>(`${this.base}/maintenance/${id}/resolve`, {}); }

  // Payments
  getOverduePayments(): Observable<Payment[]> { return this.http.get<Payment[]>(`${this.base}/payments/overdue`); }
  getPaymentsByLease(leaseId: string): Observable<Payment[]> { return this.http.get<Payment[]>(`${this.base}/payments/by-lease/${leaseId}`); }
  runRentCollection(): Observable<void> { return this.http.post<void>(`${this.base}/payments/run-collection`, {}); }
  markPaid(id: string, paymentMethod: string): Observable<void> { return this.http.post<void>(`${this.base}/payments/${id}/pay`, { paymentMethod }); }
}
