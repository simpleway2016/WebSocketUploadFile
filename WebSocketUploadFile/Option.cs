using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketUploadFile
{
    public class Option
    {

		private long _MaxFileLength = 1024*1024*20;
		/// <summary>
		/// 文件大小的上限，默认20m
		/// </summary>
		public long MaxFileLength
		{
			get => _MaxFileLength;
			set
			{
				if (_MaxFileLength != value)
				{
					_MaxFileLength = value;
				}
			}
		}
    }
}
