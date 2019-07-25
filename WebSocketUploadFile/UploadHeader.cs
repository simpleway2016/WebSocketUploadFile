using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketUploadFile
{
    class UploadHeader
    {

        private int? _tranid;
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
