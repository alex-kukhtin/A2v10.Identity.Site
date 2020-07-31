// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Configuration;
using Newtonsoft.Json;

namespace A2v10.Identity.Ua
{
	public class IdGovUaConfig
	{
		[JsonProperty("url")]
		public String Url { get; set; }

		[JsonProperty("callback")]
		public String Callback { get; set; }

		[JsonProperty("client_id")]
		public String ClientId { get; set; }

		[JsonProperty("secret")]
		public String Secret { get; set; }

		[JsonProperty("return_url")]
		public String ReturnUrl { get; set; }

		public static IdGovUaConfig Create()
		{
			var json = ConfigurationManager.AppSettings["id.gov.ua"];
			return JsonConvert.DeserializeObject<IdGovUaConfig>(json);
		}
	}
}
