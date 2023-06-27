using DbDiffChecker.Data;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text.Json;
using DbDiffChecker.Service.DbDesign;
using System.IO;
using DbDiffChecker.Web.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json.Linq;
using System.Configuration;
using ConfigurationManager = Microsoft.Extensions.Configuration.ConfigurationManager;
using DbDiffChecker.Web.Helpers;
using Microsoft.Extensions.Options;

namespace DbDiffChecker.Web.Controllers
{
    public class HomeController : Controller
    {
        #region Initialize
        private readonly ILogger<HomeController> _logger;
        private readonly IDbDesignService _dbDesignService;

        public HomeController(
            ILogger<HomeController> logger,
            IDbDesignService dbDesignService)
        {
            _logger = logger;
            _dbDesignService = dbDesignService;
        }

        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region Install

        [Route("install")]
        public IActionResult InstallApp()
        {
            return View();
        }

        [HttpPost]
        [Route("install")]
        public IActionResult InstallApp(SaveSettingsRequest request)
        {
            if (!request.ProdDbSettings.IsValid || !request.UatDbSettings.IsValid)
            {
                ModelState.AddModelError("", "Please fill all your database connection string settings!");
                return View();
            }

            #region UAT Connection String Check and Initialize
            SqlConnectionStringBuilder uatConnectionStringBuilder = new SqlConnectionStringBuilder();
            uatConnectionStringBuilder["Server"] = request.UatDbSettings.Server;
            uatConnectionStringBuilder["Database"] = request.UatDbSettings.Name;
            uatConnectionStringBuilder["User Id"] = request.UatDbSettings.Username;
            uatConnectionStringBuilder["Password"] = request.UatDbSettings.Password;
            if (request.UatDbSettings.TrustedConnection)
                uatConnectionStringBuilder["Trusted_Connection"] = true;
            if (request.UatDbSettings.TrustServerCertificate)
                uatConnectionStringBuilder["TrustServerCertificate"] = true;
            if (request.UatDbSettings.MarsEnabled)
                uatConnectionStringBuilder["MultipleActiveResultSets"] = true;

            using (SqlConnection cnn = new SqlConnection(uatConnectionStringBuilder.ConnectionString))
            {
                try
                {
                    cnn.Open();
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Your uat connectionstring is invalid!");
                    return View();
                }
                finally { cnn.Close(); }
            }
            #endregion

            #region Production Connection String Check and Initialize
            SqlConnectionStringBuilder prodConnectionStringBuilder = new SqlConnectionStringBuilder();
            prodConnectionStringBuilder["Server"] = request.ProdDbSettings.Server;
            prodConnectionStringBuilder["Database"] = request.ProdDbSettings.Name;
            prodConnectionStringBuilder["User Id"] = request.ProdDbSettings.Username;
            prodConnectionStringBuilder["Password"] = request.ProdDbSettings.Password;
            if (request.ProdDbSettings.TrustedConnection)
                prodConnectionStringBuilder["Trusted_Connection"] = true;
            if (request.ProdDbSettings.TrustServerCertificate)
                prodConnectionStringBuilder["TrustServerCertificate"] = true;
            if (request.ProdDbSettings.MarsEnabled)
                prodConnectionStringBuilder["MultipleActiveResultSets"] = true;

            using (SqlConnection cnn = new SqlConnection(prodConnectionStringBuilder.ConnectionString))
            {
                try
                {
                    cnn.Open();
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Your production connectionstring is invalid!");
                    return View();
                }
                finally { cnn.Close(); }
            }
            #endregion

            /*var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");*/
            AppSettingsUpdateHelper.AddOrUpdateAppSetting("ConnectionStrings:ConnectionString_UAT", uatConnectionStringBuilder.ConnectionString/*, environment*/);
            AppSettingsUpdateHelper.AddOrUpdateAppSetting("ConnectionStrings:ConnectionString_Prod", prodConnectionStringBuilder.ConnectionString/*, environment*/);
            return Redirect("/install-successful");
        }

        [Route("install-successful")]
        public IActionResult InstallSuccessful()
        {
            return View();
        }

        #endregion

        #region UAT-Prod Control Methods
        public IActionResult UatProductionDbDesignDiffs()
        {
            var data = _dbDesignService.GetAllTablesWithColumns();
            return View(data);
        }

        public IActionResult UatProductionDbDataDiffs(string id)
        {
            var data = _dbDesignService.UatProductionDbDataDiffs(id);
            if (!data.IsSucccesful)
            {
                TempData["ErrorMessage"] = data.ErrorMessage;
                return View();
            }
            ViewBag.TableName = id;
            ViewBag.PrimaryKey = data.AdditionalData;
            return View(data.Data);
        }

        [HttpPost]
        public JsonResult UpdateChanges(string table, List<ChangeModel> model)
        {
            var data = _dbDesignService.UpdateChanges(table, model);
            return Json(data);
        }

        [HttpPost]
        public IActionResult DownloadSqlFile(string table, List<ChangeModel> model)
        {
            var data = _dbDesignService.CreateSqlFile(table, model);
            if (!data.IsSucccesful)
            {
                TempData["ErrorMessage"] = data.ErrorMessage;
                return View();
            }
            return File(System.IO.File.ReadAllBytes(data.Data), "application/sql", Path.GetFileName(data.Data));
        }
        #endregion

        #region Other

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion
    }
}