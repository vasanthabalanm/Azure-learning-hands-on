using AppointmentQueueAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentQueueAPI.Controllers;

/// <summary>
/// Real-time event streaming via Server-Sent Events (SSE).
/// Clients connect once and receive push notifications for all appointment changes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SseController : ControllerBase
{
    private readonly SseService _sseService;

    public SseController(SseService sseService)
    {
        _sseService = sseService;
    }

    /// <summary>
    /// Open a Server-Sent Events stream for real-time appointment updates.
    /// Clients should use EventSource API to connect to this endpoint.
    /// Connection remains open; events are pushed as they occur.
    /// </summary>
    /// <returns>Continuous stream of appointment events</returns>
    /// <remarks>
    /// Example JavaScript usage:
    /// <code>
    /// const eventSource = new EventSource('http://localhost:5000/api/sse/stream');
    /// eventSource.onmessage = (event) => {
    ///   const data = JSON.parse(event.data);
    ///   console.log('Event:', data.eventType, data.data);
    /// };
    /// </code>
    /// </remarks>
    [HttpGet("stream")]
    public async Task StreamEvents()
    {
        // Set SSE-specific headers
        Response.ContentType = "text/event-stream";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");

        var writer = new StreamWriter(Response.Body);
        _sseService.AddClient(writer);

        // Send initial connection message
        await writer.WriteAsync("data: {\"eventType\":\"connected\",\"message\":\"SSE connection established\"}\n\n");
        await writer.FlushAsync();

        // Keep connection alive until client disconnects
        try
        {
            await Request.HttpContext.RequestAborted.WaitHandle.WaitOneAsync();
        }
        catch
        {
            // Client disconnected
        }
        finally
        {
            _sseService.RemoveClient(writer);
        }
    }

    /// <summary>
    /// Get the current status of the SSE service.
    /// </summary>
    /// <returns>Number of active SSE connections and service status</returns>
    /// <response code="200">Service status retrieved</response>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            activeConnections = _sseService.GetActiveClientCount(),
            message = "SSE service is running",
            timestamp = DateTime.UtcNow
        });
    }
}

// Extension method for WaitHandle
public static class WaitHandleExtensions
{
    public static Task WaitOneAsync(this WaitHandle handle)
    {
        var tcs = new TaskCompletionSource<bool>();
        ThreadPool.RegisterWaitForSingleObject(
            handle,
            (state, timedOut) => tcs.SetResult(!timedOut),
            null,
            Timeout.Infinite,
            true);
        return tcs.Task;
    }
}
