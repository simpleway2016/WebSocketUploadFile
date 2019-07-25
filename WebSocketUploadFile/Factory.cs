using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketUploadFile
{
    public static class Factory
    {
        static IApplicationBuilder Application;
        static IHostingEnvironment Environment;
        static int transcationId = 0;
        /// <summary>
        /// 启用WebSocketUploadFile
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public static void Enable(IApplicationBuilder app, IHostingEnvironment env)
        {
            Application = app;
            Environment = env;
            app.UseWebSockets();

            app.Use((context,next)=> {
                if (context.WebSockets.IsWebSocketRequest && context.Request.Query.ContainsKey("WebSocketUploadFile"))
                {
                    //do something 
                    var t = context.WebSockets.AcceptWebSocketAsync();
                    t.Wait();
                    WebSocket webSocket = t.Result;
                    return ProcessWebSocketRequest(webSocket);
                }

                return next();
            });
        }

        static async Task ProcessWebSocketRequest(WebSocket socket)
        {
            var bs = new byte[2048];
            while (true)
            {
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        ArraySegment<byte> buffer = new ArraySegment<byte>(bs);
                        WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);
  
                        if( result.MessageType == WebSocketMessageType.Text )
                        {
                            string jsonString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                            var header = Newtonsoft.Json.JsonConvert.DeserializeObject<UploadHeader>(jsonString);
                            if (header.tranid == null)
                            {
                                header.tranid = System.Threading.Interlocked.Add(ref transcationId, 1);
                            }

                            var outputTranIdBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(header.tranid.ToString()));
                            socket.SendAsync(outputTranIdBuffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait();

                            await new UploadHandler(Application, Environment, header, socket).Process();
                            return;
                            
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
