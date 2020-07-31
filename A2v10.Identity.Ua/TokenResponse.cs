using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Identity.Ua
{
	public class TokenResponse
	{
		[JsonProperty("access_token")]
		public String AccessToken { get; set; }

		[JsonProperty("user_id")]
		public String UserId { get; set; }

		[JsonProperty("token_type")]
		public String TokenType { get; set; }

		[JsonProperty("error")]
		public String ErrorCode { get; set; }

		[JsonProperty("error_description")]
		public String Error { get; set; }

		[JsonProperty("message")]
		public String Message { get; set; }

		public Boolean IsValid()
		{
			return String.IsNullOrEmpty(ErrorCode);
		}
	}
}
