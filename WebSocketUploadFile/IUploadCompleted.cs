using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketUploadFile
{
    public interface IUploadCompleted
    {
        void OnUploadCompleted(UploadHeader header);
    }
}
