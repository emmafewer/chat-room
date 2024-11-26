using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatBotWorker;

public class TcpServer
{
    private TcpListener _tcpListener;
    
    private static Dictionary<string, TcpClient> _clients = new Dictionary<string, TcpClient>();
    private static List<(string Username, string Message)> _chatHistory = new List<(string, string)>();

    public async Task StartServer()
    {
        var port = 13000;
        var hostAddress = IPAddress.Parse("127.0.0.1");
        _tcpListener = new TcpListener(hostAddress, port);
        _tcpListener.Start();
        Console.WriteLine($"Listening on port {port}");

        while (true)
        {
            TcpClient client = await _tcpListener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected!");
            _ = HandleClientsAsync(client);
        }
    }

    public async Task HandleClientsAsync(TcpClient client)
    {
        var tcpStream = client.GetStream();
        StreamReader reader = new StreamReader(tcpStream);
        StreamWriter writer = new StreamWriter(tcpStream) { AutoFlush = true };
        
        await writer.WriteLineAsync($"Hello there, what is your nickname?");
        
        string clientEndPoint = client.Client.RemoteEndPoint.ToString();

        try
        {
            while (true)
            {
                bool isNicknameSet = false;
                string nickname = string.Empty;

                while (!isNicknameSet)
                {
                    string? nameAttempt = await reader.ReadLineAsync();
                    
                    Console.WriteLine($"Received from {clientEndPoint}: {nameAttempt}");
                    
                    if (!string.IsNullOrWhiteSpace(nameAttempt) && !_clients.ContainsKey(nameAttempt))
                    {
                        Broadcast($"{nickname} has joined the chat{Environment.NewLine}");
                        _clients.Add(nameAttempt, client);
                        nickname = nameAttempt;
                        isNicknameSet = true;
                    }
                    else
                    {
                        await writer.WriteLineAsync($"You must choose a new nickname");
                    }
                }

                
                await Welcome(nickname, tcpStream);

                while (true)
                {
                    string? newMessage = await reader.ReadLineAsync();
                    
                    if (newMessage == null)
                    {
                        Console.WriteLine("Client Disconnected");
                        Broadcast($"{nickname} has left the chat.{Environment.NewLine}");
                        break;
                    }
                    AddToChatHistory(nickname, newMessage);
                    Broadcast($"<{nickname}>: {newMessage}{Environment.NewLine}");
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with client: {ex.Message}");
        }
        finally
        {
            tcpStream.Close();
        }

   
    }
    
    private static void Broadcast(string message)
    {
        var response = Encoding.UTF8.GetBytes(message);

        foreach (var client in _clients)
        {
            NetworkStream stream = client.Value.GetStream();
            _ = stream.WriteAsync(response, 0, response.Length);
        }
    }

    private async Task Welcome(string nickname, NetworkStream stream)
    {
        if (_clients.Count == 1)
        {
            byte[] welcome = Encoding.UTF8.GetBytes($"Welcome! You are the only person online.");
            await stream.WriteAsync(welcome, 0, welcome.Length); 
        }
        else
        {
            var otherPeopleInChat = string.Join( ',', _clients.Keys.Where(k=>k != nickname).ToArray());
            byte[] instructions = Encoding.UTF8.GetBytes($"{otherPeopleInChat} are also in the chat. Here is the latest history{Environment.NewLine}");
            await stream.WriteAsync(instructions, 0, instructions.Length);

            foreach (var entry in _chatHistory)
            {
                byte[] prevEntry = Encoding.UTF8.GetBytes($"<{entry.Username}>: {entry.Message}{Environment.NewLine}");
                await stream.WriteAsync(prevEntry, 0, prevEntry.Length);
            } 
        }
    }
    
    private static void AddToChatHistory(string nickname, string newMessage)
    {
        _chatHistory.Add((nickname, newMessage));

        //only need to store the last 10 records
        if (_chatHistory.Count > 10)
        {
            _chatHistory.RemoveAt(0);
        }
    }
}