// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using A2v10.Identity.Ua;
using A2v10.Data.Interfaces;
using A2v10.Data;
using System.Configuration;

namespace A2v10.Identity.Site.Controllers
{
	public class NullProfiler : IDataProfiler
	{
		public IDisposable Start(String command)
		{
			return null;
		}
	}

	public class NullLocalizer : IDataLocalizer
	{
		public String Localize(String content)
		{
			return content;
		}
	}

	public class SimpleDataConfiguration : IDataConfiguration
	{
		public String ConnectionString(string source)
		{
			if (String.IsNullOrEmpty(source))
				source = "Default";
			return ConfigurationManager.ConnectionStrings[source].ConnectionString;
		}
	}

	public class HomeController : Controller
	{
		IIdentityIdGovUa _ident;
		IDbContext _dbContext;

		public HomeController()
		{

			var prof = new NullProfiler();
			var localizer = new NullLocalizer();
			var config = new SimpleDataConfiguration();
			_dbContext = new SqlDbContext(prof, config, localizer);
			_ident = new IdentityGovUa(_dbContext);
		}

		public ActionResult Index()
		{
			return View();
		}

		public async Task<ActionResult> Answer()
		{
			var url = Request.Url;
			if (String.IsNullOrEmpty(url.Query))
				return View("About");

			var rr = await _ident.ProcessCallback(url.ToString(), userId: 99);

			if (rr.status == "success")
				return Redirect(rr.url);

			return View("Error");
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}

		[HttpPost]
		public async Task<ActionResult> GetUrl()
		{
			var rr = await _ident.IdentityUrlAction(userId:99);
			return Json(rr);
		}

		[HttpPost] 
		public async Task<ActionResult> ProcessUrl(String url)
		{
			var rr = await _ident.ProcessCallback(url, userId: 99);
			return Json(rr);
		}
	}
}