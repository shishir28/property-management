import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Inspection,
  Lease,
  MaintenanceRequest,
  PagedResponse,
  Payment,
  Property,
  Tenant,
  WorkflowJob,
  WorkflowLaunchResponse
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = '/api';
  private pageQuery(page: number, pageSize: number) {
    return `page=${page}&pageSize=${pageSize}`;
  }

  // Properties
  getProperties(): Observable<Property[]> { return this.http.get<Property[]>(`${this.base}/properties`); }
  getPropertiesPage(page = 1, pageSize = 10): Observable<PagedResponse<Property>> {
    return this.http.get<PagedResponse<Property>>(`${this.base}/properties/paged?${this.pageQuery(page, pageSize)}`);
  }
  createProperty(body: Omit<Property, 'id'>): Observable<{ id: string }> { return this.http.post<{ id: string }>(`${this.base}/properties`, body); }

  // Tenants
  getTenants(): Observable<Tenant[]> { return this.http.get<Tenant[]>(`${this.base}/tenants`); }
  getTenantsPage(page = 1, pageSize = 10): Observable<PagedResponse<Tenant>> {
    return this.http.get<PagedResponse<Tenant>>(`${this.base}/tenants/paged?${this.pageQuery(page, pageSize)}`);
  }
  createTenant(body: { firstName: string; lastName: string; email: string; phone?: string }): Observable<{ id: string }> { return this.http.post<{ id: string }>(`${this.base}/tenants`, body); }
  activateTenant(id: string): Observable<void> { return this.http.post<void>(`${this.base}/tenants/${id}/activate`, {}); }

  // Leases
  getLeases(): Observable<Lease[]> { return this.http.get<Lease[]>(`${this.base}/leases`); }
  getLeasesPage(page = 1, pageSize = 10): Observable<PagedResponse<Lease>> {
    return this.http.get<PagedResponse<Lease>>(`${this.base}/leases/paged?${this.pageQuery(page, pageSize)}`);
  }
  getExpiringLeases(withinDays = 60): Observable<Lease[]> { return this.http.get<Lease[]>(`${this.base}/leases/expiring?withinDays=${withinDays}`); }
  getExpiringLeasesPage(page = 1, pageSize = 10, withinDays = 60): Observable<PagedResponse<Lease>> {
    return this.http.get<PagedResponse<Lease>>(
      `${this.base}/leases/expiring/paged?withinDays=${withinDays}&${this.pageQuery(page, pageSize)}`
    );
  }
  createLease(body: Omit<Lease, 'id' | 'status'>): Observable<{ id: string }> { return this.http.post<{ id: string }>(`${this.base}/leases`, body); }
  activateLease(id: string): Observable<void> { return this.http.post<void>(`${this.base}/leases/${id}/activate`, {}); }
  triggerRenewal(id: string): Observable<void> { return this.http.post<void>(`${this.base}/leases/${id}/trigger-renewal-workflow`, {}); }

  // Maintenance
  getMaintenance(): Observable<MaintenanceRequest[]> { return this.http.get<MaintenanceRequest[]>(`${this.base}/maintenance/open/routine`); }
  getMaintenancePage(priority = 'Routine', page = 1, pageSize = 10): Observable<PagedResponse<MaintenanceRequest>> {
    return this.http.get<PagedResponse<MaintenanceRequest>>(
      `${this.base}/maintenance/open/${priority}/paged?${this.pageQuery(page, pageSize)}`
    );
  }
  getOpenMaintenance(priority: string): Observable<MaintenanceRequest[]> { return this.http.get<MaintenanceRequest[]>(`${this.base}/maintenance/open/${priority}`); }
  getMaintenanceByLease(leaseId: string): Observable<MaintenanceRequest[]> { return this.http.get<MaintenanceRequest[]>(`${this.base}/maintenance/by-lease/${leaseId}`); }
  createMaintenance(body: Omit<MaintenanceRequest, 'id' | 'status' | 'reportedAt' | 'resolvedAt'>): Observable<{ id: string }> { return this.http.post<{ id: string }>(`${this.base}/maintenance`, body); }
  resolveMaintenance(id: string): Observable<void> { return this.http.post<void>(`${this.base}/maintenance/${id}/resolve`, {}); }

  // Payments
  getOverduePayments(): Observable<Payment[]> { return this.http.get<Payment[]>(`${this.base}/payments/overdue`); }
  getOverduePaymentsPage(page = 1, pageSize = 10): Observable<PagedResponse<Payment>> {
    return this.http.get<PagedResponse<Payment>>(`${this.base}/payments/overdue/paged?${this.pageQuery(page, pageSize)}`);
  }
  getPaymentsByLease(leaseId: string): Observable<Payment[]> { return this.http.get<Payment[]>(`${this.base}/payments/by-lease/${leaseId}`); }
  createPayment(body: { leaseId: string; amount: number; dueDate: string }): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.base}/payments`, body);
  }
  runRentCollection(): Observable<void> { return this.http.post<void>(`${this.base}/payments/run-collection`, {}); }
  markPaid(id: string, paymentMethod: string): Observable<void> { return this.http.post<void>(`${this.base}/payments/${id}/pay`, { paymentMethod }); }

  // Inspections
  getInspections(): Observable<Inspection[]> { return this.http.get<Inspection[]>(`${this.base}/inspections`); }
  getInspectionsPage(page = 1, pageSize = 10): Observable<PagedResponse<Inspection>> {
    return this.http.get<PagedResponse<Inspection>>(`${this.base}/inspections/paged?${this.pageQuery(page, pageSize)}`);
  }
  getScheduledInspections(): Observable<Inspection[]> { return this.http.get<Inspection[]>(`${this.base}/inspections/scheduled`); }
  getScheduledInspectionsPage(page = 1, pageSize = 10): Observable<PagedResponse<Inspection>> {
    return this.http.get<PagedResponse<Inspection>>(
      `${this.base}/inspections/scheduled/paged?${this.pageQuery(page, pageSize)}`
    );
  }

  // Orchestration
  triggerLeaseRenewalWorkflow(leaseId: string): Observable<WorkflowLaunchResponse> {
    return this.http.post<WorkflowLaunchResponse>('/workflows/lease-renewal', { lease_id: leaseId });
  }

  triggerMaintenanceWorkflow(requestId: string): Observable<WorkflowLaunchResponse> {
    return this.http.post<WorkflowLaunchResponse>('/workflows/maintenance', { request_id: requestId });
  }

  triggerRentCollectionWorkflow(): Observable<WorkflowLaunchResponse> {
    return this.http.post<WorkflowLaunchResponse>('/workflows/rent-collection', {});
  }

  triggerOnboardingWorkflow(tenantId: string, leaseId: string): Observable<WorkflowLaunchResponse> {
    return this.http.post<WorkflowLaunchResponse>('/workflows/onboarding', { tenant_id: tenantId, lease_id: leaseId });
  }

  triggerInspectionWorkflow(inspectionId: string): Observable<WorkflowLaunchResponse> {
    return this.http.post<WorkflowLaunchResponse>('/workflows/inspection', { inspection_id: inspectionId });
  }

  triggerSupervisorWorkflow(request: string, context: Record<string, unknown> = {}): Observable<WorkflowLaunchResponse> {
    return this.http.post<WorkflowLaunchResponse>('/workflows/supervisor', { request, context });
  }

  // Workflow jobs
  getWorkflowJob(jobId: string): Observable<WorkflowJob> { return this.http.get<WorkflowJob>(`/jobs/${jobId}`); }
}
