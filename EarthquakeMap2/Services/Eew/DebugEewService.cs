using System.Net.WebSockets;
using System.Text;
using EewLibrary;

namespace EarthquakeMap2.Services.Eew
{
    internal class DebugEewService : IEewService<EEW>
    {
        public async Task Start()
        {
            var content = await File.ReadAllLinesAsync("ip.txt");
            var uriBuilder = new UriBuilder(content[0]){Port=int.Parse(content[1])};
            Task.Run(async () =>
            {
                while (await Connect(uriBuilder.Uri)) await Task.Delay(10000);
            });
        }

        public SortedSet<string> EventIdSet = new();

        private async Task<bool> Connect(Uri uri)
        {
            using var webSocket = new ClientWebSocket();
            try
            {
                await webSocket.ConnectAsync(uri, CancellationToken.None);
            }
            catch
            {
                return true;
            }
            

            while (true)
            {
                var buffer = new byte[64 * 1024 * 1024];
                var segment = new ArraySegment<byte>(buffer);
                WebSocketReceiveResult result;
                try
                {
                    result = await webSocket.ReceiveAsync(segment, CancellationToken.None);
                }
                catch
                {;
                    return true;
                }

                if (result.MessageType == WebSocketMessageType.Close) return true;

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count).Trim();
                var messageData = message[2..];
                if (message[..2] is not ("E:" or "T:")) continue;

                var eew = EewMonitor.Parse(messageData);
                var eventArgs = new EewUpdatedEventArgs<EEW>(eew);
                if (EventIdSet.Contains(eew.QuakeId)) EewUpdatedForContinue?.Invoke(this, eventArgs);
                else EewUpdatedForFirst?.Invoke(this, eventArgs);
            }
        }

        public void UpdateTime(DateTime? dateTime)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EewUpdatedEventArgs<EEW>>? EewUpdatedForFirst;
        public event EventHandler<EewUpdatedEventArgs<EEW>>? EewUpdatedForContinue;
    }
}
