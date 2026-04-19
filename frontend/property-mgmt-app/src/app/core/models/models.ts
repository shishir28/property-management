export interface Property {
  id: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  unitCount: number;
}

export interface Tenant {
  id: string;
  fullName: string;
  email: string;
  phone?: string;
  status: string;
}

export interface Lease {
  id: string;
  propertyId: string;
  tenantId: string;
  unitNumber: string;
  startDate: string;
  endDate: string;
  monthlyRent: number;
  securityDeposit: number;
  status: string;
}

export interface MaintenanceRequest {
  id: string;
  leaseId: string;
  title: string;
  description: string;
  priority: string;
  status: string;
  assignedTo?: string;
  reportedAt: string;
  resolvedAt?: string;
}

export interface Payment {
  id: string;
  leaseId: string;
  amount: number;
  dueDate: string;
  paidDate?: string;
  status: string;
  paymentMethod?: string;
}

export interface Inspection {
  id: string;
  propertyId: string;
  leaseId?: string | null;
  type: string;
  status: string;
  scheduledAt: string;
  completedAt?: string | null;
  notes?: string | null;
}

export interface WorkflowLaunchResponse {
  job_id: string;
  status: string;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface WorkflowJob {
  status: 'running' | 'completed' | 'failed';
  result?: Record<string, unknown>;
  error?: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  displayName: string;
  username: string;
  role: string;
}
