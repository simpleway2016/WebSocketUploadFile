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
        public int? tranid
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


        private string _state;
        public string state
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

        private string _filename;
        public string filename
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


        private int _length;
        /// <summary>
        /// 文件的大小
        /// </summary>
        public int length
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


        private int _position;
        public int position
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
