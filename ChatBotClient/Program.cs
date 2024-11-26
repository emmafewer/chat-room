using System.Net;
using System.Net.Sockets;
using System.Text;


//Template code, but instead let's just use netcat until I develop a web app
/*
var hostAddress = IPAddress.Parse("127.0.0.1");
var ipEndPoint = new IPEndPoint(hostAddress, 13000);

using TcpClient client = new();
await client.ConnectAsync(ipEndPoint);

await using NetworkStream stream = client.GetStream();
while (true)
{
    var buffer = new byte[1_024];
    int received = await stream.ReadAsync(buffer);

    var message = Encoding.UTF8.GetString(buffer, 0, received);
    Console.WriteLine($"Message received: \"{message}\""); 
}

// Sample output:
//     Message received: "📅 8/22/2022 9:07:17 AM 🕛"

*/
