using AppointmentQueueAPI.Data;
using AppointmentQueueAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentQueueAPI.Services;

// Scoped (not Singleton) because DbContext is scoped per request
public class AppointmentService
{
    private readonly AppointmentDbContext _db;

    public AppointmentService(AppointmentDbContext db)
    {
        _db = db;
    }

    public async Task<Appointment> BookAppointmentAsync(string patientName)
    {
        var appointment = new Appointment
        {
            PatientName = patientName,
            Status = "Pending",
            BookedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment?> MapToDoctorAsync(int appointmentId, string doctorName)
    {
        var appointment = await _db.Appointments.FindAsync(appointmentId);

        if (appointment == null || appointment.Status != "Pending")
            return null;

        appointment.Status = "MappedToDoctor";
        appointment.AssignedDoctorName = doctorName;

        await _db.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment?> PickPatientAsync(int appointmentId, string doctorName)
    {
        // Retry loop handles optimistic concurrency conflicts
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var appointment = await _db.Appointments.FindAsync(appointmentId);

                if (appointment == null)
                    return null;

                // Only pick if it's still available and assigned to this doctor
                if (appointment.Status != "MappedToDoctor" ||
                    appointment.AssignedDoctorName != doctorName)
                    return null;

                appointment.Status = "PickedByDoctor";
                appointment.PickedByDoctorName = doctorName;

                // SaveChanges throws DbUpdateConcurrencyException if another
                // doctor picked this patient at the same time (RowVersion mismatch)
                await _db.SaveChangesAsync();
                return appointment;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Another doctor won the race - patient already picked
                return null;
            }
        }

        return null;
    }

    public async Task<List<Appointment>> GetAllAppointmentsAsync()
    {
        return await _db.Appointments
            .OrderBy(a => a.BookedAt)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorName)
    {
        return await _db.Appointments
            .Where(a => a.AssignedDoctorName == doctorName)
            .OrderBy(a => a.BookedAt)
            .ToListAsync();
    }
}
