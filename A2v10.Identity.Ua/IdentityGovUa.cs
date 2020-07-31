// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Specialized;
using System.Net;

using Newtonsoft.Json;
using A2v10.Data.Interfaces;
using System.Dynamic;

namespace A2v10.Identity.Ua
{
	public class IdentityGovUa : IIdentityIdGovUa
	{
		readonly IDbContext _dbContext;
		readonly IdGovUaConfig _config;

		public IdentityGovUa(IDbContext dbContext)
		{
			_dbContext = dbContext;
			_config = IdGovUaConfig.Create();

		}

		#region IIdentityIdGovUa
		public async Task<String> IdentityUrl(Int64 userId)
		{
			String state = await CreateStateAsync(userId);

			var query = new Dictionary<String, String>()
			{
				["response_type"] = "code",
				["client_id"] = _config.ClientId,
				["auth_type"] = "dig_sign,bank_id,mobile_id",
				["state"] = state
			};
			if (!String.IsNullOrEmpty(_config.Callback))
				query.Add("redirect_uri", _config.Callback);

			String queryString = String.Join("&", query.Select(p => $"{p.Key}={p.Value}"));
			var b = new UriBuilder(_config.Url);
			b.Query = queryString;
			return b.Uri.ToString();
		}

		public async Task<ResponseResult> IdentityUrlAction(Int64 userId)
		{
			ResponseResult rr = new ResponseResult();
			try
			{
				var url = await IdentityUrl(userId);
				rr.SetSuccess();
				rr.url = url;
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null)
					ex = ex.InnerException;
				rr.SetError(ex.Message);
			}
			return rr;
		}


		public async Task<ResponseResult> ProcessCallback(String url, Int64 userId)
		{
			var rr = new ResponseResult();
			try
			{
				var ub = new UriBuilder(url);
				var query = HttpUtils.ParseQueryString(ub.Query);
				var q = ub.Query.Split('&');
				var code = query.Get("code");
				var state = query.Get("state");
				var accessResult = await GetAccessTokenAsync(state, code);
				if (!accessResult.IsValid())
					return rr.SetError(accessResult.ErrorCode, accessResult.Message);
				var identityInfo = await GetIdentityInfo(accessResult);
				await SaveIdentity(identityInfo, userId, state);
				rr.state = state;
				rr.url = _config.ReturnUrl;
				return rr.SetSuccess();
			} 
			catch (Exception ex)
			{
				if (ex.InnerException != null)
					ex = ex.InnerException;
				return rr.SetError(ex.Message);
			}
		}
		#endregion

		async Task<String> CreateStateAsync(Int64 userId)
		{
			// create and save request
			var state = await _dbContext.LoadAsync<RequestState>(null, "a2id.[State.Create]", new { UserId = userId });
			return state.State.ToString().ToLowerInvariant();
		}

		Task SaveIdentity(ExpandoObject identityInfo, Int64 userId, String state)
		{
			var list = IdentityItem.FromExpando(identityInfo);
			var session = Guid.Parse(state);
			return _dbContext.SaveListAsync<IdentityItem>(null, "a2id.[UserInfo.Save]", new { UserId = userId, Session = session}, list);
		}


		String GetTokenUrl(String state, String code)
		{
			var query = new Dictionary<String, String>()
			{
				["grant_type"] = "authorization_code",
				["client_id"] = _config.ClientId,
				["client_secret"] = _config.Secret,
				["code"] = code,
				["state"] = state
			};
			if (!String.IsNullOrEmpty(_config.Callback))
				query.Add("redirect_uri", _config.Callback);

			String queryString = String.Join("&", query.Select(p => $"{p.Key}={p.Value}"));
			var b = new UriBuilder(_config.Url);
			b.Path = "get-access-token";
			b.Query = queryString;
			return b.Uri.ToString();
		}

		String GetInfoUrl(TokenResponse resp)
		{
			var query = new Dictionary<String, String>()
			{
				["access_token"] = resp.AccessToken,
				["user_id"] = resp.UserId
			};
			String queryString = String.Join("&", query.Select(p => $"{p.Key}={p.Value}"));

			var b = new UriBuilder(_config.Url);
			b.Path = "get-user-info";
			b.Query = queryString;
			return b.Uri.ToString();
		}

		async Task<TokenResponse> GetAccessTokenAsync(String state, String code)
		{
			try
			{
				var wr = WebRequest.CreateHttp(GetTokenUrl(state, code));
				wr.UserAgent = "A2v10.Application";
				wr.Method = "POST";
				wr.ContentType = "application/json;charset=utf8";
				wr.ContentLength = 0;
				using (var resp = await wr.GetResponseAsync())
				{
					using (var rs = resp.GetResponseStream())
					{
						using (var ms = new StreamReader(rs))
						{
							String strResult = ms.ReadToEnd();
							return JsonConvert.DeserializeObject<TokenResponse>(strResult);
						}
					}
				}
			}
			catch (WebException wex)
			{
				if (wex.Response != null)
				{
					using (var rs = new StreamReader(wex.Response.GetResponseStream()))
					{
						String strError = rs.ReadToEnd();
						return JsonConvert.DeserializeObject<TokenResponse>(strError);
					}
				}
			}
			return null;
		}

		async Task<ExpandoObject> GetIdentityInfo(TokenResponse token)
		{
			try
			{
				var wr = WebRequest.CreateHttp(GetInfoUrl(token));
				wr.UserAgent = "A2v10.Application";
				wr.Method = "POST";
				wr.ContentType = "application/json;charset=utf8";
				wr.ContentLength = 0;
				using (var resp = await wr.GetResponseAsync())
				{
					using (var rs = resp.GetResponseStream())
					{
						using (var ms = new StreamReader(rs))
						{
							String strResult = ms.ReadToEnd();
							return JsonConvert.DeserializeObject<ExpandoObject>(strResult);
						}
					}
				}
			}
			catch (WebException wex)
			{
				if (wex.Response != null)
				{
					using (var rs = new StreamReader(wex.Response.GetResponseStream()))
					{
						String strError = rs.ReadToEnd();
						return JsonConvert.DeserializeObject<ExpandoObject>(strError);
					}
				}
			}
			return null;
		}
	}
}
