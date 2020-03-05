using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketUploadFile
{
    class MyUploadFileHandler : IUploadFileHandler
    {
        Dictionary<int, System.IO.FileStream> _dict = new Dictionary<int, System.IO.FileStream>();

        IApplicationBuilder _app;
        public MyUploadFileHandler(IApplicationBuilder app)
        {
            _app = app;
        }
        public void OnBeginUploadFile(WebSocketUploadFile.UploadHeader header, bool isContinue)
        {
            if (_dict.ContainsKey(header.TranId.Value) == false)
            {
                if(System.IO.Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}WebSocketUploadFileTemp") == false)
                {
                    System.IO.Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}WebSocketUploadFileTemp");
                }
                string filepath = $"{AppDomain.CurrentDomain.BaseDirectory}WebSocketUploadFileTemp/{Guid.NewGuid().ToString("N")}";
                _dict[header.TranId.Value] = System.IO.File.Create(filepath);
            }
        }

        public void OnError(WebSocketUploadFile.UploadHeader header)
        {

        }

        public void OnReceivedFileContent(WebSocketUploadFile.UploadHeader header, byte[] data, int length, long filePosition)
        {
            var stream = _dict[header.TranId.Value];
            stream.Seek(filePosition, System.IO.SeekOrigin.Begin);
            stream.Write(data, 0, length);
        }

        public void OnUploadCompleted(WebSocketUploadFile.UploadHeader header)
        {
          
            var stream = _dict[header.TranId.Value];
            string filepath = stream.Name;
            stream.Close();
            stream.Dispose();

            var handler = (IUploadCompleted)_app.ApplicationServices.GetService(typeof(IUploadCompleted));
            if(handler != null)
            {
                try
                {
                    header.FilePath = filepath;
                    handler.OnUploadCompleted(header);
                }
                catch
                {
                }
                finally
                {
                    _dict.Remove(header.TranId.Value);
                    try
                    {
                        System.IO.File.Delete(filepath);
                    }
                    catch 
                    {
                    }
                }
            }
            
        }
    }
}
