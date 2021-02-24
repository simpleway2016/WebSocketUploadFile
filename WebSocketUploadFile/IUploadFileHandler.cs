using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketUploadFile
{
    /// <summary>
    /// 处理上传的文件
    /// </summary>
    public interface IUploadFileHandler
    {
        /// <summary>
        /// 开始文件的传输
        /// </summary>
        /// <param name="header"></param>
        /// <param name="isContinue">是否断点续传</param>
        void OnBeginUploadFile(UploadHeader header,bool isContinue);

        /// <summary>
        /// 接收到文件内容
        /// </summary>
        /// <param name="header"></param>
        /// <param name="data"></param>
        /// <param name="length">data的长度</param>
        /// <param name="filePosition">接收到的数据所在的position</param>
        void OnReceivedFileContent(UploadHeader header, byte[] data, int length, long filePosition);

        /// <summary>
        /// 文件上传完毕
        /// </summary>
        /// <param name="header"></param>
        void OnUploadCompleted(UploadHeader header);

        /// <summary>
        /// 传输错误
        /// </summary>
        /// <param name="header"></param>
        void OnError(UploadHeader header);
    }
}
