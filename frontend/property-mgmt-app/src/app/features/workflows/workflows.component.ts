import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { JsonPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { interval, Subscription, switchMap, takeWhile } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import {
  Inspection,
  Lease,
  MaintenanceRequest,
  WorkflowJob,
  WorkflowLaunchResponse
} from '../../core/models/models';

type WorkflowEntry = {
  id: string;
  label: string;
  workflow: string;
  status: 'running' | 'completed' | 'failed';
  result?: Record<string, unknown>;
  error?: string;
  startedAt: string;
};

@Component({
  selector: 'app-workflows',
  imports: [
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatIconModule,
    MatProgressBarModule,
    MatChipsModule,
    JsonPipe,
    DatePipe
  ],
  template: `
    <section class="hero">
      <div>
        <p class="eyebrow">Workflow Control</p>
        <h2>Agent Workflows</h2>
        <p class="intro">
          Trigger lease renewal, maintenance, rent collection, onboarding, inspection, and supervisor flows
          from one place, then watch each job complete in real time.
        </p>
      </div>
      <button mat-stroked-button type="button" (click)="reloadData()">
        <mat-icon>refresh</mat-icon>
        Refresh Seeded Data
      </button>
    </section>

    <div class="workflow-grid">
      <mat-card class="workflow-card">
        <mat-card-header>
          <mat-icon>autorenew</mat-icon>
          <mat-card-title>Lease Renewal</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>Choose an expiring lease and generate a renewal notice with compliance notes.</p>
          <mat-form-field appearance="outline">
            <mat-label>Expiring Lease</mat-label>
            <mat-select [(ngModel)]="leaseRenewalLeaseId">
              @for (lease of expiringLeases(); track lease.id) {
                <mat-option [value]="lease.id">
                  {{ lease.unitNumber }} · {{ lease.endDate }} · &#36;{{ lease.monthlyRent }}
                </mat-option>
              }
            </mat-select>
          </mat-form-field>
          <button mat-flat-button color="primary" type="button" [disabled]="!leaseRenewalLeaseId" (click)="triggerLeaseRenewal()">
            Run Lease Renewal
          </button>
        </mat-card-content>
      </mat-card>

      <mat-card class="workflow-card">
        <mat-card-header>
          <mat-icon>build_circle</mat-icon>
          <mat-card-title>Maintenance</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>Select an open maintenance request and let the workflow triage and assign it.</p>
          <mat-form-field appearance="outline">
            <mat-label>Maintenance Request</mat-label>
            <mat-select [(ngModel)]="maintenanceRequestId">
              @for (request of maintenanceRequests(); track request.id) {
                <mat-option [value]="request.id">
                  {{ request.priority }} · {{ request.title }}
                </mat-option>
              }
            </mat-select>
          </mat-form-field>
          <button mat-flat-button color="primary" type="button" [disabled]="!maintenanceRequestId" (click)="triggerMaintenance()">
            Run Maintenance
          </button>
        </mat-card-content>
      </mat-card>

      <mat-card class="workflow-card">
        <mat-card-header>
          <mat-icon>payments</mat-icon>
          <mat-card-title>Rent Collection</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>Run the rent collection workflow against the current overdue payments set.</p>
          <p class="secondary">Current overdue payments: {{ overdueLeaseCount() }}</p>
          <button mat-flat-button color="primary" type="button" (click)="triggerRentCollection()">
            Run Rent Collection
          </button>
        </mat-card-content>
      </mat-card>

      <mat-card class="workflow-card">
        <mat-card-header>
          <mat-icon>person_add</mat-icon>
          <mat-card-title>Onboarding</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>Use a draft lease to prepare a welcome message and move-in checklist.</p>
          <mat-form-field appearance="outline">
            <mat-label>Draft Lease</mat-label>
            <mat-select [(ngModel)]="onboardingLeaseId">
              @for (lease of draftLeases(); track lease.id) {
                <mat-option [value]="lease.id">
                  {{ lease.unitNumber }} · tenant {{ shortId(lease.tenantId) }}
                </mat-option>
              }
            </mat-select>
          </mat-form-field>
          <button mat-flat-button color="primary" type="button" [disabled]="!onboardingLeaseId" (click)="triggerOnboarding()">
            Run Onboarding
          </button>
        </mat-card-content>
      </mat-card>

      <mat-card class="workflow-card">
        <mat-card-header>
          <mat-icon>fact_check</mat-icon>
          <mat-card-title>Inspection</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>Pick an inspection with notes to generate findings and possible maintenance tickets.</p>
          <mat-form-field appearance="outline">
            <mat-label>Inspection</mat-label>
            <mat-select [(ngModel)]="inspectionId">
              @for (inspection of actionableInspections(); track inspection.id) {
                <mat-option [value]="inspection.id">
                  {{ inspection.type }} · {{ inspection.status }} · {{ shortId(inspection.id) }}
                </mat-option>
              }
            </mat-select>
          </mat-form-field>
          <button mat-flat-button color="primary" type="button" [disabled]="!inspectionId" (click)="triggerInspection()">
            Run Inspection
          </button>
        </mat-card-content>
      </mat-card>

      <mat-card class="workflow-card workflow-card--wide">
        <mat-card-header>
          <mat-icon>hub</mat-icon>
          <mat-card-title>Supervisor</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <p>Send a natural-language operational request to the supervisor agent and inspect the delegated result.</p>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Supervisor Request</mat-label>
            <textarea matInput rows="4" [(ngModel)]="supervisorPrompt"></textarea>
          </mat-form-field>
          <button mat-flat-button color="primary" type="button" [disabled]="!supervisorPrompt.trim()" (click)="triggerSupervisor()">
            Run Supervisor
          </button>
        </mat-card-content>
      </mat-card>
    </div>

    <section class="jobs-section">
      <div class="jobs-header">
        <div>
          <p class="eyebrow">Live Results</p>
          <h3>Workflow Jobs</h3>
        </div>
        <div class="job-stats">
          <div class="stat-pill stat-pill--running">Running {{ runningCount() }}</div>
          <div class="stat-pill stat-pill--completed">Completed {{ completedCount() }}</div>
          <div class="stat-pill stat-pill--failed">Failed {{ failedJobs().length }}</div>
        </div>
      </div>

      @if (latestFailure(); as failure) {
        <mat-card class="failure-banner" appearance="outlined">
          <mat-card-content>
            <div class="failure-banner__header">
              <div>
                <p class="eyebrow">Needs Attention</p>
                <h4>{{ failure.label }} failed</h4>
              </div>
              <button mat-stroked-button type="button" (click)="dismissJob(failure.id)">
                Dismiss
              </button>
            </div>
            <p class="secondary">Job {{ failure.id }} started {{ failure.startedAt | date:'medium' }}</p>
            <pre class="job-error">{{ failure.error }}</pre>
          </mat-card-content>
        </mat-card>
      }

      @if (!jobs().length) {
        <mat-card class="empty-card">
          <mat-card-content>No workflows started yet. Run one of the cards above to see results appear here.</mat-card-content>
        </mat-card>
      } @else {
        <div class="jobs-list">
          @for (job of jobs(); track job.id) {
            <mat-card class="job-card">
              <mat-card-header>
                <mat-card-title>{{ job.label }}</mat-card-title>
                <mat-chip [class]="chipClass(job.status)">{{ job.status }}</mat-chip>
              </mat-card-header>
              <mat-card-content>
                <p class="secondary">Job ID: {{ job.id }}</p>
                <p class="secondary">Started: {{ job.startedAt | date:'medium' }}</p>
                @if (job.status === 'running') {
                  <mat-progress-bar mode="indeterminate"></mat-progress-bar>
                }
                @if (job.error) {
                  <pre class="job-error">{{ job.error }}</pre>
                }
                @if (job.result) {
                  <pre class="job-result">{{ job.result | json }}</pre>
                }
              </mat-card-content>
            </mat-card>
          }
        </div>
      }
    </section>
  `,
  styles: [`
    :host {
      display: block;
    }

    .hero {
      display: flex;
      justify-content: space-between;
      gap: 24px;
      align-items: start;
      margin-bottom: 24px;
    }

    .eyebrow {
      margin: 0 0 6px;
      text-transform: uppercase;
      letter-spacing: 0.12em;
      font-size: 0.72rem;
      color: #0f766e;
      font-weight: 700;
    }

    h2, h3 {
      margin: 0 0 8px;
    }

    .intro,
    .secondary {
      color: #475569;
    }

    .workflow-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 18px;
      margin-bottom: 28px;
    }

    .workflow-card {
      min-height: 240px;
    }

    .workflow-card--wide {
      grid-column: 1 / -1;
    }

    mat-card-header {
      align-items: center;
      gap: 12px;
      margin-bottom: 8px;
    }

    mat-form-field {
      width: 100%;
      margin-bottom: 12px;
    }

    .full-width {
      width: 100%;
    }

    .jobs-list {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 16px;
    }

    .jobs-header {
      display: flex;
      justify-content: space-between;
      align-items: end;
      gap: 16px;
      margin-bottom: 16px;
    }

    .job-stats {
      display: flex;
      gap: 10px;
      flex-wrap: wrap;
    }

    .stat-pill {
      padding: 8px 12px;
      border-radius: 999px;
      font-size: 0.82rem;
      font-weight: 700;
    }

    .stat-pill--running {
      background: #dbeafe;
      color: #1d4ed8;
    }

    .stat-pill--completed {
      background: #dcfce7;
      color: #166534;
    }

    .stat-pill--failed {
      background: #fee2e2;
      color: #b91c1c;
    }

    .job-card {
      min-height: 220px;
    }

    .failure-banner {
      margin-bottom: 16px;
      border-color: #fca5a5;
      background: linear-gradient(135deg, #fff7f7 0%, #fff1f2 100%);
    }

    .failure-banner__header {
      display: flex;
      justify-content: space-between;
      align-items: start;
      gap: 16px;
    }

    h4 {
      margin: 0 0 6px;
    }

    .job-result,
    .job-error {
      white-space: pre-wrap;
      word-break: break-word;
      padding: 12px;
      border-radius: 12px;
      background: #f8fafc;
      font-size: 0.85rem;
    }

    .job-error {
      background: #fef2f2;
      color: #991b1b;
    }

    .chip-running {
      background: #dbeafe;
      color: #1d4ed8;
    }

    .chip-completed {
      background: #dcfce7;
      color: #166534;
    }

    .chip-failed {
      background: #fee2e2;
      color: #b91c1c;
    }

    .empty-card {
      border: 1px dashed #cbd5e1;
      box-shadow: none;
    }

    @media (max-width: 960px) {
      .workflow-grid,
      .jobs-list {
        grid-template-columns: 1fr;
      }

      .hero,
      .jobs-header,
      .failure-banner__header {
        flex-direction: column;
      }

      .jobs-header,
      .failure-banner__header {
        align-items: stretch;
      }
    }
  `]
})
export class WorkflowsComponent implements OnInit, OnDestroy {
  private readonly api = inject(ApiService);
  private readonly snack = inject(MatSnackBar);
  private readonly subscriptions = new Subscription();

  readonly leases = signal<Lease[]>([]);
  readonly expiringLeaseOptions = signal<Lease[]>([]);
  readonly maintenanceByPriority = signal<Record<string, MaintenanceRequest[]>>({
    Emergency: [],
    Urgent: [],
    Routine: []
  });
  readonly inspections = signal<Inspection[]>([]);
  readonly overdueLeaseCount = signal(0);
  readonly jobs = signal<WorkflowEntry[]>([]);
  readonly runningCount = computed(() => this.jobs().filter(job => job.status === 'running').length);
  readonly completedCount = computed(() => this.jobs().filter(job => job.status === 'completed').length);
  readonly failedJobs = computed(() => this.jobs().filter(job => job.status === 'failed'));
  readonly latestFailure = computed(() => this.failedJobs()[0] ?? null);

  leaseRenewalLeaseId = '';
  maintenanceRequestId = '';
  onboardingLeaseId = '';
  inspectionId = '';
  supervisorPrompt = 'Review operational priorities for expiring leases, overdue payments, and emergency maintenance.';

  readonly expiringLeases = computed(() => this.expiringLeaseOptions());
  readonly draftLeases = computed(() => this.leases().filter(lease => lease.status === 'Draft'));
  readonly maintenanceRequests = computed(() =>
    ['Emergency', 'Urgent', 'Routine'].flatMap(priority => this.maintenanceByPriority()[priority] ?? []));
  readonly actionableInspections = computed(() =>
    this.inspections().filter(inspection => !!inspection.notes || !!inspection.leaseId));

  ngOnInit() {
    this.reloadData();
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  reloadData() {
    this.subscriptions.add(this.api.getExpiringLeases().subscribe(leases => {
      this.expiringLeaseOptions.set(leases);
      if (!this.leaseRenewalLeaseId && leases.length) {
        this.leaseRenewalLeaseId = leases[0].id;
      }
    }));

    this.subscriptions.add(this.api.getLeases().subscribe(leases => {
      const allLeases = new Map(leases.map(lease => [lease.id, lease]));
      const expiring = this.expiringLeaseOptions();
      expiring.forEach(lease => allLeases.set(lease.id, lease));
      const merged = Array.from(allLeases.values());
      this.leases.set(merged);
      if (!this.onboardingLeaseId) {
        const draft = merged.find(lease => lease.status === 'Draft');
        if (draft) {
          this.onboardingLeaseId = draft.id;
        }
      }
    }));

    ['Emergency', 'Urgent', 'Routine'].forEach(priority => {
      this.subscriptions.add(this.api.getOpenMaintenance(priority).subscribe(requests => {
        this.maintenanceByPriority.update(state => ({ ...state, [priority]: requests }));
        if (!this.maintenanceRequestId && requests.length) {
          this.maintenanceRequestId = requests[0].id;
        }
      }));
    });

    this.subscriptions.add(this.api.getInspections().subscribe(inspections => {
      this.inspections.set(inspections);
      if (!this.inspectionId) {
        const actionable = inspections.find(inspection => inspection.notes || inspection.leaseId);
        if (actionable) {
          this.inspectionId = actionable.id;
        }
      }
    }));

    this.subscriptions.add(this.api.getOverduePayments().subscribe(payments => {
      this.overdueLeaseCount.set(payments.length);
    }));
  }

  triggerLeaseRenewal() {
    this.launchWorkflow('lease renewal', this.api.triggerLeaseRenewalWorkflow(this.leaseRenewalLeaseId));
  }

  triggerMaintenance() {
    this.launchWorkflow('maintenance', this.api.triggerMaintenanceWorkflow(this.maintenanceRequestId));
  }

  triggerRentCollection() {
    this.launchWorkflow('rent collection', this.api.triggerRentCollectionWorkflow());
  }

  triggerOnboarding() {
    const lease = this.leases().find(item => item.id === this.onboardingLeaseId);
    if (!lease) {
      this.snack.open('Select a draft lease before running onboarding.', 'OK', { duration: 3000 });
      return;
    }

    this.launchWorkflow(
      'onboarding',
      this.api.triggerOnboardingWorkflow(lease.tenantId, lease.id)
    );
  }

  triggerInspection() {
    this.launchWorkflow('inspection', this.api.triggerInspectionWorkflow(this.inspectionId));
  }

  triggerSupervisor() {
    this.launchWorkflow(
      'supervisor',
      this.api.triggerSupervisorWorkflow(this.supervisorPrompt.trim())
    );
  }

  shortId(value: string | null | undefined) {
    return value ? `${value.slice(0, 8)}...` : 'n/a';
  }

  chipClass(status: WorkflowEntry['status']) {
    return `chip-${status}`;
  }

  dismissJob(jobId: string) {
    this.jobs.update(entries => entries.filter(entry => entry.id !== jobId));
  }

  private launchWorkflow(label: string, request$: ReturnType<ApiService['triggerRentCollectionWorkflow']>) {
    this.subscriptions.add(request$.subscribe({
      next: response => {
        const entry: WorkflowEntry = {
          id: response.job_id,
          label,
          workflow: label,
          status: 'running',
          startedAt: new Date().toISOString()
        };

        this.jobs.update(jobs => [entry, ...jobs]);
        this.watchJob(entry.id);
        this.snack.open(`${label} workflow started`, 'OK', { duration: 2500 });
      },
      error: error => {
        this.snack.open(this.buildStartErrorMessage(label, error), 'OK', { duration: 4500 });
      }
    }));
  }

  private watchJob(jobId: string) {
    this.subscriptions.add(
      interval(2500).pipe(
        switchMap(() => this.api.getWorkflowJob(jobId)),
        takeWhile(job => job.status === 'running', true)
      ).subscribe({
        next: job => this.updateJob(jobId, job),
        error: error => {
          const message = this.buildPollingErrorMessage(error);
          this.jobs.update(entries => entries.map(entry =>
            entry.id === jobId ? { ...entry, status: 'failed', error: message } : entry));
          this.snack.open(message, 'OK', { duration: 5000 });
        }
      })
    );
  }

  private updateJob(jobId: string, job: WorkflowJob) {
    const current = this.jobs().find(entry => entry.id === jobId);

    this.jobs.update(entries => entries.map(entry =>
      entry.id === jobId
        ? {
            ...entry,
            status: job.status,
            result: job.result,
            error: job.error
          }
        : entry));

    if (current?.status === 'running' && job.status === 'completed') {
      this.snack.open(`${current.label} workflow completed`, 'OK', { duration: 2500 });
    }

    if (current?.status !== 'failed' && job.status === 'failed') {
      this.snack.open(`${current?.label ?? 'Workflow'} failed: ${job.error ?? 'Unknown error.'}`, 'OK', {
        duration: 5000
      });
    }
  }

  private buildStartErrorMessage(label: string, error: unknown) {
    const detail = this.extractErrorDetail(error);
    return detail ? `Failed to start ${label} workflow: ${detail}` : `Failed to start ${label} workflow`;
  }

  private buildPollingErrorMessage(error: unknown) {
    const detail = this.extractErrorDetail(error);
    return detail ? `Unable to retrieve job status: ${detail}` : 'Unable to retrieve job status.';
  }

  private extractErrorDetail(error: unknown) {
    if (error instanceof HttpErrorResponse) {
      if (typeof error.error === 'string' && error.error.trim()) {
        return error.error.trim();
      }

      if (error.error && typeof error.error === 'object' && 'detail' in error.error) {
        const detail = error.error.detail;
        if (typeof detail === 'string' && detail.trim()) {
          return detail.trim();
        }
      }

      if (error.message) {
        return error.message;
      }
    }

    if (error instanceof Error && error.message) {
      return error.message;
    }

    return '';
  }
}
