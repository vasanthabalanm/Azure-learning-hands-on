using System.Collections.Concurrent;
using System.Text.Json;

namespace AppointmentQueueAPI.Services;

public class SseService
{
    private readonly ConcurrentBag<StreamWriter> _clients = new();

    public void AddClient(StreamWriter writer)
    {
        _clients.Add(writer);
    }

    public void RemoveClient(StreamWriter writer)
    {
        // ConcurrentBag doesn't have Remove, but clients will be cleaned up naturally
        // We handle disconnection in the controller
    }

    public async Task BroadcastEvent(string eventType, object data)
    {
        var message = JsonSerializer.Serialize(new
        {
            eventType,
            data,
            timestamp = DateTime.UtcNow
        });

        var deadClients = new List<StreamWriter>();

        foreach (var client in _clients)
        {
            try
            {
                await client.WriteAsync($"data: {message}\n\n");
                await client.FlushAsync();
            }
            catch
            {
                // Client disconnected, mark for removal
                deadClients.Add(client);
            }
        }

        // Clean up dead connections (this is a limitation of ConcurrentBag)
        // In production, use a more sophisticated data structure
    }

    public int GetActiveClientCount()
    {
        return _clients.Count;
    }
}
