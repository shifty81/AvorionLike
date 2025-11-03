using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace AvorionLike.Core.Networking;

/// <summary>
/// Server for handling multiplayer connections
/// </summary>
public class GameServer
{
    private TcpListener? _listener;
    private readonly List<ClientConnection> _clients = new();
    private readonly Dictionary<string, SectorServer> _sectors = new();
    private bool _isRunning;
    private readonly int _port;

    public bool IsRunning => _isRunning;

    public GameServer(int port = 27015)
    {
        _port = port;
    }

    /// <summary>
    /// Start the server
    /// </summary>
    public void Start()
    {
        if (_isRunning) return;

        try
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"Server started on port {_port}");

            // Start accepting clients in background
            Task.Run(AcceptClientsAsync);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start server: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop the server
    /// </summary>
    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _listener?.Stop();

        foreach (var client in _clients.ToList())
        {
            client.Disconnect();
        }

        _clients.Clear();
        Console.WriteLine("Server stopped");
    }

    private async Task AcceptClientsAsync()
    {
        while (_isRunning && _listener != null)
        {
            try
            {
                var tcpClient = await _listener.AcceptTcpClientAsync();
                var client = new ClientConnection(tcpClient);
                _clients.Add(client);

                Console.WriteLine($"Client connected: {client.Id}");

                // Handle client in background
                _ = Task.Run(() => HandleClientAsync(client));
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }
    }

    private async Task HandleClientAsync(ClientConnection client)
    {
        try
        {
            while (client.IsConnected)
            {
                var message = await client.ReceiveMessageAsync();
                if (message != null)
                {
                    await ProcessMessageAsync(client, message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client {client.Id}: {ex.Message}");
        }
        finally
        {
            _clients.Remove(client);
            client.Disconnect();
        }
    }

    private async Task ProcessMessageAsync(ClientConnection client, NetworkMessage message)
    {
        Console.WriteLine($"Received message from {client.Id}: {message.Type}");

        switch (message.Type)
        {
            case MessageType.JoinSector:
                await HandleJoinSector(client, message.Data);
                break;
            case MessageType.LeaveSector:
                await HandleLeaveSector(client, message.Data);
                break;
            case MessageType.EntityUpdate:
                await BroadcastToSector(client, message);
                break;
        }
    }

    private async Task HandleJoinSector(ClientConnection client, string? sectorId)
    {
        if (string.IsNullOrEmpty(sectorId)) return;

        if (!_sectors.ContainsKey(sectorId))
        {
            _sectors[sectorId] = new SectorServer(sectorId);
        }

        _sectors[sectorId].AddClient(client);
        await client.SendMessageAsync(new NetworkMessage
        {
            Type = MessageType.SectorJoined,
            Data = sectorId
        });
    }

    private Task HandleLeaveSector(ClientConnection client, string? sectorId)
    {
        if (string.IsNullOrEmpty(sectorId)) return Task.CompletedTask;

        if (_sectors.TryGetValue(sectorId, out var sector))
        {
            sector.RemoveClient(client);
        }

        return Task.CompletedTask;
    }

    private async Task BroadcastToSector(ClientConnection sender, NetworkMessage message)
    {
        foreach (var sector in _sectors.Values)
        {
            if (sector.HasClient(sender))
            {
                await sector.BroadcastAsync(message, sender);
            }
        }
    }
}

/// <summary>
/// Represents a client connection
/// </summary>
public class ClientConnection
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    public Guid Id { get; }
    public bool IsConnected => _client?.Connected ?? false;

    public ClientConnection(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        Id = Guid.NewGuid();
    }

    public async Task<NetworkMessage?> ReceiveMessageAsync()
    {
        try
        {
            var buffer = new byte[4096];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0) return null;

            var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            return JsonSerializer.Deserialize<NetworkMessage>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task SendMessageAsync(NetworkMessage message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            await _stream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        _stream?.Close();
        _client?.Close();
    }
}

/// <summary>
/// Manages a single sector on the server
/// </summary>
public class SectorServer
{
    public string Id { get; }
    private readonly List<ClientConnection> _clients = new();
    private readonly object _lock = new();

    public SectorServer(string id)
    {
        Id = id;
    }

    public void AddClient(ClientConnection client)
    {
        lock (_lock)
        {
            if (!_clients.Contains(client))
            {
                _clients.Add(client);
            }
        }
    }

    public void RemoveClient(ClientConnection client)
    {
        lock (_lock)
        {
            _clients.Remove(client);
        }
    }

    public bool HasClient(ClientConnection client)
    {
        lock (_lock)
        {
            return _clients.Contains(client);
        }
    }

    public async Task BroadcastAsync(NetworkMessage message, ClientConnection? exclude = null)
    {
        List<ClientConnection> clientsCopy;
        lock (_lock)
        {
            clientsCopy = new List<ClientConnection>(_clients);
        }

        foreach (var client in clientsCopy)
        {
            if (client != exclude && client.IsConnected)
            {
                await client.SendMessageAsync(message);
            }
        }
    }
}

/// <summary>
/// Network message types
/// </summary>
public enum MessageType
{
    JoinSector,
    LeaveSector,
    EntityUpdate,
    SectorJoined,
    ChatMessage
}

/// <summary>
/// Network message structure
/// </summary>
public class NetworkMessage
{
    public MessageType Type { get; set; }
    public string? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
