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

export interface WorkflowJob {
  job_id: string;
  status: 'running' | 'completed' | 'failed';
  result?: Record<string, unknown>;
  error?: string;
}
