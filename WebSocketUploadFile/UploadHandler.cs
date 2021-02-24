using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace WebSocketUploadFile
{
    class UploadHandler
    {
        IApplicationBuilder _app;
        IHostingEnvironment _env;
        WebSocket _socket;
        UploadHeader _header;
        IUploadFileHandler _handler;
        static MyUploadFileHandler MyUploadFileHandler;
        public UploadHandler(IApplicationBuilder app, IHostingEnvironment env,UploadHeader header, WebSocket socket)
        {
            if(MyUploadFileHandler == null)
            {
                MyUploadFileHandler = new MyUploadFileHandler(app);
            }
            _app = app;
            _env = env;
            _header = header;
            _socket = socket;
            _handler = (IUploadFileHandler)app.ApplicationServices.GetService(typeof(IUploadFileHandler));
            if (_handler == null)
            {
                _handler = MyUploadFileHandler;
                MyUploadFileHandler.start();
            }
        }

        public async Task Process()
        {
            if(_handler != null)
            {
                _handler.OnBeginUploadFile(_header , _header.Position == 0 ? false : true);
            }
            var lastReportTime = DateTime.Now.AddSeconds(-100);
            var bs = new byte[204800];
            bool finished = false;
            while (true)
            {
                if (_socket.State == WebSocketState.Open)
                {
                    try
                    {
                        ArraySegment<byte> buffer = new ArraySegment<byte>(bs);
                        WebSocketReceiveResult result = await _socket.ReceiveAsync(buffer, CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            this.onReceived(this._header.TranId.GetValueOrDefault(),_header.FileName, buffer.Array, result.Count , _header.Position);
                            _header.Position += result.Count;

                            if (_header.Position == _header.Length)
                            {
                                this.onFinish(this._header.TranId.GetValueOrDefault(), _header.FileName);
                                finished = true;

                                var outputTranIdBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes("-1"));
                                _socket.SendAsync(outputTranIdBuffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                            }
                            else
                            {
                                if ((DateTime.Now - lastReportTime).TotalSeconds >= 1)
                                {
                                    lastReportTime = DateTime.Now;
                                    var outputTranIdBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(_header.Position.ToString()));
                                    _socket.SendAsync(outputTranIdBuffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                                }
                            }
                                                      
                        }
                    }
                    catch(Exception ex)
                    {
                        var outputTranIdBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(new { code = (int)ErrorCode.ServerError, message = ex.Message })));
                        _socket.SendAsync(outputTranIdBuffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            if(!finished)
            {
                if (_handler != null)
                {
                    _handler.OnError(_header );
                }
            }
        }

        void onReceived(int tranid,string filename, byte[] data, int length ,long filePosition)
        {
            if(_handler != null)
            {
                _handler.OnReceivedFileContent(_header, data, length, filePosition);
            }
        }

        void onFinish(int tranid, string filename)
        {
            if (_handler != null)
            {
                _handler.OnUploadCompleted(_header);
            }
        }
    }
}
