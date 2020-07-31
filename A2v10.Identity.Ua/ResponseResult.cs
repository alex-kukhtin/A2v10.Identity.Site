
using System;

namespace A2v10.Identity.Ua
{
	public class ResponseResult
	{
		public String status { get; private set; }
		public String msg { get; private set; }
		public String url { get; set; }
		public String errorCode { get; set; }
		public String state { get; set; }

		public ResponseResult SetSuccess()
		{
			status = "success";
			return this;
		}

		public ResponseResult SetError(String code, String text)
		{
			status = "error";
			errorCode = code;
			msg = text;
			return this;
		}

		public ResponseResult SetError(String text)
		{
			status = "error";
			msg = text;
			return this;
		}
	}
}
