﻿using Microsoft.AspNetCore.Builder;
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
        public UploadHandler(IApplicationBuilder app, IHostingEnvironment env,UploadHeader header, WebSocket socket)
        {
            _app = app;
            _env = env;
            _header = header;
            _socket = socket;
            _handler = (IUploadFileHandler)app.ApplicationServices.GetService(typeof(IUploadFileHandler));
        }

        public async Task Process()
        {
            if(_handler != null)
            {
                _handler.OnBeginUploadFile(_header.tranid.GetValueOrDefault(), _header.filename, _header.length,_header.position == 0 ? false : true);
            }
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
                            this.onReceived(this._header.tranid.GetValueOrDefault(),_header.filename, buffer.Array, result.Count , _header.position);
                            _header.position += result.Count;

                            if (_header.position == _header.length)
                            {
                                this.onFinish(this._header.tranid.GetValueOrDefault(), _header.filename);
                                finished = true;

                                var outputTranIdBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes("-1"));
                                _socket.SendAsync(outputTranIdBuffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                            }
                            else
                            {
                                var outputTranIdBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(_header.position.ToString()));
                                _socket.SendAsync(outputTranIdBuffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                            }
                                                      
                        }
                    }
                    catch
                    {
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
                    _handler.OnError(_header.tranid.GetValueOrDefault(), _header.filename);
                }
            }
        }

        void onReceived(int tranid,string filename, byte[] data, int length ,int filePosition)
        {
            if(_handler != null)
            {
                _handler.OnReceivedFileContent(tranid, filename, data, length, filePosition);
            }
        }

        void onFinish(int tranid, string filename)
        {
            if (_handler != null)
            {
                _handler.OnUploadCompleted(tranid, filename);
            }
        }
    }
}