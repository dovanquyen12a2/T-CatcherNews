using Aspose.Words;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using WebsiteMonitoring;

namespace TCatcherClient.Controllers
{
    public class HomeController : Controller
    {
        private List<string> lstNewsCatalog = new List<string>();
        private string strNewsDatabase = "", strNewsCollection = "";
        #region Index
        public ActionResult Index()
        {
            lstNewsCatalog.Add("AntiVietNam");
            return View();
        }
        [HttpGet]
        public JsonResult StatisticsTarget()
        {
            var db = csGetMongoData.GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_Target>("ImportantTarget");

            try
            {
                var result = collection.AsQueryable()
                    .Where(p => p.NewsCatalog == "AntiVietNam")
                    .Select(p => new { p.SiteAddress })
                    .ToList();

                double dTong = result.Count();
                if (dTong == 0)
                    return Json(new { categories = new string[] { }, series = new object[] { } }, JsonRequestBehavior.AllowGet);

                double blog = 0, facebook = 0, twitter = 0, youtube = 0, net = 0, org = 0, com = 0, other = 0;

                foreach (var row in result)
                {
                    string url = row.SiteAddress?.ToLower() ?? "";
                    if (url.Contains(".blogspot") || url.Contains(".wordpress"))
                    {
                        blog++;
                        continue;
                    }
                    if (url.Contains("facebook.com"))
                    {
                        facebook++;
                        continue;
                    }
                    if (url.Contains("twitter.com"))
                    {
                        twitter++;
                        continue;
                    }
                    if (url.Contains("youtube.com"))
                    {
                        youtube++;
                        continue;
                    }
                    if (url.EndsWith(".net"))
                    {
                        net++;
                        continue;
                    }
                    if (url.EndsWith(".org"))
                    {
                        org++;
                        continue;
                    }
                    if (url.EndsWith(".com"))
                    {
                        com++;
                        continue;
                    }
                    other++;
                }

                // Tính phần trăm
                string fmt(double val) => (Math.Round(val / dTong * 100, 2)).ToString("0.##") + "%";

                var categories = new List<string> { "Blog", ".org", ".net", ".com", "YouTube", "Facebook", "Twitter", "Other" };
                var data = new List<double> { blog, org, net, com, youtube, facebook, twitter, other };
                var labels = new List<string>
                {
                    $"Blog ({fmt(blog)})",
                    $".org ({fmt(org)})",
                    $".net ({fmt(net)})",
                    $".com ({fmt(com)})",
                    $"YouTube ({fmt(youtube)})",
                    $"Facebook ({fmt(facebook)})",
                    $"Twitter ({fmt(twitter)})",
                    $"Other ({fmt(other)})"
                };

                var series = new List<object>
                {
                    new { name = "All Targets", data }
                };

                return Json(new { total = dTong, categories, labels, series }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult StatisticNewsInWeek()
        {
            try
            {
                // Lấy 7 ngày gần nhất (tính từ hôm nay)
                DateTime dtNow = DateTime.Now;
                DateTime dtStart = dtNow.AddDays(-6); // ngày đầu (cách 6 ngày)

                // Danh sách ngày theo định dạng dd/MM
                var categories = Enumerable.Range(0, 7)
                    .Select(i => dtStart.AddDays(i).ToString("dd/MM"))
                    .ToList();

                // Chuẩn bị dữ liệu
                var db = csGetMongoData.GetDatabase(csGlobal.strMongoDatabase);
                var collection = db.GetCollection<Mongo_News>("HighPriority_NewsDetail_AntiVietNam_Lastweek");

                // Tạo danh sách đếm
                var websiteData = new List<int>();
                var youtubeData = new List<int>();
                var mindsData = new List<int>();
                var totalData = new List<int>();

                foreach (var day in Enumerable.Range(0, 7))
                {
                    var date = dtStart.AddDays(day);
                    string from = $"{date:yyyy/MM/dd} 00:00";
                    string to = $"{date:yyyy/MM/dd} 23:59";

                    var website = collection.AsQueryable()
                        .Count(e => !e.SiteAddress.Contains("youtube.com")
                                 && !e.SiteAddress.Contains("minds.com")
                                 && e.PostedDate.CompareTo(from) >= 0
                                 && e.PostedDate.CompareTo(to) <= 0);

                    var youtube = collection.AsQueryable()
                        .Count(e => e.SiteAddress.Contains("youtube.com")
                                 && e.PostedDate.CompareTo(from) >= 0
                                 && e.PostedDate.CompareTo(to) <= 0);

                    var minds = collection.AsQueryable()
                        .Count(e => e.SiteAddress.Contains("minds.com")
                                 && e.PostedDate.CompareTo(from) >= 0
                                 && e.PostedDate.CompareTo(to) <= 0);

                    websiteData.Add(website);
                    youtubeData.Add(youtube);
                    mindsData.Add(minds);
                    totalData.Add(website + youtube + minds);
                }

                var series = new List<object>
            {
                new { name = "Website", data = websiteData },
                new { name = "Youtube", data = youtubeData },
                new { name = "Tổng", data = totalData }
            };

                double total = totalData.Sum();

                return Json(new { total, categories, series }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult StatisticsTopActiveTargetInWeek()
        {
            DateTime dtTo = DateTime.Now;
            DateTime dtFrom = dtTo.Subtract(new TimeSpan(7, 0, 0, 0));

            string strDateTimeToForFilter = $"{dtTo:yyyy/MM/dd} 23:59";
            string strDateTimeFromForFilter = $"{dtFrom:yyyy/MM/dd} 00:00";

            string strNewsDB = csGlobal.strMongoDatabase;
            string strNewsCollection = "HighPriority_NewsDetail_AntiVietNam_Lastweek";

            var dbNews = csGetMongoData.GetDatabase(strNewsDB);
            var collectionNews = dbNews.GetCollection<Mongo_News>(strNewsCollection);

            var result = collectionNews.AsQueryable()
                .Where(e => e.PostedDate.CompareTo(strDateTimeFromForFilter) >= 0
                         && e.PostedDate.CompareTo(strDateTimeToForFilter) <= 0)
                .Select(p => new { p.SiteAddress, p.PostedDate })
                .OrderBy(p => p.PostedDate)
                .ToList();

            var lstTargetTemp = new List<string>();
            var lstTargetCounterTemp = new List<int>();

            foreach (var row in result)
            {
                string site = row.SiteAddress;
                int idx = lstTargetTemp.IndexOf(site);
                if (idx < 0)
                {
                    lstTargetTemp.Add(site);
                    lstTargetCounterTemp.Add(1);
                }
                else
                {
                    lstTargetCounterTemp[idx]++;
                }
            }

            var lstTargetCounter = new List<string>();
            for (int i = 0; i < lstTargetTemp.Count; i++)
            {
                string title;
                if (lstTargetTemp[i].Contains("facebook.com"))
                    title = "Fb " + csGetMongoData.GetTargetTitle(lstTargetTemp[i], "ImportantTarget");
                else if (lstTargetTemp[i].Contains("youtube.com"))
                    title = "Yt " + csGetMongoData.GetTargetTitle(lstTargetTemp[i], "ImportantTarget");
                else if (lstTargetTemp[i].Contains("minds.com"))
                    title = "Minds " + csGetMongoData.GetMindsAccountName(lstTargetTemp[i], "MindsAccount");
                else
                    title = lstTargetTemp[i].Replace("https://", "").Replace("http://", "");

                lstTargetCounter.Add($"{lstTargetCounterTemp[i]:D6};{title}");
            }

            lstTargetCounter.Sort();

            int iMaxDisplay = 10;
            if (iMaxDisplay > lstTargetCounter.Count)
                iMaxDisplay = lstTargetCounter.Count;

            var categories = new List<string>();
            var series = new List<int>();

            for (int i = lstTargetCounter.Count - 1; i >= lstTargetCounter.Count - iMaxDisplay; i--)
            {
                string[] arr = lstTargetCounter[i].Split(';');
                string label = arr[1];
                if (label.Length > 30)
                    label = label.Substring(0, 30);
                categories.Add(label);
                series.Add(int.Parse(arr[0]));
            }

            return Json(new { categories, series }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet]
        public JsonResult TagCloudData()
        {
            try
            {
                List<string> lstHotkey = new List<string>();
                csGetMongoData.GetListKeyword(ref lstHotkey, "Hotkey");

                for (int i = lstHotkey.Count - 1; i >= 0; i--)
                {
                    if (csSupport.WordCount(lstHotkey[i]) > 4)
                        lstHotkey.RemoveAt(i);
                }

                DateTime dt = DateTime.Now.Subtract(new TimeSpan(24, 0, 0, 0));
                string strDate = $"{dt:yyyy/MM/dd HH:mm}";

                var db = csGetMongoData.GetDatabase(csGlobal.strMongoDatabase);
                var collection = db.GetCollection<Mongo_News>("HighPriority_NewsDetail_AntiVietNam_Lastweek");

                var result = collection.AsQueryable()
                    .Where(p => p.PostedDate.CompareTo(strDate) >= 0)
                    .Select(p => new { p.NewsContent })
                    .ToList();

                var foundKeywords = new List<string>();

                if (result.Count > 0)
                {
                    foreach (var row in result)
                    {
                        if (string.IsNullOrEmpty(row.NewsContent)) continue;
                        string content = row.NewsContent.ToLower();

                        foreach (var key in lstHotkey)
                        {
                            var k = key.ToLower();
                            if (content.Contains(k))
                                foundKeywords.Add(k);
                        }
                    }
                }

                var freq = foundKeywords
                    .GroupBy(k => k)
                    .Select(g => new { word = g.Key, count = g.Count() })
                    .OrderByDescending(x => x.count)
                    .Take(60) // 👈 Giới hạn top 100 từ
                    .ToList();

                if (!freq.Any())
                    return Json(new { words = new object[] { } }, JsonRequestBehavior.AllowGet);

                int minCount = freq.Min(x => x.count);
                int maxCount = freq.Max(x => x.count);
                int minSize = 12;
                int maxSize = 48;

                var words = freq.Select(x => new
                {
                    text = x.word,
                    count = x.count,
                    size = (maxCount == minCount)
                        ? (minSize + maxSize) / 2
                        : (int)Math.Round(minSize + (double)(x.count - minCount) / (maxCount - minCount) * (maxSize - minSize))
                }).ToList();

                return Json(new { words }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetListNewsWeb(int take = 50)
        {
            try
            {
                string strNewsCollection = "TopNews";
                var db = csGetMongoData.GetDatabase(csGlobal.strMongoDatabase);
                var collection = db.GetCollection<Mongo_News>(strNewsCollection);

                // Lấy top `take` bản ghi mới nhất
                var result = collection.AsQueryable()
                .Select(p => new Mongo_News
                {
                    SiteAddress = p.SiteAddress,
                    PostedDate = p.PostedDate,
                    Title = p.Title,
                    URL = p.URL,
                    NewsContent = p.NewsContent
                })
                .OrderByDescending(p => p.PostedDate)
                .Take(take)
                .ToList();

                foreach (var row in result)
                {
                    row.SiteAddress = GetSource(row.SiteAddress, row.Title); // không cần GetRtfUnicodeEscapedString
                }
                // Trả về dưới dạng mảng các object (giữ nguyên PostedDate string nếu DB đang lưu chuỗi)
                return Json(new { items = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        private string GetSource(string strSiteAddress, string strTitle)
        {
            string strResult = "Anonymous";
            if (strSiteAddress.Contains("youtube.com"))
            {
                int index = strTitle.IndexOf(" - ");
                if (index >= 0) strResult = strTitle.Substring(0, index).Trim();
                return strResult;
            }
            if (strSiteAddress.Contains("minds.com"))
            {
                strResult = "@" + strSiteAddress.Replace("https://www.minds.com/", "");
                return strResult;
            }
            if (strSiteAddress.Contains("facebook.com"))
            {
                //strResult = "@" + strSiteAddress.Replace("https://www.minds.com/", "");
                return strResult;
            }
            if (strSiteAddress.Contains("twitter.com"))
            {
                strResult = "@" + strSiteAddress.Replace("https://www.minds.com/", "");
                return strResult;
            }
            //website
            strResult = strSiteAddress.Replace("https://", "").Replace("http://", "").Replace("www.", "");
            int index1 = strResult.IndexOf(".");
            if (index1 >= 0) strResult = strResult.Substring(0, index1);
            return strResult;
        }

        #endregion
        #region NewsViewer
        public ActionResult NewsViewer()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetDataNewsViewer(string tuNgay , string denNgay, string mainKeyword, string combineKeyword, string expectKeyword, int source)
        {
            try
            {

                DateTime tuNgayValue;
                DateTime denNgayValue;

                if (string.IsNullOrEmpty(tuNgay) || string.IsNullOrEmpty(denNgay))
                {
                    tuNgayValue = DateTime.Today; // hôm nay
                    denNgayValue = DateTime.Today; // hôm nay

                }
                else
                {
                    var format = "dd/MM/yyyy";
                    var culture = CultureInfo.InvariantCulture; // hoặc CultureInfo.GetCultureInfo("vi-VN")

                    if (!DateTime.TryParseExact(tuNgay, format, culture, DateTimeStyles.None, out tuNgayValue))
                    {
                        tuNgayValue = DateTime.Today;
                    }

                    if (!DateTime.TryParseExact(denNgay, format, culture, DateTimeStyles.None, out denNgayValue))
                    {
                        denNgayValue = DateTime.Today;
                    }
                }

                DateTime datetimenow = DateTime.Now;
                string strDateTimeFromForFilter = tuNgayValue.Year.ToString() + "/" + tuNgayValue.Month.ToString().PadLeft(2, '0') + "/" +
                                           tuNgayValue.Day.ToString().PadLeft(2, '0') + " 00:00";
                string strDateTimeToForFilter = denNgayValue.Year.ToString() + "/" + denNgayValue.Month.ToString().PadLeft(2, '0') + "/" +
                                          denNgayValue.Day.ToString().PadLeft(2, '0') + " 23:59";
                if (tuNgayValue >= DateTime.Now - new TimeSpan(8, 0, 0, 0))
                {
                    strNewsDatabase = csGlobal.strMongoDatabase;
                    strNewsCollection = "HighPriority_NewsDetail_AntiVietNam_Lastweek";
                }
                else
                {
                    strNewsDatabase = "db_AntiVietNam_News";
                    strNewsCollection = "HighPriority_Priority_NewsDetail";

                }

                string[] arrMainKeywords =mainKeyword.ToLower().Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string[] arrCombineKeywords = combineKeyword.ToLower().Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string[] arrEK = expectKeyword.ToLower().Split(new char[1] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> lstCW = new List<string>();
                if (arrMainKeywords.Length > 0 && arrCombineKeywords.Length > 0)
                {
                    for (int i = 0; i < arrMainKeywords.Length; i++)
                        for (int j = 0; j < arrCombineKeywords.Length; j++)
                            lstCW.Add(arrMainKeywords[i].Trim().Replace("\n", "") + "\n" + arrCombineKeywords[j].Trim().Replace("\n", ""));
                }
                else
                {
                    if (arrMainKeywords.Length > 0)
                    {
                        for (int i = 0; i < arrMainKeywords.Length; i++)
                            lstCW.Add(arrMainKeywords[i].Trim().Replace("\n", "") + "\n" + arrMainKeywords[i].Trim().Replace("\n", ""));
                    }
                    else
                    {
                        for (int i = 0; i < arrCombineKeywords.Length; i++)
                            lstCW.Add(arrCombineKeywords[i].Trim().Replace("\n", "") + "\n" + arrCombineKeywords[i].Trim().Replace("\n", ""));
                    }
                }

                if (lstCW.Count > 10)
                {
                    return Json(new { success = false, message = "Too many keywords\r\n(Keyword X combine) must be <= 10" }, JsonRequestBehavior.AllowGet);
                }
                if (arrEK.Length > 5)
                {
                    return Json(new { success = false, message = "Too many keywords\r\n(Expect) must be <= 5" }, JsonRequestBehavior.AllowGet);
                }
                var db = csGetMongoData.GetDatabase(strNewsDatabase);
                var collection = db.GetCollection<Mongo_News>(strNewsCollection);
                var resultCounter = collection.AsQueryable().Where(e => e.PostedDate.CompareTo(strDateTimeFromForFilter) >= 0 && e.PostedDate.CompareTo(strDateTimeToForFilter) <= 0);
                if (source == 0)
                    resultCounter = resultCounter.Where(e => e.SiteAddress.Contains("youtube.com") == false && e.SiteAddress.Contains("minds.com") == false);
                if (source == 1)
                    resultCounter = resultCounter.Where(e => e.SiteAddress.Contains("minds.com"));
                if (source == 2)
                    resultCounter = resultCounter.Where(e => e.SiteAddress.Contains("youtube.com"));

                #region "query with combined keyword"
                if (lstCW.Count == 1)
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[1])));
                if (lstCW.Count == 2)
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[1])));
                if (lstCW.Count == 3)
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[1])));
                if (lstCW.Count == 4)
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[1])));
                if (lstCW.Count == 5)
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[1])));
                if (lstCW.Count == 6)
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[5].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[5].Split('\n')[1])));
                if (lstCW.Count == 7)
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[5].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[5].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[6].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[6].Split('\n')[1])));
                if (lstCW.Count == 8)
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[5].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[5].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[6].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[6].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[7].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[7].Split('\n')[1])));
                if (lstCW.Count == 9)
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[5].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[5].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[6].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[6].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[7].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[7].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[8].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[8].Split('\n')[1])));
                if (lstCW.Count == 10)
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[0].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[1].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[2].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[3].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[4].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[5].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[5].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[6].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[6].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[7].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[7].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[8].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[8].Split('\n')[1])) || (e.NewsContent.ToLower().Contains(lstCW[9].Split('\n')[0]) && e.NewsContent.ToLower().Contains(lstCW[9].Split('\n')[1])));
                #endregion

                #region "query with expection keyword"
                if (arrEK.Length == 1)
                    resultCounter = resultCounter.Where(e => e.NewsContent.ToLower().Contains(arrEK[0]) == false);
                if (arrEK.Length == 2)
                {
                    resultCounter = resultCounter.Where(e => (e.NewsContent.ToLower().Contains(arrEK[0]) == false || e.NewsContent.ToLower().Contains(arrEK[1]) == false));
                }

                if (arrEK.Length == 3)
                    resultCounter = resultCounter.Where(e => e.NewsContent.ToLower().Contains(arrEK[0]) == false || e.NewsContent.ToLower().Contains(arrEK[1]) == false || e.NewsContent.ToLower().Contains(arrEK[2]) == false);
                if (arrEK.Length == 4)
                    resultCounter = resultCounter.Where(e => e.NewsContent.ToLower().Contains(arrEK[0]) == false || e.NewsContent.ToLower().Contains(arrEK[1]) == false || e.NewsContent.ToLower().Contains(arrEK[2]) == false || e.NewsContent.ToLower().Contains(arrEK[3]) == false);
                if (arrEK.Length == 5)
                    resultCounter = resultCounter.Where(e => e.NewsContent.ToLower().Contains(arrEK[0]) == false || e.NewsContent.ToLower().Contains(arrEK[1]) == false || e.NewsContent.ToLower().Contains(arrEK[2]) == false || e.NewsContent.ToLower().Contains(arrEK[3]) == false || e.NewsContent.ToLower().Contains(arrEK[4]) == false);
                #endregion
                var result = resultCounter.OrderByDescending(p => p.PostedDate).ToList();

                foreach (var row in result)
                {
                    row.Title = " (" + GetSource(row.SiteAddress, row.Title) +") " + row.Title; // không cần GetRtfUnicodeEscapedString
                    row.SiteAddress =  " - " + GetNewsContentSummary(row.URL,row.NewsContent).Replace("https://www.youtube.com/v/", "https://www.youtube.com/watch?v=");
                }

                var jsonResult = new JsonResult
                {
                    Data = new
                    {
                        success = true,
                        data = result
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue
                };

                return jsonResult;
            }
            catch (Exception ex){
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public static string GetNewsContentSummary(string strURL, string strNewsContent)
        {
            int index = 0;
            if (strURL.Contains("minds.com") == false)
            {
                index = strNewsContent.IndexOf("http");
                if (index >= 0) index = strNewsContent.IndexOf("\r\n", index + 1);
                if (index > 0) strNewsContent = strNewsContent.Substring(index).Trim();
            }
            strNewsContent = strNewsContent.ToString().Replace("\r\n", "...").Replace("\n", "...").Replace("\t", "...").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Replace("  ", " ");
            if (strNewsContent.Length < 270)
                return strNewsContent;
            else return strNewsContent.Substring(0, 270);
        }
        #endregion

        #region YoutubeMonitor
        public ActionResult YoutubeMonitor()
        {
            return View();
        }
        [HttpGet]
        public ActionResult GetYouTubeMonitor()
        {
            string strDB = csGlobal.strMongoDatabase;
            string strCollection = "HighPriority_NewsDetail_AntiVietNam_Lastweek";
            try
            {
                var db = csGetMongoData.GetDatabase(strDB);
                var collection = db.GetCollection<Mongo_News>(strCollection);
                var result = collection.AsQueryable().Where(e => e.URL.Contains("https://www.youtube.com/")).Select(p => new { p.URL, p.Title, p.PostedDate }).OrderByDescending(p => p.PostedDate).Take(30);

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
