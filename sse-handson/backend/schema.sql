-- ============================================
-- PostgreSQL Schema for SSE Appointment Queue
-- Run this in psql or pgAdmin if you prefer
-- manual setup over EF Core migrations
-- ============================================

-- Step 1: Create the database
CREATE DATABASE appointment_queue;

-- Step 2: Connect to it, then run the rest
\c appointment_queue;

-- Step 3: Create appointments table
CREATE TABLE IF NOT EXISTS "Appointments" (
    "Id"                  SERIAL PRIMARY KEY,
    "PatientName"         VARCHAR(100)  NOT NULL,
    "Status"              VARCHAR(50)   NOT NULL DEFAULT 'Pending',
    "AssignedDoctorName"  VARCHAR(100)  NULL,
    "PickedByDoctorName"  VARCHAR(100)  NULL,
    "BookedAt"            TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    "xmin"                xid           -- used by EF Core for optimistic concurrency
);

-- Step 4: Verify
SELECT * FROM "Appointments";

-- ============================================
-- Sample Data (Optional - for testing)
-- ============================================
INSERT INTO "Appointments" ("PatientName", "Status", "BookedAt")
VALUES
  ('Alice Johnson', 'Pending', NOW()),
  ('Bob Smith',     'Pending', NOW()),
  ('Carol White',   'Pending', NOW());

SELECT * FROM "Appointments";
