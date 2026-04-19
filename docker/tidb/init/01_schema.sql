CREATE DATABASE IF NOT EXISTS property_mgmt;
USE property_mgmt;

-- ── Properties ────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS properties (
    id            CHAR(36)     NOT NULL PRIMARY KEY,
    address       VARCHAR(255) NOT NULL,
    city          VARCHAR(100) NOT NULL,
    state         CHAR(2)      NOT NULL,
    zip           VARCHAR(10)  NOT NULL,
    unit_count    INT          NOT NULL DEFAULT 1,
    created_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ── Tenants ───────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS tenants (
    id            CHAR(36)     NOT NULL PRIMARY KEY,
    first_name    VARCHAR(100) NOT NULL,
    last_name     VARCHAR(100) NOT NULL,
    email         VARCHAR(255) NOT NULL UNIQUE,
    phone         VARCHAR(20),
    status        ENUM('applicant','active','former') NOT NULL DEFAULT 'applicant',
    created_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ── Leases ────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS leases (
    id              CHAR(36)       NOT NULL PRIMARY KEY,
    property_id     CHAR(36)       NOT NULL,
    tenant_id       CHAR(36)       NOT NULL,
    unit_number     VARCHAR(20)    NOT NULL,
    start_date      DATE           NOT NULL,
    end_date        DATE           NOT NULL,
    monthly_rent    DECIMAL(10,2)  NOT NULL,
    security_deposit DECIMAL(10,2) NOT NULL,
    status          ENUM('draft','active','expired','terminated') NOT NULL DEFAULT 'draft',
    created_at      DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (property_id) REFERENCES properties(id),
    FOREIGN KEY (tenant_id)   REFERENCES tenants(id)
);

-- ── Maintenance Requests ──────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS maintenance_requests (
    id            CHAR(36)     NOT NULL PRIMARY KEY,
    lease_id      CHAR(36)     NOT NULL,
    title         VARCHAR(255) NOT NULL,
    description   TEXT         NOT NULL,
    priority      ENUM('emergency','urgent','routine') NOT NULL DEFAULT 'routine',
    status        ENUM('open','assigned','in_progress','resolved','closed') NOT NULL DEFAULT 'open',
    assigned_to   VARCHAR(255),
    reported_at   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    resolved_at   DATETIME,
    updated_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (lease_id) REFERENCES leases(id)
);

-- ── Payments ──────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS payments (
    id            CHAR(36)      NOT NULL PRIMARY KEY,
    lease_id      CHAR(36)      NOT NULL,
    amount        DECIMAL(10,2) NOT NULL,
    due_date      DATE          NOT NULL,
    paid_date     DATE,
    status        ENUM('pending','paid','overdue','waived') NOT NULL DEFAULT 'pending',
    payment_method VARCHAR(50),
    notes         TEXT,
    created_at    DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (lease_id) REFERENCES leases(id)
);

-- ── Inspections ───────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS inspections (
    id            CHAR(36)     NOT NULL PRIMARY KEY,
    property_id   CHAR(36)     NOT NULL,
    lease_id      CHAR(36),
    type          ENUM('move_in','move_out','routine','emergency') NOT NULL,
    scheduled_at  DATETIME     NOT NULL,
    completed_at  DATETIME,
    notes         TEXT,
    status        ENUM('scheduled','completed','cancelled') NOT NULL DEFAULT 'scheduled',
    created_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (property_id) REFERENCES properties(id),
    FOREIGN KEY (lease_id)    REFERENCES leases(id)
);

-- ── Workflow Events (audit trail for agent actions) ───────────────────────

CREATE TABLE IF NOT EXISTS workflow_events (
    id            CHAR(36)     NOT NULL PRIMARY KEY,
    workflow_type VARCHAR(100) NOT NULL,
    entity_id     CHAR(36)     NOT NULL,
    entity_type   VARCHAR(100) NOT NULL,
    status        ENUM('triggered','running','completed','failed') NOT NULL DEFAULT 'triggered',
    payload       JSON,
    result        JSON,
    triggered_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at  DATETIME
);
