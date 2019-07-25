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
        /// <param name="tranid">事务id，断点续传的文件，事务id应该相同</param>
        /// <param name="filename"></param>
        /// <param name="length">文件的大小</param>
        /// <param name="isContinue">是否断点续传</param>
        void OnBeginUploadFile(int tranid, string filename, int length,bool isContinue);

        /// <summary>
        /// 接收到文件内容
        /// </summary>
        /// <param name="tranid">事务id，断点续传的文件，事务id应该相同</param>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <param name="length">文件总长度</param>
        /// <param name="filePosition">接收到的数据所在的position</param>
        void OnReceivedFileContent(int tranid, string filename, byte[] data, int length, int filePosition);

        /// <summary>
        /// 文件上传完毕
        /// </summary>
        /// <param name="tranid">事务id，断点续传的文件，事务id应该相同</param>
        /// <param name="filename"></param>
        void OnUploadCompleted(int tranid, string filename);

        /// <summary>
        /// 传输错误
        /// </summary>
        /// <param name="tranid"></param>
        /// <param name="filename"></param>
        void OnError(int tranid, string filename);
    }
}
