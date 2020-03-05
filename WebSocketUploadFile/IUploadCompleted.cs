using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketUploadFile
{
    public interface IUploadCompleted
    {
        void OnFileComing(UploadHeader header);
        void OnUploadCompleted(UploadHeader header);
    }
}
