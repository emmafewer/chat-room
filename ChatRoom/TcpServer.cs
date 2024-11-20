using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatRoom;

public class TcpServer
{
    private TcpListener _tcpListener;
    
    private Dictionary<string, TcpClient> _clients = new Dictionary<string, TcpClient>();
    private List<(string Username, string Message)> _chatHistory = new List<(string, string)>();

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
        byte[] helloMessage = Encode($"Hello there, what is your nickname?");
        await tcpStream.WriteAsync(helloMessage, 0, helloMessage.Length);
 
        
        byte[] buffer = new byte[256];
        string clientEndPoint = client.Client.RemoteEndPoint.ToString();

        try
        {
            while (true)
            {
                int bytesRead = await tcpStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine($"Client {clientEndPoint} disconnected.");
                    break;
                }
                
                string nickname = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                Console.WriteLine($"Received from {clientEndPoint}: {nickname}");
                
                if (_clients.ContainsKey(nickname))
                {
                    byte[] failedResponse = Encode($"You must choose a new nickname");
                    await tcpStream.WriteAsync(failedResponse, 0, failedResponse.Length);
                }
                else
                {
                    _clients.Add(nickname, client);
                    
                    await Welcome(nickname, tcpStream);
                    _ = HandleFurtherChats(nickname, tcpStream);
                   
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

    private async Task Welcome(string nickname, NetworkStream stream)
    {
      
     foreach (var client in _clients)
     {
         NetworkStream clientStream = client.Value.GetStream();
         byte[] response = Encode($"{nickname} joined the chat");
         await clientStream.WriteAsync(response, 0, response.Length);
     }

     /*
        byte[] instructions = Encode($"Continue chatting as you please. Here are the latest history");
        await stream.WriteAsync(instructions, 0, instructions.Length);

        foreach (var entry in _chatHistory)
        {
            byte[] prevEntry = Encode($"<{entry.Username}>: {entry.Message}");
            await stream.WriteAsync(prevEntry, 0, prevEntry.Length);
        }
        */
        
        //_ = HandleFurtherChats(nickname, stream);
    }

    private async Task HandleFurtherChats(string nickname, NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
    
        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine($"Client  disconnected.");
                    break;
                }
                
                string chat = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                
                _ = HandleChatHistory(nickname, chat);
               
                
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with client: {ex.Message}");
        }
        finally
        {
            stream.Close();
        }
    }

    private async Task HandleChatHistory(string nickname, string newMessage)
    {
        _chatHistory.Add((nickname, newMessage));

        if (_chatHistory.Count > 10)
        {
            _chatHistory.RemoveAt(0);
        }

        foreach (var client in _clients)
        {
            if (client.Key == nickname)
                continue;
            
            NetworkStream stream = client.Value.GetStream();
            byte[] returnMessage = Encode($"<{nickname}>: {newMessage}");
            await stream.WriteAsync(returnMessage, 0, returnMessage.Length);
        }
    }

    private static byte[] Encode(string message)
    {
        return Encoding.UTF8.GetBytes(message);
    }
}