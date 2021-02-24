using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketUploadFile
{
    enum ErrorCode:int
    {
        TooBig = 601,
        ServerError = 500,
        NoContinue = 603
    }
}
