// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Specialized;

namespace A2v10.Identity.Ua
{
	public static class HttpUtils
	{
		public static NameValueCollection ParseQueryString(String query)
		{
			var lims = new Char[] { '?', ' ' };
			NameValueCollection nvc = new NameValueCollection();
			var seg = query.Split('&');
			foreach (string s in seg)
			{
				if (String.IsNullOrEmpty(s))
					continue;
				var parts = s.Split('=');
				if (parts.Length > 1)
				{
					var key = parts[0].Trim(lims);
					var val = parts[1].Trim();
					nvc.Add(key, val);
				}
			}
			return nvc;
		}
	}
}
