using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        static int transcationId = 0;
        static Option Option;
        /// <summary>
        /// 启用WebSocketUploadFile
        /// </summary>
        /// <param name="app"></param>
        /// <param name="option"></param>
        public static void Enable(IApplicationBuilder app,Option option = null)
        {
            Application = app;
            app.UseWebSockets();

            if (option != null)
                Option = option;
            else
                Option = new Option();

            app.Use((context,next)=> {
                if (context.WebSockets.IsWebSocketRequest && context.Request.Query.ContainsKey("WebSocketUploadFile"))
                {
                    //do something 
                    var t = context.WebSockets.AcceptWebSocketAsync();
                    t.Wait();
                    WebSocket webSocket = t.Result;
                    return ProcessWebSocketRequest(webSocket , context);
                }

                return next();
            });
        }

        static async Task ProcessWebSocketRequest(WebSocket socket,HttpContext context)
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
                            if(header.Length > Option.MaxFileLength)
                            {
                                var errBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(new { code=(int)ErrorCode.TooBig, message = "文件大小超过上限" })));
                                await socket.SendAsync(errBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                                break;
                            }
                            if (header.TranId == null)
                            {
                                header.TranId = System.Threading.Interlocked.Add(ref transcationId, 1);
                            }
                            
                            header.HttpContext = context;

                            var outputTranIdBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(header.TranId.ToString()));
                            socket.SendAsync(outputTranIdBuffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait();

                            await new UploadHandler(Application, header, socket).Process();
                            return;
                            
                        }
                    }
                    catch(Exception ex)
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
