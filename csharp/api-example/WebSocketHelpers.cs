using System.Net.WebSockets;
using System.Text;

namespace ApiExample;

public class WebSocketHelpers
{
    public static async Task SendAsync(ClientWebSocket ws, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        Console.WriteLine($"➡️ Sent: {json}");
    }

    public static async Task ListenWebSocket(ClientWebSocket ws, CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];

        try
        {
            // Keep listening until WebSocket is open or cancellation is requested
            while (ws.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"Socket closed. Reason: {result.CloseStatus} - {result.CloseStatusDescription}");
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine("Received message:");
                Console.WriteLine(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred while receiving WebSocket messages: {ex.Message}");
        }
        finally
        {
            // Ensure the WebSocket is closed properly
            if (ws.State == WebSocketState.Open)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                Console.WriteLine("WebSocket closed.");
            }
        }
    }
}