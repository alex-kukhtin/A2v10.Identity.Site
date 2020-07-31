// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Identity.Ua
{
	public class IdentityItem
	{
		public String Name { get; }
		public String Value { get; }

		public IdentityItem(String name, String value)
		{
			Name = name;
			Value = value;
		}

		public static List<IdentityItem> FromExpando(ExpandoObject exp)
		{
			var l = new List<IdentityItem>();
			var dict = exp as IDictionary<String, Object>;
			foreach (var kv in dict)
			{
				l.Add(new IdentityItem( kv.Key, kv.Value?.ToString()));
			}
			return l;
		}
	}
}
