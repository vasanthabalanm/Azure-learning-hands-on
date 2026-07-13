using AppointmentQueueAPI.Models;
using AppointmentQueueAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentQueueAPI.Controllers;

/// <summary>
/// Manages appointment operations: booking, assignment, and patient pickup.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly AppointmentService _appointmentService;
    private readonly SseService _sseService;

    public AppointmentController(AppointmentService appointmentService, SseService sseService)
    {
        _appointmentService = appointmentService;
        _sseService = sseService;
    }

    /// <summary>
    /// Book a new appointment for a patient.
    /// Triggers SSE event: "appointment_booked" to all connected clients.
    /// </summary>
    /// <param name="request">Patient name for the appointment</param>
    /// <returns>The newly created appointment</returns>
    /// <response code="200">Appointment successfully booked</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("book")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
    {
        var appointment = await _appointmentService.BookAppointmentAsync(request.PatientName);

        // Broadcast to all SSE clients
        await _sseService.BroadcastEvent("appointment_booked", appointment);

        return Ok(appointment);
    }

    /// <summary>
    /// Assign a patient appointment to a specific doctor.
    /// Staff role only. Triggers SSE event: "patient_mapped" to all connected clients.
    /// </summary>
    /// <param name="request">Appointment ID and doctor name</param>
    /// <returns>The updated appointment</returns>
    /// <response code="200">Patient successfully assigned to doctor</response>
    /// <response code="400">Appointment not found or already assigned</response>
    [HttpPost("map")]
    public async Task<IActionResult> MapToDoctor([FromBody] MapToDoctorRequest request)
    {
        var appointment = await _appointmentService.MapToDoctorAsync(
            request.AppointmentId, request.DoctorName);

        if (appointment == null)
            return BadRequest("Appointment not found or already assigned.");

        // Broadcast to all SSE clients - doctors see new patient in queue
        await _sseService.BroadcastEvent("patient_mapped", appointment);

        return Ok(appointment);
    }

    /// <summary>
    /// Doctor picks a patient from their assigned queue.
    /// Enforces concurrency: only one doctor can pick a patient.
    /// Triggers SSE event: "patient_picked" to all connected clients.
    /// </summary>
    /// <param name="request">Appointment ID and doctor name picking the patient</param>
    /// <returns>The updated appointment</returns>
    /// <response code="200">Patient successfully picked</response>
    /// <response code="400">Patient not available or already picked by another doctor (concurrency lock)</response>
    [HttpPost("pick")]
    public async Task<IActionResult> PickPatient([FromBody] PickPatientRequest request)
    {
        var appointment = await _appointmentService.PickPatientAsync(
            request.AppointmentId, request.DoctorName);

        if (appointment == null)
            return BadRequest("Patient not available or already picked by another doctor.");

        // Broadcast to all SSE clients - other doctors see patient removed from queue
        await _sseService.BroadcastEvent("patient_picked", appointment);

        return Ok(appointment);
    }

    /// <summary>
    /// Get all appointments in the system.
    /// </summary>
    /// <returns>List of all appointments ordered by booking time</returns>
    /// <response code="200">List of appointments retrieved</response>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllAppointments()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    /// <summary>
    /// Get all appointments assigned to a specific doctor.
    /// </summary>
    /// <param name="doctorName">Name of the doctor (e.g., "Smith")</param>
    /// <returns>List of appointments assigned to the doctor</returns>
    /// <response code="200">List of doctor's appointments retrieved</response>
    [HttpGet("doctor/{doctorName}")]
    public async Task<IActionResult> GetDoctorAppointments(string doctorName)
    {
        var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorName);
        return Ok(appointments);
    }
}
