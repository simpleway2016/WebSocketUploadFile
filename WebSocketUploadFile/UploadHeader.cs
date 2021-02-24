using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketUploadFile
{
    public class UploadHeader
    {

        public HttpContext HttpContext
        {
            get;
            internal set;
        }

        private int? _tranid;
        /// <summary>
        /// 事务id，断点续传的文件，事务id应该相同
        /// </summary>
        public int? TranId
        {
            get => _tranid;
            set
            {
                if (_tranid != value)
                {
                    _tranid = value;
                }
            }
        }


        private string _auth;
        public string Auth
        {
            get => _auth;
            set
            {
                if (_auth != value)
                {
                    _auth = value;
                }
            }
        }

        private string _state;
        public string State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                }
            }
        }


        private string _FilePath;
        public string FilePath
        {
            get => _FilePath;
            set
            {
                if (_FilePath != value)
                {
                    _FilePath = value;
                }
            }
        }

        private string _filename;
        public string FileName
        {
            get => _filename;
            set
            {
                if (_filename != value)
                {
                    _filename = value;
                }
            }
        }


        private long _length;
        /// <summary>
        /// 文件的大小
        /// </summary>
        public long Length
        {
            get => _length;
            set
            {
                if (_length != value)
                {
                    _length = value;
                }
            }
        }


        private long _position;
        public long Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                }
            }
        }
    }
}
