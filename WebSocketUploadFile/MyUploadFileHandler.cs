using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace WebSocketUploadFile
{
    class MyUploadFileHandler : IUploadFileHandler
    {
        class FileOwner
        {
            public FileStream Stream;
            public DateTime LastWriteTime = DateTime.Now;
        }

        Dictionary<int, FileOwner> _dict = new Dictionary<int, FileOwner>();

        IApplicationBuilder _app;
        public MyUploadFileHandler(IApplicationBuilder app)
        {
            _app = app;
           
        }

        internal void start()
        {
            new Thread(checkDeleteFile).Start();
        }

        /// <summary>
        /// 检查无用的文件流
        /// </summary>
        void checkDeleteFile()
        {
            while(true)
            {
                try
                {
                    bool sleep = true;
                    foreach ( var item in _dict )
                    {
                        if((DateTime.Now - item.Value.LastWriteTime).TotalHours >= 1)
                        {
                            try
                            {
                                _dict.Remove(item.Key);
                                var filepath = item.Value.Stream.Name;
                                item.Value.Stream.Dispose();
                                File.Delete(filepath);
                            }
                            catch 
                            {
                            }
                            sleep = false;
                            break;
                        }
                    }

                    if (!sleep)
                        continue;

                    if (_dict.Count == 0)
                        Thread.Sleep(1000*60*60);
                    else
                        Thread.Sleep(1000*60);
                }
                catch 
                {

                }
            }
        }

        public void OnBeginUploadFile(WebSocketUploadFile.UploadHeader header, bool isContinue)
        {
            if (_dict.ContainsKey(header.TranId.Value) == false)
            {
                if(header.Position > 0)
                {
                    throw new Exception("服务器已经没有此文件的续传记录");
                }
                if (System.IO.Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}Temp/WebSocketUploadFile") == false)
                {
                    System.IO.Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}Temp/WebSocketUploadFile");
                }
                string filepath = $"{AppDomain.CurrentDomain.BaseDirectory}Temp/WebSocketUploadFile/{Guid.NewGuid().ToString("N")}";
                _dict[header.TranId.Value] = new FileOwner
                {
                    Stream = System.IO.File.Create(filepath)
                };
            }
        }

        public void OnError(WebSocketUploadFile.UploadHeader header)
        {

        }

        public void OnReceivedFileContent(WebSocketUploadFile.UploadHeader header, byte[] data, int length, long filePosition)
        {
            var owner = _dict[header.TranId.Value];
            owner.LastWriteTime = DateTime.Now;
            owner.Stream.Seek(filePosition, System.IO.SeekOrigin.Begin);
            owner.Stream.Write(data, 0, length);
        }

        public void OnUploadCompleted(WebSocketUploadFile.UploadHeader header)
        {
          
            var stream = _dict[header.TranId.Value].Stream;
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
