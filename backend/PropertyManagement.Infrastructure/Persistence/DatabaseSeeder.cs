using PropertyManagement.Domain.Finance;
using PropertyManagement.Domain.Inspections;
using PropertyManagement.Domain.Leases;
using PropertyManagement.Domain.Maintenance;
using PropertyManagement.Domain.Properties;
using PropertyManagement.Domain.Tenants;

namespace PropertyManagement.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Properties.Any()) return;

        // ── Properties ────────────────────────────────────────────────────

        var properties = new[]
        {
            Property.Create("123 Oak Street",        "San Francisco", "CA", "94102", 8),
            Property.Create("456 Elm Avenue",         "Los Angeles",   "CA", "90001", 6),
            Property.Create("789 Maple Drive",        "San Diego",     "CA", "92101", 4),
            Property.Create("321 Pine Boulevard",     "Sacramento",    "CA", "95814", 10),
            Property.Create("654 Cedar Lane",         "Oakland",       "CA", "94601", 5),
            Property.Create("987 Birch Court",        "San Jose",      "CA", "95101", 3),
        };
        await db.Properties.AddRangeAsync(properties);

        // ── Tenants ───────────────────────────────────────────────────────

        var tenantData = new[]
        {
            ("Alice",   "Johnson",   "alice.johnson@email.com",    "415-555-0101"),
            ("Bob",     "Smith",     "bob.smith@email.com",        "213-555-0202"),
            ("Carol",   "Williams",  "carol.w@email.com",          "415-555-0303"),
            ("David",   "Brown",     "david.brown@email.com",      "619-555-0404"),
            ("Eva",     "Martinez",  "eva.martinez@email.com",     "916-555-0505"),
            ("Frank",   "Garcia",    "frank.garcia@email.com",     "510-555-0606"),
            ("Grace",   "Lee",       "grace.lee@email.com",        "408-555-0707"),
            ("Henry",   "Wilson",    "henry.wilson@email.com",     "415-555-0808"),
            ("Isabel",  "Taylor",    "isabel.taylor@email.com",    "213-555-0909"),
            ("James",   "Anderson",  "james.anderson@email.com",   "619-555-1010"),
            ("Karen",   "Thomas",    "karen.thomas@email.com",     "916-555-1111"),
            ("Leo",     "Jackson",   "leo.jackson@email.com",      "510-555-1212"),
            ("Maria",   "White",     "maria.white@email.com",      "408-555-1313"),
            ("Nathan",  "Harris",    "nathan.harris@email.com",    "415-555-1414"),
            ("Olivia",  "Clark",     "olivia.clark@email.com",     "213-555-1515"),
        };

        var tenants = tenantData.Select(t => Tenant.Create(t.Item1, t.Item2, t.Item3, t.Item4)).ToList();

        // All active except last 3 (applicants)
        foreach (var t in tenants.Take(12)) t.Activate();

        await db.Tenants.AddRangeAsync(tenants);

        // ── Leases ────────────────────────────────────────────────────────

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var leaseDefinitions = new[]
        {
            // prop, tenant, unit, start, end, rent, deposit, activate
            (properties[0], tenants[0],  "1A", today.AddYears(-2), today.AddMonths(8),  2800m, 5600m,  true),
            (properties[0], tenants[1],  "1B", today.AddYears(-1), today.AddMonths(2),  3000m, 6000m,  true),   // expiring soon
            (properties[0], tenants[2],  "2A", today.AddMonths(-8), today.AddMonths(4), 3200m, 6400m,  true),
            (properties[0], tenants[3],  "2B", today.AddYears(-1), today.AddMonths(1),  2900m, 5800m,  true),   // expiring very soon
            (properties[1], tenants[4],  "1",  today.AddYears(-1), today.AddMonths(6),  2200m, 4400m,  true),
            (properties[1], tenants[5],  "2",  today.AddMonths(-6), today.AddMonths(6), 2400m, 4800m,  true),
            (properties[2], tenants[6],  "A",  today.AddYears(-2), today.AddMonths(-1), 1800m, 3600m,  true),   // expired
            (properties[2], tenants[7],  "B",  today.AddMonths(-3), today.AddMonths(9), 1900m, 3800m,  true),
            (properties[3], tenants[8],  "101",today.AddYears(-1), today.AddMonths(5),  2600m, 5200m,  true),
            (properties[3], tenants[9],  "102",today.AddMonths(-9), today.AddMonths(3), 2750m, 5500m,  true),
            (properties[3], tenants[10], "103",today.AddMonths(-4), today.AddMonths(8), 2500m, 5000m,  true),
            (properties[4], tenants[11], "1A", today.AddYears(-1), today.AddMonths(7),  2100m, 4200m,  true),
            (properties[5], tenants[12], "1",  today.AddMonths(-2), today.AddMonths(10),1700m, 3400m,  true),
            (properties[0], tenants[13], "3A", today,              today.AddYears(1),   3100m, 6200m,  false),  // draft
            (properties[1], tenants[14], "3",  today,              today.AddYears(1),   2300m, 4600m,  false),  // draft
        };

        var leases = leaseDefinitions.Select(l =>
        {
            var lease = Lease.Create(l.Item1.Id, l.Item2.Id, l.Item3, l.Item4, l.Item5, l.Item6, l.Item7);
            if (l.Item8)
            {
                lease.Activate();
                if (l.Item5 < today) lease.Expire();   // past end date → expired
            }
            return lease;
        }).ToList();

        await db.Leases.AddRangeAsync(leases);

        // ── Maintenance Requests ──────────────────────────────────────────

        var activeLeases = leases.Where(l => l.Status == LeaseStatus.Active).ToList();

        var maintenanceData = new[]
        {
            (activeLeases[0],  "Leaking kitchen faucet",          "Kitchen faucet drips constantly.",                         Priority.Urgent,    MaintenanceStatus.Open,       null as string),
            (activeLeases[0],  "Broken heater",                   "Heating unit not working. No heat.",                        Priority.Emergency, MaintenanceStatus.InProgress, "HVAC Pro Services"),
            (activeLeases[1],  "Cracked bedroom window",          "Crack on left bedroom window pane.",                        Priority.Routine,   MaintenanceStatus.Open,       null),
            (activeLeases[1],  "Mold in bathroom",                "Black mold found near shower grout.",                       Priority.Urgent,    MaintenanceStatus.Assigned,   "CleanPro Remediation"),
            (activeLeases[2],  "Broken garbage disposal",         "Disposal stopped working after jam.",                       Priority.Routine,   MaintenanceStatus.Open,       null),
            (activeLeases[2],  "Electrical outlet sparking",      "Outlet in living room sparks when plugged in.",             Priority.Emergency, MaintenanceStatus.Assigned,   "City Electric Co."),
            (activeLeases[3],  "Water heater not heating",        "No hot water for 2 days.",                                  Priority.Urgent,    MaintenanceStatus.InProgress, "PlumbRight LLC"),
            (activeLeases[3],  "Pest infestation",                "Cockroaches spotted in kitchen cabinets.",                  Priority.Urgent,    MaintenanceStatus.Open,       null),
            (activeLeases[4],  "Broken door lock",                "Front door deadbolt doesn't engage properly.",              Priority.Urgent,    MaintenanceStatus.Resolved,   "LockSmith Plus"),
            (activeLeases[4],  "AC not cooling",                  "Air conditioner runs but doesn't cool below 80F.",          Priority.Urgent,    MaintenanceStatus.Open,       null),
            (activeLeases[5],  "Dryer not working",               "Dryer runs but clothes stay wet.",                          Priority.Routine,   MaintenanceStatus.Open,       null),
            (activeLeases[5],  "Hole in drywall",                 "Large hole in hallway wall from door handle.",              Priority.Routine,   MaintenanceStatus.Assigned,   "HandyFix Services"),
            (activeLeases[6],  "Roof leak",                       "Water dripping from ceiling after rain.",                   Priority.Emergency, MaintenanceStatus.InProgress, "RoofMasters Inc."),
            (activeLeases[7],  "Broken dishwasher",               "Dishwasher fills with water but won't drain.",              Priority.Routine,   MaintenanceStatus.Open,       null),
            (activeLeases[8],  "Smoke detector beeping",          "Smoke detector in bedroom beeping every 30 seconds.",       Priority.Urgent,    MaintenanceStatus.Resolved,   "Safety First Co."),
            (activeLeases[8],  "Toilet running constantly",       "Toilet tank refills every 10 minutes non-stop.",            Priority.Routine,   MaintenanceStatus.Open,       null),
            (activeLeases[9],  "Broken stair railing",            "Railing on second floor stairs loose and unsafe.",          Priority.Urgent,    MaintenanceStatus.Assigned,   "StructureRight LLC"),
            (activeLeases[10], "No cold water in bathroom",       "Cold water tap in bathroom doesn't work.",                  Priority.Urgent,    MaintenanceStatus.Open,       null),
            (activeLeases[11], "Garage door not opening",         "Garage door motor runs but door stays closed.",             Priority.Routine,   MaintenanceStatus.Open,       null),
            (activeLeases[11], "Gas smell near stove",            "Faint gas smell detected near kitchen stove area.",         Priority.Emergency, MaintenanceStatus.InProgress, "SafeGas Utilities"),
        };

        var maintenanceRequests = maintenanceData.Select(m =>
        {
            var req = MaintenanceRequest.Create(m.Item1.Id, m.Item2, m.Item3, m.Item4);
            if (m.Item6 != null) req.Assign(m.Item6);
            if (m.Item5 == MaintenanceStatus.InProgress) { if (m.Item6 != null) req.StartWork(); }
            if (m.Item5 == MaintenanceStatus.Resolved) req.Resolve();
            return req;
        }).ToList();

        await db.MaintenanceRequests.AddRangeAsync(maintenanceRequests);

        // ── Payments — 6 months history per active lease ──────────────────

        var payments = new List<Payment>();
        var activeForPayments = leases.Where(l => l.Status == LeaseStatus.Active).ToList();

        foreach (var lease in activeForPayments)
        {
            for (var monthsAgo = 5; monthsAgo >= 0; monthsAgo--)
            {
                var dueDate = new DateOnly(today.Year, today.Month, 1).AddMonths(-monthsAgo);
                var payment = Payment.Create(lease.Id, lease.MonthlyRent, dueDate);

                if (monthsAgo >= 2)
                {
                    // older months — all paid
                    payment.MarkPaid(monthsAgo % 2 == 0 ? "bank_transfer" : "online_portal");
                }
                else if (monthsAgo == 1)
                {
                    // last month — some paid, some overdue (every 3rd lease)
                    var idx = activeForPayments.IndexOf(lease);
                    if (idx % 3 == 0) payment.MarkOverdue();
                    else payment.MarkPaid("bank_transfer");
                }
                // current month stays pending

                payments.Add(payment);
            }
        }

        await db.Payments.AddRangeAsync(payments);

        // ── Inspections ───────────────────────────────────────────────────

        var now = DateTime.UtcNow;
        var inspections = new List<Inspection>();

        // Completed move-in inspections for active leases
        foreach (var lease in leases.Where(l => l.Status == LeaseStatus.Active).Take(6))
        {
            var moveIn = Inspection.Create(lease.PropertyId, lease.Id, InspectionType.MoveIn,
                lease.StartDate.ToDateTime(TimeOnly.MinValue).AddDays(1));
            moveIn.Complete("Move-in inspection completed. Unit in good condition. Minor scuffs on hallway wall noted.");
            inspections.Add(moveIn);
        }

        // Scheduled routine inspections (upcoming)
        foreach (var prop in properties.Take(4))
        {
            var routine = Inspection.Create(prop.Id, null, InspectionType.Routine,
                now.AddDays(Random.Shared.Next(5, 30)));
            inspections.Add(routine);
        }

        // Scheduled move-out inspections for expiring leases
        var expiringLeases = leases.Where(l => l.Status == LeaseStatus.Active &&
            l.EndDate <= DateOnly.FromDateTime(now.AddDays(60))).ToList();
        foreach (var lease in expiringLeases)
        {
            var moveOut = Inspection.Create(lease.PropertyId, lease.Id, InspectionType.MoveOut,
                lease.EndDate.ToDateTime(TimeOnly.MinValue).AddDays(-3));
            inspections.Add(moveOut);
        }

        // Emergency inspection with findings (for workflow exploration)
        var emergencyInspection = Inspection.Create(properties[0].Id, activeLeases[0].Id,
            InspectionType.Emergency, now.AddDays(-1));
        emergencyInspection.Complete(
            "Emergency inspection triggered by tenant report. " +
            "ISSUE: Water damage on ceiling in master bedroom - Priority: Emergency. " +
            "ISSUE: Broken smoke detector in kitchen - Priority: Emergency. " +
            "ISSUE: Mold forming near window sill in bathroom - Priority: Urgent. " +
            "ISSUE: Cracked tile in hallway floor - Priority: Routine.");
        inspections.Add(emergencyInspection);

        // Completed routine inspection with issues
        var routineWithIssues = Inspection.Create(properties[2].Id, activeLeases[6].Id,
            InspectionType.Routine, now.AddDays(-7));
        routineWithIssues.Complete(
            "Routine annual inspection completed. " +
            "ISSUE: Dryer vent blocked and needs cleaning - Priority: Urgent. " +
            "ISSUE: Weatherstripping on front door worn out - Priority: Routine. " +
            "ISSUE: Kitchen faucet showing early signs of corrosion - Priority: Routine.");
        inspections.Add(routineWithIssues);

        // A few cancelled inspections
        var cancelled = Inspection.Create(properties[3].Id, null, InspectionType.Routine,
            now.AddDays(-14));
        cancelled.Cancel();
        inspections.Add(cancelled);

        await db.Inspections.AddRangeAsync(inspections);
        await db.SaveChangesAsync();
    }
}
