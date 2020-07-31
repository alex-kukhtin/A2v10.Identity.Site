// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.Identity.Ua
{
	public interface IIdentityIdGovUa
	{
		Task<String> IdentityUrl(Int64 userId);
		Task<ResponseResult> IdentityUrlAction(Int64 userId);

		Task<ResponseResult> ProcessCallback(String url, Int64 userId);
	}
}
