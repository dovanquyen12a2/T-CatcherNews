using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.Security.Cryptography;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using System.Web;
using HtmlAgilityPack;
namespace WebsiteMonitoring
{
    public class csSupport
    {
        public csSupport()
        { }
        #region {ProcessKeyword //xu ly tu khoa voi toan tu +
        public static string ProcessKeyword(string strKeyword)
        { 
             string[] arr = strKeyword.Split(new char[1]{'+'},StringSplitOptions.RemoveEmptyEntries);
            if(arr.Length==0) return "";
            if (arr.Length > 0) strKeyword = arr[0];
            for (int i = 1; i < arr.Length; i++)
                strKeyword += "+" + arr[i];
            return strKeyword;
        }
        #endregion
        #region {GetWebPageContent}
        static public string GetWebPageContent(string url)
        {
            string result = "";
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
               // myRequest.UserAgent = csGlobal.agent_firefox;
                myRequest.Timeout = 40000;
                WebResponse myResponse = myRequest.GetResponse();
                StreamReader sr = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd();
                sr.Close();
                myResponse.Close();
            }
            catch { result = ""; }
            return result;
        }
        #endregion
        #region GetRtfUnicodeEscapedString
        static public string GetRtfUnicodeEscapedString(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (c <= 0x7f)
                    sb.Append(c);
                else
                    sb.Append("\\u" + Convert.ToUInt32(c) + "?");
            }
            return sb.ToString();
        }
        #endregion
        static public string GetWebPageContent_AutoProxy(string url)
        {
         
            if (url.Contains("https://"))
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
            string result = "";
           
            try
            {
                HttpWebRequest myRequest1 = (HttpWebRequest)WebRequest.Create(url);
              //  myRequest1.Proxy = new WebProxy("127.0.0.1", 9666);
                myRequest1.Timeout = 30000;
                WebResponse myResponse1 = myRequest1.GetResponse();
                StreamReader sr1;
                sr1 = new StreamReader(myResponse1.GetResponseStream(), Encoding.UTF8);
                result = sr1.ReadToEnd();
                sr1.Close();
                myResponse1.Close();
            }
            catch { }
            if (result == "")//thay doi che do proxy
            {
                try
                {
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                    myRequest.Proxy = new WebProxy("127.0.0.1", 9666);
                    myRequest.Timeout = 30000;
                    WebResponse myResponse = myRequest.GetResponse();
                    StreamReader sr;
                    sr = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                    result = sr.ReadToEnd();
                    sr.Close();
                    myResponse.Close();
                }
                catch { }
            }
            return HttpUtility.HtmlDecode(result);
        }
        #region {GetTitle}
        static public string GetTitle(string strPageContent)
        {
            try
            {
                int index1 = 0, index2 = 0;
                index1 = strPageContent.IndexOf("\"og:title\"");
                if (index1 < 0) index1 = strPageContent.IndexOf("'og:title'");
                if (index1 > 0)
                {
                    index2 = index1;
                    do
                    {
                        index1 -= 1;
                        if (strPageContent.Substring(index1, 1) == "<")
                            break;
                    }
                    while (index1 > 0);
                    do
                    {
                        index2 += 1;
                        string str = strPageContent.Substring(index2, 1);
                        if (strPageContent.Substring(index2, 1) == ">")
                            break;
                    }
                    while (index2 < index1 + 500);
                    strPageContent = strPageContent.Substring(index1, index2 - index1);
                    index1 = strPageContent.IndexOf("content=\"");
                    if (index1 < 0) index1 = strPageContent.IndexOf("content='");
                    if (index1 > 0)
                    {
                        index2 = strPageContent.IndexOf("\"", index1 + "content=\"".Length);
                        if (index2 < 0)
                            index2 = strPageContent.IndexOf("'", index1 + "content=\"".Length);
                        if (index2 >= 0)
                            return strPageContent.Substring(index1 + "content=\"".Length, index2 - index1 - "content=\"".Length);
                    }
                }

                index1 = strPageContent.IndexOf("<title>");
                if (index1 > 0)
                {
                    index2 = strPageContent.IndexOf("</title>", index1 + "<title>".Length);
                    if (index2 > 0)
                        return strPageContent.Substring(index1 + "<title>".Length, index2 - index1 - "<title>".Length).Trim();
                }
            }
            catch { }
            return "";
        }
        #endregion
        #region {ConvertTimeFromGoogleSearch}
        static public string ConvertTimeFromGoogleSearch(string strTime)
        {
            try
            {
                //Apr 17, 2016
                //  Jan / Feb / Mar / Apr / May / Jun / Jul / Aug / Sept / Oct / Nov / Dec
                strTime = strTime.ToLower().Replace("jan", "01").Replace("feb", "02").Replace("mar", "03").Replace("apr", "04").Replace("may", "05").Replace("jun", "06")
                                            .Replace("jul", "07").Replace("aug", "08").Replace("sep", "09").Replace("oct", "10").Replace("nov", "11").Replace("dec", "12");
                strTime = strTime.Replace(",", "").Replace("  ", " ").Trim();
                string[] lines = strTime.Split(' ');
                strTime = lines[2] + "/" + lines[0] + "/" + lines[1].PadLeft(2, '0');
            }
            catch { }
            return strTime;
        }
        #endregion
        #region {GetGoogleSearchData_Xpath}
        static public void GetGoogleSearchData_Website(ref List<string> lstImportantDomains, ref List<string> lstWebsite, ref List<string> lstPostTime_Website, ref List<string> lstTitle_Website, ref List<string> lstLink_Website, ref List<string> lstRelevantText_Website, string strContent)
        {
            List<string> pottydomains_temp = new List<string>();
            List<string> importantdomains_temp = new List<string>();
            pottydomains_temp.Add("youtube.com");
            pottydomains_temp.Add("youtube - vn.com");
            pottydomains_temp.Add("facebook.com");

            string strXpath = "//div[contains(@class,'rc')]";
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(strContent);
            List<string> lstTemp = new List<string>();
            try
            {
                foreach (HtmlNode element in document.DocumentNode.SelectNodes(strXpath))
                {
                    lstTemp.Add(element.InnerHtml);
                }
            }
            catch { };
            for (int i = 0; i < lstTemp.Count; i++)
            {
                string strWebsite = "", strTitle = "", strTime = "", strLink = "", strRelevantText = "";
                int index1 = 0, index2 = 0;
                try
                {
                    index1 = lstTemp[i].IndexOf("<a href=\"");
                    if (index1 < 0) continue;
                    index2 = lstTemp[i].IndexOf("\"", index1 + "<a href=\"".Length);
                    if (index2 < 0) continue;
                    strLink = lstTemp[i].Substring(index1 + "<a href=\"".Length, index2 - index1 - "<a href=\"".Length).Trim();
                    if (lstLink_Website.Contains(strLink)) continue;
                    strWebsite = GetDomainName(strLink).Replace("https://", "").Replace("http://", "");
                    if (pottydomains_temp.IndexOf(strWebsite.Replace("www.", "")) >= 0) continue;
                    bool bPottyDomain = true;
                    if (importantdomains_temp.IndexOf(strWebsite.Replace("www.", "")) < 0)
                    {
                        
                        for (int h = 0; h < lstImportantDomains.Count; h++)
                            if (lstImportantDomains[h].Contains(strWebsite.Replace("www.", "")) == true)
                            {
                                bPottyDomain = false;
                                importantdomains_temp.Add(strWebsite.Replace("www.", ""));
                                break;
                            }
                        if (bPottyDomain == true)
                            pottydomains_temp.Add(strWebsite.Replace("www.", ""));
                    }
                    if (bPottyDomain == true) continue;
                    index1 = lstTemp[i].IndexOf("<h3 class=\"");
                    if (index1 > 0)
                    {
                        index1 = lstTemp[i].IndexOf("\">", index1 + "<h3 class=\"".Length);
                        index2 = lstTemp[i].IndexOf("</h3>", index1);
                        if (index2 > 0)
                        {
                            strTitle = lstTemp[i].Substring(index1 + "\">".Length, index2 - index1 - "\">".Length).Trim();
                            strTitle = HttpUtility.HtmlDecode(strTitle);
                        }
                    }
                    index1 = lstTemp[i].IndexOf("<span class=\"f\">");
                    if (index1 > 0)
                    {
                        index2 = lstTemp[i].IndexOf("</span>", index1 + "<span class=\"f\">".Length);
                        if (index2 > 0)
                        {
                            strTime = lstTemp[i].Substring(index1 + "<span class=\"f\">".Length, index2 - index1 - "<span class=\"f\">".Length).Replace("-", "").Trim();
                            strTime = ConvertTimeFromGoogleSearch(strTime);
                            index1 = index2 + "</span>".Length;
                            index2 = lstTemp[i].IndexOf("</span>", index1);
                            if (index2 > 0)
                            {
                                strRelevantText = lstTemp[i].Substring(index1, index2 - index1).Replace("<em>", "").Replace("</em>", "");
                                strRelevantText = HttpUtility.HtmlDecode(strRelevantText);
                            }
                        }
                    }
                }
                catch { }
             //   if (strTime == "") { int kt = 1; }
                index1 = lstWebsite.IndexOf(strWebsite);
                if (index1 < 0)
                {
                    lstWebsite.Add(strWebsite);
                    lstPostTime_Website.Add(strTime);
                    lstTitle_Website.Add(strTitle);
                    lstLink_Website.Add(strLink);
                    lstRelevantText_Website.Add(strRelevantText);
                }
                else
                {
                    index2 = lstWebsite.LastIndexOf(strWebsite);
                    int index = index2;
                    for (int k = index1; k <= index2; k++)
                    {
                        if (strTime.CompareTo(lstPostTime_Website[k]) > 0)
                            continue;
                        else
                        {
                            index = k;
                            break;
                        }
                    }
                    lstWebsite.Insert(index,strWebsite);
                    lstPostTime_Website.Insert(index, strTime);
                    lstTitle_Website.Insert(index, strTitle);
                    lstLink_Website.Insert(index, strLink);
                    lstRelevantText_Website.Insert(index, strRelevantText);
                }
            }
        }
        static public void GetGoogleSearchData_Youtube(ref List<string>  lstImportantYoutubeChannel, ref List<string> lstYoutube, ref List<string> lstPostTime_Youtube, ref List<string> lstTitle_Youtube, ref List<string> lstLink_Youtube, ref List<string> lstRelevantText_Youtube, string strContent)
        {
            List<string> pottychannelyoutube_temp = new List<string>();
            List<string> importantchannelyoutube_temp = new List<string>();

            string strXpath = "//div[contains(@class,'rc')]";
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(strContent);
            List<string> lstTemp = new List<string>();
            try
            {
                foreach (HtmlNode element in document.DocumentNode.SelectNodes(strXpath))
                {
                    lstTemp.Add(element.InnerHtml);
                }
            }
            catch { };
            for (int i = 0; i < lstTemp.Count; i++)
            {
                string strYoutubeChannel = "", strTitle = "", strTime = "", strLink = "", strRelevantText = "";
                int index1 = 0, index2 = 0;
                try
                {
                    index1 = lstTemp[i].IndexOf("<a href=\"");
                    if (index1 < 0) continue;
                    index2 = lstTemp[i].IndexOf("\"", index1 + "<a href=\"".Length);
                    if (index2 < 0) continue;
                    strLink = lstTemp[i].Substring(index1 + "<a href=\"".Length, index2 - index1 - "<a href=\"".Length).Trim();
                    if (lstLink_Youtube.Contains(strLink)) continue;

                    index1 = lstTemp[i].IndexOf("<h3 class=\"");
                    if (index1 > 0)
                    {
                        index1 = lstTemp[i].IndexOf("\">", index1 + "<h3 class=\"".Length);
                        index2 = lstTemp[i].IndexOf("</h3>", index1);
                        if (index2 > 0)
                        {
                            strTitle = lstTemp[i].Substring(index1 + "\">".Length, index2 - index1 - "\">".Length).Trim();
                            strTitle = HttpUtility.HtmlDecode(strTitle);
                        }
                    }

                    index1 = lstTemp[i].IndexOf("<div class=\"slp f\">");
                    if (index1 < 0) continue;
                    if (index1 > 0)
                    {
                        index2 = lstTemp[i].IndexOf("</div>", index1 + "<div class=\"slp f\">".Length);
                        if (index2 > 0)
                        {
                            strTime = lstTemp[i].Substring(index1 + "<div class=\"slp f\">".Length, index2 - index1 - "<div class=\"slp f\">".Length);
                            int index = strTime.IndexOf("-");
                            strYoutubeChannel = strTime.Substring(index + 1).Trim().Replace("Uploaded by", "").Replace("Tải lên bởi", "").Trim();
                           
                            strTime = strTime.Substring(0, index).Trim();
                            strTime = ConvertTimeFromGoogleSearch(strTime);
                        }
                    }
                    index1 = lstTemp[i].IndexOf("<span class=\"st\">");
                    if (index1 > 0)
                    {
                        index2 = lstTemp[i].IndexOf("</span>", index1 + "<span class=\"st\">".Length);
                        if (index2 > 0)
                        {
                            strRelevantText = lstTemp[i].Substring(index1 + "<span class=\"st\">".Length, index2 - index1 - "<span class=\"st\">".Length).Replace("</em>", "").Replace("<em>", "").Trim();
                            strRelevantText = HttpUtility.HtmlDecode(strRelevantText);
                        }
                    }

                    if (pottychannelyoutube_temp.IndexOf(strYoutubeChannel) >= 0) continue;
                    bool bPottyYoutube = true;
                    if (pottychannelyoutube_temp.IndexOf(strYoutubeChannel) < 0)
                    {

                        for (int h = 0; h < lstImportantYoutubeChannel.Count; h++)
                            if (lstImportantYoutubeChannel[h].Contains(strYoutubeChannel) == true)
                            {
                                bPottyYoutube = false;
                                importantchannelyoutube_temp.Add(strYoutubeChannel);
                                break;
                            }
                        if (bPottyYoutube == true)
                            pottychannelyoutube_temp.Add(strYoutubeChannel);
                    }
                    if (bPottyYoutube == true) continue;

                }
                catch { }
                //   if (strTime == "") { int kt = 1; }
                index1 = lstYoutube.IndexOf(strYoutubeChannel);
                if (index1 < 0)
                {
                    lstYoutube.Add(strYoutubeChannel);
                    lstPostTime_Youtube.Add(strTime);
                    lstTitle_Youtube.Add(strTitle);
                    lstLink_Youtube.Add(strLink);
                    lstRelevantText_Youtube.Add(strRelevantText);
                }
                else
                {
                    index2 = lstYoutube.LastIndexOf(strYoutubeChannel);
                    int index = index2;
                    for (int k = index1; k <= index2; k++)
                    {
                        if (strTime.CompareTo(lstPostTime_Youtube[k]) > 0)
                            continue;
                        else
                        {
                            index = k;
                            break;
                        }
                    }
                    lstYoutube.Insert(index, strYoutubeChannel);
                    lstPostTime_Youtube.Insert(index, strTime);
                    lstTitle_Youtube.Insert(index, strTitle);
                    lstLink_Youtube.Insert(index, strLink);
                    lstRelevantText_Youtube.Insert(index, strRelevantText);
                }
            }
        }
        #endregion
        #region {GetAllLinkCrawler_Xpath}
        static public void GetAllLinkCrawler_Xpath(ref List<string> listlink, string strUrl, string strAreaCrawler)
        {
            listlink.Clear();
            string strSiteAddress = "";
            while (1 == 1)
            {
                if (strUrl.Length == 0) break;
                if (strUrl[strUrl.Length - 1] == '/')
                    strUrl = strUrl.Substring(0, strUrl.Length - 1);
                else break;
            }
            int index = strUrl.IndexOf("/", 8);
            if (index > 0) strSiteAddress = strUrl.Substring(0, index);
            else strSiteAddress = strUrl;
            List<string> temp = new List<string>();
            string strContent = "";


            strContent = GetWebPageContent_AutoProxy(strUrl);
            if (strContent == "")
                return;
            strAreaCrawler = strAreaCrawler.Replace("^^^", "\r");
            string[] xpath_arr = strAreaCrawler.Split(new char[1] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            List<string> lstContent = new List<string>();
            document.LoadHtml(strContent);
            for (int i = 0; i < xpath_arr.Length; i++)
            {
                try
                {
                    foreach (HtmlNode element in document.DocumentNode.SelectNodes(xpath_arr[i]))
                    {
                        lstContent.Add(element.InnerHtml);
                        break;
                    }
                }
                catch { }
            }
            for (int i = 0; i < lstContent.Count; i++)
            {
                strContent = lstContent[i].Replace("'","\"");

                MatchCollection mc;
                string pattern1 = @"href=\""(.*?)\""";
                Regex myRegex = new Regex(pattern1);
                mc = myRegex.Matches(strContent);

                List<string> sss = new List<string>();
                string strNewLink = "";
                int iIndexDauThang = 0;
                   List<string> lst = new List<string>();
                foreach (Match m in mc)
                {
                    strNewLink = HttpUtility.UrlDecode(m.Value.Trim().Replace("href=\"", "").Trim());
                    if (strNewLink.Contains(".")==false) continue;
                    // lst.Add(strNewLink);
                   // continue;
                    index = strNewLink.IndexOf("\"");
                    if (index >= 0) strNewLink = strNewLink.Substring(0, index);
                    index = strNewLink.IndexOf(" ");
                    if (index >= 0) strNewLink = strNewLink.Substring(0, index);

                    iIndexDauThang = strNewLink.IndexOf("#");
                    if (iIndexDauThang >= 0) strNewLink = strNewLink.Substring(0, iIndexDauThang);
                    if (strNewLink.Length < 2) continue;
                    if (strNewLink.Contains("/css") || strNewLink.Contains(".css") || strNewLink.Contains(".fcss") || strNewLink.Contains("/search") || strNewLink.Contains("http://www.blogger.com") || strNewLink.Contains(".wordpress.com/category/")
                        || strNewLink.Contains("/rearrange?") || strNewLink.Contains("/?share=") || strNewLink.Contains("/feed") || strNewLink.Contains("/uploads") || strNewLink.Contains("/favicon.ico")
                        || strNewLink.Contains(".jpg") || strNewLink.Contains(".gif") || strNewLink.Contains(".png") || strNewLink.Contains(".mp3") || strNewLink.Contains(".mp4") ||
                        strNewLink.Contains(".flv") || strNewLink.Contains("_archive.html") || strNewLink.Contains("javascript:") || strNewLink.Contains("@") || strNewLink.Contains(".xml")) continue;

                    index = strNewLink.IndexOf("#");
                    if (index >= 0)
                        strNewLink = strNewLink.Substring(0, index);

                    while (1 == 1)
                    {
                        if (strNewLink.Length == 0) break;
                        if (strNewLink[strNewLink.Length - 1] == '/')
                            strNewLink = strNewLink.Substring(0, strNewLink.Length - 1);
                        else break;
                    }
                    if ((strNewLink.Contains("http://") == false) && (strNewLink.Contains("https://") == false))
                        if (strNewLink[0] != '/')
                            strNewLink = strSiteAddress + "/" + strNewLink;
                        else strNewLink = strSiteAddress + strNewLink;

                    if (strNewLink.Length <= 8) continue;
                  
                    string strDomain = strSiteAddress.Replace("https://", "").Replace("http://", "").Replace("www.", "");
                    if (strNewLink.Contains(strDomain) == false) continue;
                    if (strNewLink.Contains("https://www." + strDomain) && strNewLink.IndexOf("https://www." + strDomain) != 0) continue;
                    if (strNewLink.Contains("https://" + strDomain) && strNewLink.IndexOf("https://" + strDomain) != 0) continue;
                    if (strNewLink.Contains ("http://www." + strDomain) && strNewLink.IndexOf("http://www." + strDomain) != 0) continue;
                    if (strNewLink.Contains  ("http://" + strDomain) && strNewLink.IndexOf("http://" + strDomain) != 0) continue;
                    
                    if (listlink.Contains(strNewLink) == false)
                        listlink.Add(strNewLink);
                }
                //int kt = 1;
            }
        }
        #endregion
        #region {GetWebPageContent}
        static public string GetWebPageContent(string url, ref string strCharset, ref string strProxy)
        {
           
            string result = "";

            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.UserAgent = csGlobal.agent_firefox;
                if (strProxy == "true")
                    myRequest.Proxy = new WebProxy("127.0.0.1", 9666);
                else
                    myRequest.Proxy = null;

                myRequest.Timeout = 60000;
                WebResponse myResponse = myRequest.GetResponse();
                StreamReader sr;
                if (strCharset == "utf-8")
                    sr = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                else
                    sr = new StreamReader(myResponse.GetResponseStream(), Encoding.Default);
                result = sr.ReadToEnd();
                sr.Close();
                myResponse.Close();
                if (result == "")//thay doi che do proxy
                {
                    if (strProxy == "true")
                        strProxy = "false";
                    else strProxy = "true";
                    //lay lai noi dung trang web
                    HttpWebRequest myRequest1 = (HttpWebRequest)WebRequest.Create(url);
                    if (strProxy == "true")
                        myRequest1.UserAgent = csGlobal.agent_firefox;
                    else
                        myRequest1.Proxy = null;
                    myRequest1.Timeout = 60000;
                    WebResponse myResponse1 = myRequest1.GetResponse();
                    StreamReader sr1;
                    if (strCharset == "utf-8")
                        sr1 = new StreamReader(myResponse1.GetResponseStream(), Encoding.UTF8);
                    else
                        sr1 = new StreamReader(myResponse1.GetResponseStream(), Encoding.Default);
                    result = sr1.ReadToEnd();
                    sr1.Close();
                    myResponse1.Close();
                }
            }
            catch//thay doi che do proxy
            {
                try
                {
                    result = "";
                    if (strProxy == "true")
                        strProxy = "false";
                    else strProxy = "true";
                    //lay lai noi dung trang web
                    HttpWebRequest myRequest1 = (HttpWebRequest)WebRequest.Create(url);
                    myRequest1.UserAgent = csGlobal.agent_firefox;
                    if (strProxy == "true")
                        myRequest1.Proxy = new WebProxy("127.0.0.1", 9666);
                    else
                        myRequest1.Proxy = null;
                    myRequest1.Timeout = 60000;
                    WebResponse myResponse1 = myRequest1.GetResponse();
                    StreamReader sr1 = null;
                    if (strCharset == "utf-8")
                        sr1 = new StreamReader(myResponse1.GetResponseStream(), Encoding.UTF8);
                    else
                        sr1 = new StreamReader(myResponse1.GetResponseStream(), Encoding.Default);
                    result = sr1.ReadToEnd();
                    sr1.Close();
                    myResponse1.Close();
                }
                catch { return ""; }
            }
            if (result.Contains("charset=iso-") ||
                result.IndexOf("charset=utf-8", StringComparison.CurrentCultureIgnoreCase) > 0 ||  //charset la utf-8
               result.IndexOf("charset=\"utf-8", StringComparison.CurrentCultureIgnoreCase) > 0 ||
               result.IndexOf("charset='utf-8", StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                if (strCharset == "utf-8")  //trung voi charset dua vao
                    return result;
                else
                    strCharset = "utf-8";//charset dua vao la default
            }
            else//charset la default
            {
                if (result.Contains("charset=") == true)
                {
                    if (strCharset == "default")//trung voi charset dua vao
                        return result;
                    else
                        strCharset = "default"; //charset dua vao la utf-8
                }
            }
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.UserAgent = csGlobal.agent_firefox;
                if (strProxy == "true")
                    myRequest.Proxy = new WebProxy("127.0.0.1", 9666);
                else myRequest.Proxy = null;
                myRequest.Timeout = 60000;
                WebResponse myResponse = myRequest.GetResponse();
                StreamReader sr;
                if (strCharset == "utf-8")
                    sr = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                else
                    sr = new StreamReader(myResponse.GetResponseStream(), Encoding.Default);
                result = sr.ReadToEnd();
                sr.Close();
                myResponse.Close();
                if (strCharset == "utf-8" && result.Contains("charset=") == false)
                    return result;
            }
            catch { result = ""; }
            return result;
        }
        #endregion
        #region {GetMiddleDate}
        private DateTime GetMiddleDate(DateTime dtBegin, DateTime dtEnd)
        {
            int count = (dtEnd - dtBegin).Days;
            int middle = count / 2;
            return dtBegin.AddDays(middle);

        }
        #endregion
        #region {DetermineFirstSeenDate}
        private string DetermineFirstSeenDateBasedOnGoogle(string strSite)
        {
            string strURL = "", strResult = "";
            DateTime dtMax = DateTime.Now.AddDays(2);
            DateTime dtMin = new DateTime(1990, 1, 1);
            DateTime dtMaxTemp = GetMiddleDate(dtMin, dtMax);
            string cd_min = "1/1/1990", cd_maxtemp = dtMaxTemp.Day.ToString() + "/" + dtMaxTemp.Month.ToString() + "/" + dtMaxTemp.Year.ToString(),
                                       cd_max = dtMax.Day.ToString() + "/" + dtMax.Month.ToString() + "/" + dtMax.Year.ToString();
            string strFirstSeenDate = "";
            int count01 = 0;
            while (cd_min != cd_max)
            {
                strURL = "https://www.google.com.vn/search?q=site%3A" + strSite + "&biw=1366&bih=635&noj=1&source=lnt&tbs=cdr%3A1%2Ccd_min%3A" + cd_min + "%2Ccd_max%3A" + cd_maxtemp + "&tbm=";
                count01++;
                strResult = csSupport.GetWebPageContentForGoogleSearch(strURL);
                if (strResult == "")
                {
                    int count = 0;
                    while (count < 60)
                    {
                        count++;
                        System.Threading.Thread.Sleep(1000);
                        Application.DoEvents();
                    }
                    continue;
                }
                if (strResult.Contains("Không tìm thấy") && strResult.Contains("trong tài liệu nào"))
                {

                    cd_min = dtMin.Day.ToString().PadLeft(2, '0') + "/" + dtMin.Month.ToString().PadLeft(2, '0') + "/" + dtMin.Year.ToString();
                    cd_maxtemp = dtMaxTemp.Day.ToString().PadLeft(2, '0') + "/" + dtMaxTemp.Month.ToString().PadLeft(2, '0') + "/" + dtMaxTemp.Year.ToString();
                    cd_max = dtMax.Day.ToString().PadLeft(2, '0') + "/" + dtMax.Month.ToString().PadLeft(2, '0') + "/" + dtMax.Year.ToString();
                    if (cd_min == cd_maxtemp)
                    {
                        if (strFirstSeenDate != "")
                            strFirstSeenDate = dtMax.Year.ToString() + "/" + dtMax.Month.ToString().PadLeft(2, '0') + "/" + dtMax.Day.ToString().PadLeft(2, '0');
                        return strFirstSeenDate;
                    }
                    dtMin = dtMaxTemp;
                }
                else
                {
                    cd_min = dtMin.Day.ToString().PadLeft(2, '0') + "/" + dtMin.Month.ToString().PadLeft(2, '0') + "/" + dtMin.Year.ToString();
                    cd_maxtemp = dtMaxTemp.Day.ToString().PadLeft(2, '0') + "/" + dtMaxTemp.Month.ToString().PadLeft(2, '0') + "/" + dtMaxTemp.Year.ToString();
                    cd_max = dtMax.Day.ToString().PadLeft(2, '0') + "/" + dtMax.Month.ToString().PadLeft(2, '0') + "/" + dtMax.Year.ToString();
                    strFirstSeenDate = dtMaxTemp.Year.ToString() + "/" + dtMaxTemp.Month.ToString().PadLeft(2, '0') + "/" + dtMaxTemp.Day.ToString().PadLeft(2, '0');
                    if (cd_min == cd_maxtemp)
                        return strFirstSeenDate;
                    dtMax = dtMaxTemp;
                }
                dtMaxTemp = GetMiddleDate(dtMin, dtMax);
                cd_min = dtMin.Day.ToString() + "/" + dtMin.Month.ToString() + "/" + dtMin.Year.ToString();
                cd_maxtemp = dtMaxTemp.Day.ToString() + "/" + dtMaxTemp.Month.ToString() + "/" + dtMaxTemp.Year.ToString();
                cd_max = dtMax.Day.ToString() + "/" + dtMax.Month.ToString() + "/" + dtMax.Year.ToString();
                System.Threading.Thread.Sleep(3000);
                Application.DoEvents();
            }
            return strFirstSeenDate;
        }
        #endregion
        #region {GetTitleSite}
        static public string GetTitleSite(string strPageContent)
        {
            try
            {
                string strTitle = "";
                int indexBeginTitle = 0, indexEndTitle = 0;
                indexBeginTitle = strPageContent.IndexOf("<title", StringComparison.CurrentCultureIgnoreCase);
                if (indexBeginTitle < 0) return "";
                indexEndTitle = strPageContent.IndexOf("</title>", "<title".Length, StringComparison.CurrentCultureIgnoreCase);
                strTitle = strPageContent.Substring(indexBeginTitle + 7, indexEndTitle - indexBeginTitle - 7).Trim();
                strTitle = strTitle.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ").Trim();
                return strTitle;
            }
            catch { return ""; }
        }
        #endregion
        #region {CheckSiteExist}
        static public void CheckTargetExist(ref List<string> lstSiteAddress)
        {
            var db = csGetMongoData.GetDatabase(csGlobal.strMongoDatabase);
            var collection01 = db.GetCollection<Mongo_Target>("ImportantTarget");
            string strSiteAddress = "";
            for (int i = lstSiteAddress.Count - 1; i >= 0; i--)
            {
                strSiteAddress = lstSiteAddress[i];
                if (collection01.AsQueryable().Where(p => p.SiteAddress == strSiteAddress).Select(p => new { p.SiteAddress }).Count() > 0)
                    lstSiteAddress.RemoveAt(i);
            }
            if (lstSiteAddress.Count == 0) return;

            var collection02 = db.GetCollection<Mongo_NewTarget>("NewTarget");
            for (int i = lstSiteAddress.Count - 1; i >= 0; i--)
            {
                strSiteAddress = lstSiteAddress[i];
                if (collection02.AsQueryable().Where(p => p.SiteAddress == strSiteAddress).Select(p => new { p.SiteAddress }).Count() > 0)
                    lstSiteAddress.RemoveAt(i);
            }
            if (lstSiteAddress.Count == 0) return;

            var collection03 = db.GetCollection<Mongo_NormalTarget>("NormalTarget");
            for (int i = lstSiteAddress.Count - 1; i >= 0; i--)
            {
                strSiteAddress = lstSiteAddress[i];
                if (collection03.AsQueryable().Where(p => p.SiteAddress == strSiteAddress).Select(p => new { p.SiteAddress }).Count() > 0)
                    lstSiteAddress.RemoveAt(i);
            }
        }
        #endregion
        #region {Check IP Viet Nam}
        static public void CheckIPVietNam(ref List<string> lstSiteAddress, List<string> lstNetID)
        {
            string strIPAddress = "", strIPAddressBinary = "";
            for (int i = lstSiteAddress.Count - 1; i >= 0; i--)
            {
                strIPAddress = GetIPAddressFromDomainName(lstSiteAddress[i].Replace("http://", "").Replace("https://", ""));
                if (strIPAddress == "")
                {
                    lstSiteAddress.RemoveAt(i);
                    continue;
                }
                strIPAddressBinary = csSupport.ConvertIPAddressToBinString(strIPAddress);
                if (strIPAddressBinary == "") continue;
                for (int j = 0; j < lstNetID.Count; j++)
                    if (strIPAddressBinary.IndexOf(lstNetID[j]) == 0)
                    {
                        lstSiteAddress.RemoveAt(i);
                        break;
                    }
            }
        }
        static public bool CheckIPVietNam(string strSiteAddress, List<string> lstNetID)
        {
            string strIPAddress = "", strIPAddressBinary = "";
            strIPAddress = GetIPAddressFromDomainName(strSiteAddress.Replace("http://", "").Replace("https://", ""));
            if (strIPAddress == "")
                return false;
            strIPAddressBinary = csSupport.ConvertIPAddressToBinString(strIPAddress);
            if (strIPAddressBinary == "") return false;
            for (int j = 0; j < lstNetID.Count; j++)
                if (strIPAddressBinary.IndexOf(lstNetID[j]) == 0)
                    return true;
            return false;
        }

        #endregion
        #region {SHA-256 with key}
        static public string SHAEncrypt(string strInput)
        {
            string strKey = "tamthoinhutheda";
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(strKey);
            HMACSHA1 hmacsha1 = new HMACSHA1(keyByte);
            byte[] messageBytes = encoding.GetBytes(strInput);
            byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);
            return ByteToString(hashmessage);
        }
        public static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }
        #endregion
        #region {GetWebPageContentForGoogleSearch}
        static public string GetWebPageContentForGoogleSearch(string url)
        {
            string result = "";
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.UserAgent = csGlobal.agent_firefox;
                WebProxy myproxy = new WebProxy("127.0.0.1", 9666);
                myproxy.BypassProxyOnLocal = false;
                myRequest.Proxy = myproxy;

                myRequest.Timeout = 60000;
                WebResponse myResponse = myRequest.GetResponse();
                StreamReader sr;
                sr = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd();
                sr.Close();
                myResponse.Close();
            }
            catch //thay doi che do proxy
            {
              
            }
            return result;
        }
        #endregion
        #region {WordCount}
        static public int WordCount(string str)
        {
            str = str.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
            char[] chars = new char[1];
            chars[0] = ' ';
            string[] wordcount = str.Trim().Split(chars, StringSplitOptions.RemoveEmptyEntries);
            return wordcount.Length;
        }
        #endregion
        #region {DeleteSpace}
        static public string DeleteSpace(string str)
        {
            str = str.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
            char[] chars = new char[1];
            chars[0] = ' ';
            string[] wordcount = str.Trim().Split(chars, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < wordcount.Length; i++)
                strb.Append(wordcount[i] + " ");
            return strb.ToString().Trim();
        }
        #endregion
        #region {CheckConnectByPingMethod}
        static public bool CheckConnectByPingMethod(string url)
        {
            try
            {
                url = url.Replace("https://", "").Replace("http://", "");
                if (new Ping().Send(url).Status == IPStatus.Success)
                    return true;
                else
                    return false;
            }
            catch { return false; }
        }
        #endregion
        #region {GetTime}
        static public string GetTime()
        {
            DateTime dt = DateTime.Now;
            string str = dt.Hour.ToString().PadLeft(2, '0') + dt.Minute.ToString().PadLeft(2, '0') + dt.Second.ToString().PadLeft(2, '0') + dt.Millisecond.ToString().PadLeft(3, '0');
            return str;
        }
        #endregion
        #region {GetListSite}
        static public void GetListSite(string strContent, ref List<string> lstSite)
        {
            lstSite.Clear();
            if (strContent == "") return;
            List<string> temp = new List<string>();
            strContent = strContent.Replace("'", "\"").Replace("\">", "\"").Replace("\" >", "\"").Replace("href=http", "href=\"http").Replace(" >", "\"").Replace(">", "\"");
            ConvertUnicode(ref strContent);
            MatchCollection mc;
            string pattern1 = @"http://(.*?)\""|https://(.*?)\""";
            Regex myRegex = new Regex(pattern1);
            mc = myRegex.Matches(strContent.Replace("'","\""));
            string strSite = "";
            List<string> sss = new List<string>();
            foreach (Match m in mc)
            {
                strSite = m.Value.Trim().Replace("href=\"", "").Trim().ToLower();
                while (strSite.Length > 0 && char.IsLetterOrDigit(strSite, strSite.Length - 1)==false)
                    strSite = strSite.Substring(0, strSite.Length - 1);
                //if(strSite.Contains("toquocmagazine.blogspot.com/feeds/posts/default"))
                //{
                //    int kt=1;
                //}
                if (strSite.Length <= 10) continue;
                if (strSite.Contains(".") == false) continue;
                if (strSite.Contains("http://") == true || strSite.Contains("https://") == true)
                {
                    strSite = GetDomainName(strSite);
                    if (lstSite.Contains(strSite) == false)
                        lstSite.Add(strSite);
                }
            }
        }
        #endregion
        #region {GetListURL}
        static public void GetListURL(string strContent, ref List<string> lstURL)
        {
            lstURL.Clear();
            if (strContent == "") return;
            List<string> temp = new List<string>();
            strContent = strContent.Replace("'", "\"").Replace("\">", "\"").Replace("\" >", "\"").Replace("href=http", "href=\"http").Replace(" >", "\"").Replace(">", "\"");
            ConvertUnicode(ref strContent);
            MatchCollection mc;
            string pattern1 = @"http://(.*?)\""|https://(.*?)\""";
            Regex myRegex = new Regex(pattern1);
            mc = myRegex.Matches(strContent.Replace("'","\""));
            string strSite = "";
            List<string> sss = new List<string>();
            foreach (Match m in mc)
            {
                strSite = m.Value.Trim().Replace("href=\"", "").Trim().ToLower();
                while (strSite.Length > 0 && char.IsLetterOrDigit(strSite, strSite.Length - 1) == false)
                    strSite = strSite.Substring(0, strSite.Length - 1);
                //if(strSite.Contains("toquocmagazine.blogspot.com/feeds/posts/default"))
                //{
                //    int kt=1;
                //}
                if (strSite.Length <= 10) continue;
                if (strSite.Contains(".") == false) continue;
                if (strSite.Contains("http://") == true || strSite.Contains("https://") == true)
                {
                    if (lstURL.Contains(strSite) == false)
                        lstURL.Add(strSite);
                }
            }
        }
        #endregion
        #region {Get NetID}
        static public string ConvertIPAddressToBinString(string strIP)
        {
            string[] arr = strIP.Split('.');
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < arr.Length; i++)
                strb.Append(Convert.ToString(int.Parse(arr[i]),2).PadLeft(8,'0') );
            return strb.ToString();
        }
        #endregion
        #region {CheckIPVN}
        static public string CheckIPVN(string strIP)
        {
            string strContent = GetWebPageContent("http://whatismyipaddress.com/ip/" + strIP);
            if (strContent.Contains("Country:</th><td>"))
            {
                if (strContent.Contains("Country:</th><td>Vietnam"))
                    return "VN";
                else return "AB";
            }
            else return "NO";
        }
        #endregion
        #region {GetIPAddressFromDomainName}
        static public string GetIPAddressBasedOnDNSClass(string strDomainName)
        {
            string strIP = "";
            try
            {
                IPAddress ip = new IPAddress(0);
                IPAddress.TryParse(strDomainName, out ip);
                if (ip != null) strIP = strDomainName;
            }
            catch { strIP = ""; }
            if (strIP == "")
            {
                try
                {
                    IPAddress[] ips = Dns.GetHostAddresses(strDomainName);
                    strIP = ips[0].ToString();
                }
                catch { strIP = ""; }
            }
            return strIP;
        }
        static public string GetIPAddressFromDomainName(string strDomainName)
        {
            string strIP = "";
            try
            {
                IPAddress ip = new IPAddress(0);
                IPAddress.TryParse(strDomainName, out ip);
                if (ip != null) strIP = strDomainName;
            }
            catch { strIP = ""; }
            if (strIP == "")
            {
              //  strDomainName = strDomainName.Replace("https://", "").Replace("http://", "");
                try
                {
                    IPAddress[] ips = Dns.GetHostAddresses(strDomainName);
                    strIP =  ips[0].ToString();
                }
                catch { strIP=""; }
            }
            if (strIP == "")
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(strDomainName);

                if (reply.Status == IPStatus.Success)
                    strIP= reply.Address.ToString();
                else strIP= "";
            }
            catch { strIP = ""; }
            return strIP;
        }
        static public string GetIPAddressBasedOnPing(string strDomainName)
        {
            string strIP = "";
            try
            {
                IPAddress ip = new IPAddress(0);
                IPAddress.TryParse(strDomainName, out ip);
                if (ip != null) strIP = strDomainName;
            }
            catch { strIP = ""; }
            if(strIP=="")
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(strDomainName);

                if (reply.Status == IPStatus.Success)
                    strIP = reply.Address.ToString();
                else strIP = "";
            }
            catch { strIP = ""; }
            return strIP;
        }
        #endregion

        #region {GetSiteFromURL}
        static public string GetSiteFromURL(string strURL)
        {
            try
            {
                int index = strURL.IndexOf("/", 8);
                if (index > 0)
                    return strURL.Substring(0, index);
                else return strURL;
            }
            catch { }
            return strURL;
        }
        #endregion
   
        #region {GetDomainName}
    static public string GetDomainName(string url)
        {
          /*  if (url.Length < 8) return "";
            url = url.Replace("<", " ");
            int[] arrLen = new int[5];
            arrLen[0] = url.IndexOf("/", 8);
            arrLen[1] = url.IndexOf("?", 8);
            arrLen[2] = url.IndexOf(":", 8);
            arrLen[3] = url.IndexOf("\\", 8);
            arrLen[4] = url.IndexOf(" ", 8);
            Array.Sort(arrLen);
            for(int i=0; i<arrLen.Length;i++)
                if(arrLen[i]>0) return url.Substring(0, arrLen[i]);
            return url.Replace("<","");*/
            try
            {
                int iStart = 0;
                if (url.Contains("http://")) iStart = 7;
                if (url.Contains("https://")) iStart = 8;
                if (iStart == 0) return "";
                while (url[url.Length - 1] == '.')
                    url = url.Substring(0, url.Length - 1);
                for (int i = iStart; i < url.Length; i++)
                    if (char.IsLetterOrDigit(url[i]) || url[i] == '.' || url[i]=='-')
                        continue;
                    else return url.Substring(0, i);
                return url;
            }
            catch { }
            return "";
        }
    #endregion
        #region {GetSiteFromGoogleSearch}
    static public void GetSiteFromGoogleSearch(ref List<string> lstSite, string strContent)
        {
            lstSite.Clear();
            string[] lines = strContent.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
                string url = "";
                if (lines[i] == "Translate this page")
                    url = lines[i - 1];
                else if (lines[i].Contains("Translate this page"))
                    url = lines[i].Replace("Translate this page", "");
                if (url != "")
                {
                    int iStart = 0;
                    if (url.Contains("http://")) iStart = 7;
                    if (url.Contains("https://")) iStart = 8;
                    if (iStart == 0)
                    {
                        if (url.Contains("blogspot.com") || url.Contains("wordpress.com"))
                        {
                            url = "https://" + url;
                            iStart = 8;
                        }
                        else
                        {
                            url = "http://" + url;
                            iStart = 7;
                        }
                    }
                    while (url[url.Length - 1] == '.')
                        url = url.Substring(0, url.Length - 1);
                    int index1 = url.IndexOf("/", iStart);
                    int index2 = url.IndexOf(" ", iStart);
                    if (index1 > 0 && index2 < 0) url = url.Substring(0, index1);
                    else
                        if (index2 > 0 && index1 < 0) url = url.Substring(0, index2);
                    else
                        if (index1 >= index2) url = url.Substring(0, index2);
                    else
                        if (index2 >= index1) url = url.Substring(0, index1);
                    if(lstSite.Contains(url)==false)
                        lstSite.Add(url);
                }
            }
        }
        #endregion
        #region {ConvertUnicode}
        static public void ConvertUnicode(ref string strContent)
        {
            strContent = HttpUtility.HtmlDecode(strContent);
        }
        #endregion
        #region {ConvertUnicode1}
        static public void ConvertUnicode1(ref string strContent)
        {
            List<string> lstSpecialCharacter = new List<string>();
            List<int> lstDecimalSpecialCharacter = new List<int>();
            #region {ky tu dac biet}

            lstDecimalSpecialCharacter.Add(32);
            lstSpecialCharacter.Add("&nbsp;");
            lstDecimalSpecialCharacter.Add(34);
            lstSpecialCharacter.Add("&quot;");
            lstDecimalSpecialCharacter.Add(38);
            lstSpecialCharacter.Add("&amp;");
            lstDecimalSpecialCharacter.Add(60);
            lstSpecialCharacter.Add("&lt;");
            lstDecimalSpecialCharacter.Add(62);
            lstSpecialCharacter.Add("&gt;");           
            lstDecimalSpecialCharacter.Add(161);
            lstSpecialCharacter.Add("&iexcl");
            lstDecimalSpecialCharacter.Add(162);
            lstSpecialCharacter.Add("&cent;");
            lstDecimalSpecialCharacter.Add(163);
            lstSpecialCharacter.Add("&pound;");
            lstDecimalSpecialCharacter.Add(164);
            lstSpecialCharacter.Add("&curren;");
            lstDecimalSpecialCharacter.Add(165);
            lstSpecialCharacter.Add("&yen;");
            lstDecimalSpecialCharacter.Add(166);
            lstSpecialCharacter.Add("&brvbar;");
            lstDecimalSpecialCharacter.Add(167);
            lstSpecialCharacter.Add("&sect;");
            lstDecimalSpecialCharacter.Add(168);
            lstSpecialCharacter.Add("&uml;");
            lstDecimalSpecialCharacter.Add(169);
            lstSpecialCharacter.Add("&copy;");
            lstDecimalSpecialCharacter.Add(170);
            lstSpecialCharacter.Add("&ordf;");
            lstDecimalSpecialCharacter.Add(171);
            lstSpecialCharacter.Add("&laquo;");
            lstDecimalSpecialCharacter.Add(172);
            lstSpecialCharacter.Add("&not;");
            lstDecimalSpecialCharacter.Add(173);
            lstSpecialCharacter.Add("&shy;");
            lstDecimalSpecialCharacter.Add(174);
            lstSpecialCharacter.Add("&reg;");
            lstDecimalSpecialCharacter.Add(175);
            lstSpecialCharacter.Add("&macr;");
            lstDecimalSpecialCharacter.Add(176);
            lstSpecialCharacter.Add("&deg;");
            lstDecimalSpecialCharacter.Add(177);
            lstSpecialCharacter.Add("&plusmn;");
            lstDecimalSpecialCharacter.Add(178);
            lstSpecialCharacter.Add("&sup2;");
            lstDecimalSpecialCharacter.Add(179);
            lstSpecialCharacter.Add("&sup3;");
            lstDecimalSpecialCharacter.Add(180);
            lstSpecialCharacter.Add("&acute;");
            lstDecimalSpecialCharacter.Add(181);
            lstSpecialCharacter.Add("&micro;");
            lstDecimalSpecialCharacter.Add(182);
            lstSpecialCharacter.Add("&para;");
            lstDecimalSpecialCharacter.Add(183);
            lstSpecialCharacter.Add("&middot;");
            lstDecimalSpecialCharacter.Add(184);
            lstSpecialCharacter.Add("&cedil;");
            lstDecimalSpecialCharacter.Add(185);
            lstSpecialCharacter.Add("&sup1;");
            lstDecimalSpecialCharacter.Add(186);
            lstSpecialCharacter.Add("&ordm;");
            lstDecimalSpecialCharacter.Add(187);
            lstSpecialCharacter.Add("&raquo;");
            lstDecimalSpecialCharacter.Add(188);
            lstSpecialCharacter.Add("&frac14;");
            lstDecimalSpecialCharacter.Add(189);
            lstSpecialCharacter.Add("&frac12;");
            lstDecimalSpecialCharacter.Add(190);
            lstSpecialCharacter.Add("&frac34;");
            lstDecimalSpecialCharacter.Add(191);
            lstSpecialCharacter.Add("&iquest;");
            lstDecimalSpecialCharacter.Add(192);
            lstSpecialCharacter.Add("&Agrave;");
            lstDecimalSpecialCharacter.Add(193);
            lstSpecialCharacter.Add("&Aacute;");
            lstDecimalSpecialCharacter.Add(194);
            lstSpecialCharacter.Add("&Acirc;");
            lstDecimalSpecialCharacter.Add(195);
            lstSpecialCharacter.Add("&Atilde;");
            lstDecimalSpecialCharacter.Add(196);
            lstSpecialCharacter.Add("&Auml;");
            lstDecimalSpecialCharacter.Add(197);
            lstSpecialCharacter.Add("&Aring;");
            lstDecimalSpecialCharacter.Add(198);
            lstSpecialCharacter.Add("&AElig;");
            lstDecimalSpecialCharacter.Add(199);
            lstSpecialCharacter.Add("&Ccedil;");
            lstDecimalSpecialCharacter.Add(200);
            lstSpecialCharacter.Add("&Egrave;");
            lstDecimalSpecialCharacter.Add(201);
            lstSpecialCharacter.Add("&Eacute;");
            lstDecimalSpecialCharacter.Add(202);
            lstSpecialCharacter.Add("&Ecirc;");
            lstDecimalSpecialCharacter.Add(203);
            lstSpecialCharacter.Add("&Euml;");
            lstDecimalSpecialCharacter.Add(204);
            lstSpecialCharacter.Add("&Igrave;");
            lstDecimalSpecialCharacter.Add(205);
            lstSpecialCharacter.Add("&Iacute;");
            lstDecimalSpecialCharacter.Add(206);
            lstSpecialCharacter.Add("&Icirc;");
            lstDecimalSpecialCharacter.Add(207);
            lstSpecialCharacter.Add("&Iuml;");
            lstDecimalSpecialCharacter.Add(208);
            lstSpecialCharacter.Add("&ETH;");
            lstDecimalSpecialCharacter.Add(209);
            lstSpecialCharacter.Add("&Ntilde;");
            lstDecimalSpecialCharacter.Add(210);
            lstSpecialCharacter.Add("&Ograve;");
            lstDecimalSpecialCharacter.Add(211);
            lstSpecialCharacter.Add("&Oacute;");
            lstDecimalSpecialCharacter.Add(212);
            lstSpecialCharacter.Add("&Ocirc;");
            lstDecimalSpecialCharacter.Add(213);
            lstSpecialCharacter.Add("&Otilde;");
            lstDecimalSpecialCharacter.Add(214);
            lstSpecialCharacter.Add("&Ouml;");
            lstDecimalSpecialCharacter.Add(215);
            lstSpecialCharacter.Add("&times;");
            lstDecimalSpecialCharacter.Add(216);
            lstSpecialCharacter.Add("&Oslash;");
            lstDecimalSpecialCharacter.Add(217);
            lstSpecialCharacter.Add("&Ugrave;");
            lstDecimalSpecialCharacter.Add(218);
            lstSpecialCharacter.Add("&Uacute;");
            lstDecimalSpecialCharacter.Add(219);
            lstSpecialCharacter.Add("&Ucirc;");
            lstDecimalSpecialCharacter.Add(220);
            lstSpecialCharacter.Add("&Uuml;");
            lstDecimalSpecialCharacter.Add(221);
            lstSpecialCharacter.Add("&Yacute;");
            lstDecimalSpecialCharacter.Add(222);
            lstSpecialCharacter.Add("&THORN;");
            lstDecimalSpecialCharacter.Add(223);
            lstSpecialCharacter.Add("&szlig;");
            lstDecimalSpecialCharacter.Add(224);
            lstSpecialCharacter.Add("&agrave;");
            lstDecimalSpecialCharacter.Add(225);
            lstSpecialCharacter.Add("&aacute;");
            lstDecimalSpecialCharacter.Add(226);
            lstSpecialCharacter.Add("&acirc;");
            lstDecimalSpecialCharacter.Add(227);
            lstSpecialCharacter.Add("&atilde;");
            lstDecimalSpecialCharacter.Add(228);
            lstSpecialCharacter.Add("&auml;");
            lstDecimalSpecialCharacter.Add(229);
            lstSpecialCharacter.Add("&aring;");
            lstDecimalSpecialCharacter.Add(230);
            lstSpecialCharacter.Add("&aelig;");
            lstDecimalSpecialCharacter.Add(231);
            lstSpecialCharacter.Add("&ccedil;");
            lstDecimalSpecialCharacter.Add(232);
            lstSpecialCharacter.Add("&egrave;");
            lstDecimalSpecialCharacter.Add(233);
            lstSpecialCharacter.Add("&eacute;");
            lstDecimalSpecialCharacter.Add(234);
            lstSpecialCharacter.Add("&ecirc;");
            lstDecimalSpecialCharacter.Add(235);
            lstSpecialCharacter.Add("&euml;");
            lstDecimalSpecialCharacter.Add(236);
            lstSpecialCharacter.Add("&igrave;");
            lstDecimalSpecialCharacter.Add(237);
            lstSpecialCharacter.Add("&iacute;");
            lstDecimalSpecialCharacter.Add(238);
            lstSpecialCharacter.Add("&icirc;");
            lstDecimalSpecialCharacter.Add(239);
            lstSpecialCharacter.Add("&iuml;");
            lstDecimalSpecialCharacter.Add(240);
            lstSpecialCharacter.Add("&eth;");
            lstDecimalSpecialCharacter.Add(241);
            lstSpecialCharacter.Add("&ntilde;");
            lstDecimalSpecialCharacter.Add(242);
            lstSpecialCharacter.Add("&ograve;");
            lstDecimalSpecialCharacter.Add(243);
            lstSpecialCharacter.Add("&oacute;");
            lstDecimalSpecialCharacter.Add(244);
            lstSpecialCharacter.Add("&ocirc;");
            lstDecimalSpecialCharacter.Add(245);
            lstSpecialCharacter.Add("&otilde;");
            lstDecimalSpecialCharacter.Add(246);
            lstSpecialCharacter.Add("&ouml;");
            lstDecimalSpecialCharacter.Add(247);
            lstSpecialCharacter.Add("&divide;");
            lstDecimalSpecialCharacter.Add(248);
            lstSpecialCharacter.Add("&oslash;");
            lstDecimalSpecialCharacter.Add(249);
            lstSpecialCharacter.Add("&ugrave;");
            lstDecimalSpecialCharacter.Add(250);
            lstSpecialCharacter.Add("&uacute;");
            lstDecimalSpecialCharacter.Add(251);
            lstSpecialCharacter.Add("&ucirc;");
            lstDecimalSpecialCharacter.Add(252);
            lstSpecialCharacter.Add("&uuml;");
            lstDecimalSpecialCharacter.Add(253);
            lstSpecialCharacter.Add("&yacute;");
            lstDecimalSpecialCharacter.Add(254);
            lstSpecialCharacter.Add("&thorn;");
            lstDecimalSpecialCharacter.Add(255);
            lstSpecialCharacter.Add("&yuml;");
            lstDecimalSpecialCharacter.Add(338);
            lstSpecialCharacter.Add("&OElig;");
            lstDecimalSpecialCharacter.Add(339);
            lstSpecialCharacter.Add("&oelig;");
            lstDecimalSpecialCharacter.Add(352);
            lstSpecialCharacter.Add("&Scaron;");
            lstDecimalSpecialCharacter.Add(353);
            lstSpecialCharacter.Add("&scaron;");
            lstDecimalSpecialCharacter.Add(376);
            lstSpecialCharacter.Add("&Yuml;");
            lstDecimalSpecialCharacter.Add(710);
            lstSpecialCharacter.Add("&circ;");
            lstDecimalSpecialCharacter.Add(732);
            lstSpecialCharacter.Add("&tilde;");
            lstDecimalSpecialCharacter.Add(8194);
            lstSpecialCharacter.Add("&ensp;");
            lstDecimalSpecialCharacter.Add(8195);
            lstSpecialCharacter.Add("&emsp;");
            lstDecimalSpecialCharacter.Add(8201);
            lstSpecialCharacter.Add("&thinsp;");
            lstDecimalSpecialCharacter.Add(8204);
            lstSpecialCharacter.Add("&zwnj;");
            lstDecimalSpecialCharacter.Add(8205);
            lstSpecialCharacter.Add("&zwj;");
            lstDecimalSpecialCharacter.Add(8206);
            lstSpecialCharacter.Add("&lrm;");
            lstDecimalSpecialCharacter.Add(8207);
            lstSpecialCharacter.Add("&rlm;");
            lstDecimalSpecialCharacter.Add(8211);
            lstSpecialCharacter.Add("&ndash;");
            lstDecimalSpecialCharacter.Add(8212);
            lstSpecialCharacter.Add("&mdash;");
            lstDecimalSpecialCharacter.Add(8216);
            lstSpecialCharacter.Add("&lsquo;");
            lstDecimalSpecialCharacter.Add(8217);
            lstSpecialCharacter.Add("&rsquo;");
            lstDecimalSpecialCharacter.Add(8218);
            lstSpecialCharacter.Add("&sbquo;");
            lstDecimalSpecialCharacter.Add(8220);
            lstSpecialCharacter.Add("&ldquo;");
            lstDecimalSpecialCharacter.Add(8221);
            lstSpecialCharacter.Add("&rdquo;");
            lstDecimalSpecialCharacter.Add(8222);
            lstSpecialCharacter.Add("&bdquo;");
            lstDecimalSpecialCharacter.Add(8224);
            lstSpecialCharacter.Add("&dagger;");
            lstDecimalSpecialCharacter.Add(8225);
            lstSpecialCharacter.Add("&Dagger;");
            lstDecimalSpecialCharacter.Add(8240);
            lstSpecialCharacter.Add("&permil;");
            lstDecimalSpecialCharacter.Add(8249);
            lstSpecialCharacter.Add("&lsaquo;");
            lstDecimalSpecialCharacter.Add(8250);
            lstSpecialCharacter.Add("&rsaquo;");
            lstDecimalSpecialCharacter.Add(8364);
            lstSpecialCharacter.Add("&euro;");
            #endregion

            string pattern1 = @"&#(\d*?);";
            MatchCollection mc;
            Regex myRegex = new Regex(pattern1);
            mc = myRegex.Matches(strContent);
            foreach (Match m in mc)
            {
                try
                {
                    string temp = m.Value.Trim();
                    int x = int.Parse(temp.Replace("&#", "").Replace(";", ""));
                    char c = Convert.ToChar(x);
                    strContent = strContent.Replace(temp, c.ToString());
                }
                catch
                { }
            }
            for (int i = 0; i < lstSpecialCharacter.Count; i++)
                strContent = strContent.Replace(lstSpecialCharacter[i], Convert.ToChar(lstDecimalSpecialCharacter[i]).ToString());
        }
        #endregion
        #region {ScanListPort}
        static public string ScanListPort(string strIPAddress, List<string> lstPortName, List<int> lstPortNumber)
        {
            bool[] arrResult = new bool[lstPortNumber.Count];
            Parallel.For(0, lstPortNumber.Count, index =>
            {
                arrResult[index] = csSupport.ScanPort(strIPAddress, lstPortNumber[index]);

            } //close lambda expression
            ); //cl
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < arrResult.Length; i++)
                if (arrResult[i] == true)
                    strb.Append(lstPortName[i] + " (" + lstPortNumber[i].ToString() + "): open\r\n");
            return strb.ToString();
        }
        #endregion
        #region {GetTechnicalInformation}
        static public string GetTechnicalInformation(string strSiteAddress)
        {
            return "";
           /* List<string> lstLines = new List<string>();
            int index1 = 0, index2 = 0;
            string strCharset = "utf-8", strProxy = "true", strIpAddress = "";
            StringBuilder strbResult = new StringBuilder();
            string strContent = GetWebPageContent("http://toolbar.netcraft.com/site_report?url=" + strSiteAddress, ref strCharset, ref strProxy);
            if (strContent == "") return "";

            try
            {
                index1 = strContent.IndexOf("<th width=\"13%\">Site title</th>");
                index2 = strContent.IndexOf("<th width=\"13%\">DNS Security Extensions</th>");
                strContent = strContent.Substring(index1, index2 - index1);
                string[] lines = strContent.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    if ((lines[i].Contains("<th width=\"") == true && lines[i].Contains("</th>") == true) || (lines[i].Contains("<td width=\"") == true && lines[i].Contains("</td>") == true))
                        lstLines.Add(lines[i].Replace("<th width=\"13%\">", "").Replace("</th>", "").Replace("<td width=\"37%\">", "").Replace("</td>", "").Trim());
                }
            }

            catch { return ""; }

            for (int i = 0; i < lstLines.Count; i += 2)
            {
                try
                {
                    if (lstLines[i + 1].Contains("unavailable") == false)
                    {
                        try
                        {
                            if (lstLines[i].Contains("IP address"))
                                strIpAddress = lstLines[i + 1];
                            if (lstLines[i+1][0] == '<')
                            {
                                index1 = lstLines[i + 1].IndexOf(">");
                                strbResult.Append(lstLines[i] + ": " + lstLines[i + 1].Substring(index1 + 1).Replace("</a>", "") + "\r\n");
                            }
                            else
                                strbResult.Append(lstLines[i] + ": " + lstLines[i + 1] + "\r\n");
                        }
                        catch { }
                    }
                }
                catch { }
            }
            if (strIpAddress == "") return "";
            List<string> lstPortName = new List<string>();
            List<int> lstPortNumber = new List<int>();
            AddPort(ref lstPortName, ref lstPortNumber);
            strbResult.Append("Open ports:\r\n" + ScanListPort(strIpAddress, lstPortName, lstPortNumber) + "\r\n\r\n");
            strbResult.Append("Mail exchange:\r\n" + csSupport.DNSLookupNsType(strSiteAddress.Replace("https://","").Replace("http://",""), NsType.MX));
            return strbResult.ToString();*/
        }
        #endregion
        #region {Add port}
        static public void AddPort(ref List<string> lstPortName, ref List<int> lstPortNumber)
        {
            lstPortNumber.Add(21); lstPortName.Add("FTP");
            lstPortNumber.Add(22); lstPortName.Add("SFTP");
            lstPortNumber.Add(23); lstPortName.Add("TELNET");
            lstPortNumber.Add(25); lstPortName.Add("SMTP");
            lstPortNumber.Add(26); lstPortName.Add("SMTP Alternate");
            lstPortNumber.Add(53); lstPortName.Add("DNS");
            lstPortNumber.Add(80); lstPortName.Add("HTTP");
            lstPortNumber.Add(110); lstPortName.Add("POP3");
            lstPortNumber.Add(115); lstPortName.Add("SFTP");
            lstPortNumber.Add(135); lstPortName.Add("RPC");
            lstPortNumber.Add(139); lstPortName.Add("NetBIOS");
            lstPortNumber.Add(143); lstPortName.Add("IMAP");
            lstPortNumber.Add(194); lstPortName.Add("IRC");
            lstPortNumber.Add(443); lstPortName.Add("SSL");
            lstPortNumber.Add(445); lstPortName.Add("SMN");
            lstPortNumber.Add(587); lstPortName.Add("SMTP Alternate");
            lstPortNumber.Add(990); lstPortName.Add("FTP SSL");
            lstPortNumber.Add(995); lstPortName.Add("IMAP SSL");
            lstPortNumber.Add(1433); lstPortName.Add("MSSQL");
            lstPortNumber.Add(2077); lstPortName.Add("Webdisk");
            lstPortNumber.Add(2078); lstPortName.Add("Webdisk 2078");
            lstPortNumber.Add(2082); lstPortName.Add("cPanel");
            lstPortNumber.Add(2083); lstPortName.Add("cPanel SSL");
            lstPortNumber.Add(2086); lstPortName.Add("WHM");
            lstPortNumber.Add(2087); lstPortName.Add("WHM");
            lstPortNumber.Add(2089); lstPortName.Add("cPanel");
            lstPortNumber.Add(2095); lstPortName.Add("Webmail");
            lstPortNumber.Add(2096); lstPortName.Add("Webmail SSL");
            lstPortNumber.Add(2222); lstPortName.Add("SFTP Shared/Reseller Servers");
            lstPortNumber.Add(3306); lstPortName.Add("MySQL");
            lstPortNumber.Add(3389); lstPortName.Add("Remote Desktop");
            lstPortNumber.Add(5632); lstPortName.Add("PCAnywhere");
            lstPortNumber.Add(5900); lstPortName.Add("VNC");
            lstPortNumber.Add(6112); lstPortName.Add("Warcraft III");
        }
        #endregion
        #region {GetResultZoneFromGoogleSearch}
        static public string GetResultZoneFromGoogleSearch(string strContent)
        {

            //File.WriteAllText("F:\\google1.txt", strContent);
            //Chilkat.HtmlToText h2t = new Chilkat.HtmlToText();
            //bool success = h2t.UnlockComponent("30-day trial");
            //strContent = h2t.ToText(strContent);
            int index1 = strContent.IndexOf("Các công cụ tìm kiếm");
            int index2 = 0;
            if (index1 >= 0)
            {
                index2 = strContent.IndexOf("();</script>", index1);
                if (index2 >= 0)
                    strContent = strContent.Substring(index1 + "Các công cụ tìm kiếm".Length, index2 - index1 - "Các công cụ tìm kiếm".Length).Trim();
                else
                    strContent = strContent.Substring(index1 + "Các công cụ tìm kiếm".Length);
            }
           
          //  File.WriteAllText("F:\\google.txt", strContent);
            return strContent;
        }
        #endregion
      
        #region {GetNewsContentSummary}
        static public string GetNewsContentSummary1(string strURL, string strDB, string strCollection, List<string> lstImportantKeyword)
        {
            string strNewsContent = csGetMongoData.GetNewsContent(strURL,strDB, strCollection);
            int index = strNewsContent.IndexOf("http");
            if (index >= 0) index = strNewsContent.IndexOf("\r\n", index + 1);
            if (index>0) strNewsContent = strNewsContent.Substring(index).Trim();
            strNewsContent = strNewsContent.ToString().Replace("\r\n", "...").Replace("\t", "...").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Replace("  ", " ");
            int iMinIndex = -1;
            for (int i = 0; i < lstImportantKeyword.Count; i++)
            {
                index = strNewsContent.IndexOf(lstImportantKeyword[i], StringComparison.CurrentCultureIgnoreCase);
                if (index > 0 && iMinIndex < 0)
                {
                    iMinIndex = index;
                    continue;
                }
                if (index < 0) continue;
                if (iMinIndex > index)
                    iMinIndex = index;
            }
            string strSummary = "";
            if (iMinIndex >= 0)
            {
                if (strNewsContent.Length >=270 + iMinIndex)
                    strSummary = strNewsContent.Substring(iMinIndex, 270).Trim();
                else strSummary = strNewsContent.Substring(iMinIndex).Trim();
                return strSummary;
            }
            else
            {
                if (strNewsContent.Length > 290)
                    strSummary = strNewsContent.Substring(0, 270).Trim();
                else
                    strSummary = strNewsContent;
            }
            return strSummary;
        }
        static public string GetNewsContentSummary(string strURL, string strDB, string strCollection)
        {
            string strNewsContent = csGetMongoData.GetNewsContent(strURL, strDB, strCollection);
            int index = 0;
            if (strURL.Contains("minds.com") == false)
            {
                index = strNewsContent.IndexOf("http");
                if (index >= 0) index = strNewsContent.IndexOf("\r\n", index + 1);
                if (index > 0) strNewsContent = strNewsContent.Substring(index).Trim();
            }
            if (strURL.Contains("minds.com") == true)
            {
                index = strNewsContent.IndexOf("Đăng lúc:");
                if (index < 0)
                {
                    index = strNewsContent.IndexOf("Kênh đăng:");
                }
                if (index <= 0) index = 0;
                index = strNewsContent.IndexOf("\n\n", index);
                if (index > 0) strNewsContent = strNewsContent.Substring(index+2).Trim();
            }
            strNewsContent = strNewsContent.ToString().Replace("\r\n", "...").Replace("\n", "...").Replace("\t", "...").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Replace("  ", " ");
            if (strNewsContent.Length < 270)
                return strNewsContent;
            else return strNewsContent.Substring(0, 270);
        }
        #endregion
       
        #region  {GetPlainTextFromHtml_New}
        static string GetPlainTextFromHtml_New(string htmlString)
        {
            //htmlString = htmlString.Replace("\r\n", "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            StringBuilder strbResult = new StringBuilder(), strbTemp = new StringBuilder(); ;

            bool bBegin = false, bEnd = false;
            char c = ' ';
            for (int i = 0; i < htmlString.Length; i++)
            {
                c = htmlString[i];
                if (c == '<')
                {
                    bBegin = true;
                    bEnd = false;
                    continue;
                }
                if (c == '>')
                {
                    bBegin = false;
                    bEnd = true;
                 //   strbTemp.Append("\r\n");
                    if (WordCount(strbTemp.ToString()) >= 5)
                        strbResult.Append("\t"+strbTemp.ToString().Trim()+"\r\n");
                    strbTemp.Clear();
                    continue;
                }
                if (bBegin == false)
                    strbTemp.Append(c);
            }
            //if (strbTemp.Length > 0)
            //{
                if (WordCount(strbTemp.ToString()) >= 5)
                    strbResult.Append(strbTemp);
            //}
            return strbResult.ToString();//.Replace("\t\t","\t").Replace("\t\t","\t");
        }
        #endregion
        #region {DNSLookupIP}
        static public string DNSLookupIP(string host)
        {
            return "";
        }
        #endregion
        #region {GetHostEntry}
        public static string GetHostEntry(string ipaddress)
        {
            IPAddress addr = IPAddress.Parse(ipaddress);
            IPHostEntry entry = Dns.GetHostEntry(addr);
            return entry.HostName;
        }
        #endregion
        #region {Scanport}
        public static bool ScanPort(string ipaddress, int port)
        {
            try
            {
                TcpClient TcpScan = new TcpClient();
                TcpScan.ReceiveTimeout = 3000;
                TcpScan.Connect(ipaddress, port);
                return true;
            }
            catch { return false; }
        }
        #endregion
        #region {DNSLookupNsType}
        
        #endregion
        #region {GetNewsContent}
        static public string GetNewsContent(string strTitle, string strContent)
        {
            ConvertUnicode(ref strContent);
            int index_begin_script = 0, index_end_script = 0;

            int indexBody = strContent.IndexOf("<body",StringComparison.CurrentCultureIgnoreCase);
            if (indexBody > 0) strContent = strContent.Substring(indexBody);
            while (1 == 1)
            {
                index_begin_script = strContent.IndexOf("<!--", StringComparison.CurrentCultureIgnoreCase);
                if (index_begin_script < 0) break;
                index_end_script = strContent.IndexOf("-->", index_begin_script + "<!--".Length, StringComparison.CurrentCultureIgnoreCase);
                if (index_end_script < 0) break;
                strContent = strContent.Remove(index_begin_script, index_end_script - index_begin_script + "-->".Length);
            }
         // File.WriteAllText("F:\\1.txt", strContent);
            while (1 == 1)
            {
                index_begin_script = strContent.IndexOf("<script>", StringComparison.CurrentCultureIgnoreCase);
                if (index_begin_script < 0)
                    index_begin_script = strContent.IndexOf("<script ", StringComparison.CurrentCultureIgnoreCase);
                if (index_begin_script < 0) break;
                index_end_script = strContent.IndexOf("</script>", index_begin_script + "<script".Length + 1, StringComparison.CurrentCultureIgnoreCase);
                if (index_end_script < 0) break;
                strContent = strContent.Remove(index_begin_script, index_end_script - index_begin_script + "</script>".Length);
            }
        // File.WriteAllText("F:\\2.txt", strContent);
            while (1 == 1)
            {
                index_begin_script = strContent.IndexOf("<style>", StringComparison.CurrentCultureIgnoreCase);
                if (index_begin_script < 0)
                    index_begin_script = strContent.IndexOf("<style ", StringComparison.CurrentCultureIgnoreCase);
                if (index_begin_script < 0) break;
                index_end_script = strContent.IndexOf("</style>", index_begin_script + "<style".Length + 1, StringComparison.CurrentCultureIgnoreCase);
                if (index_end_script < 0) break;
                strContent = strContent.Remove(index_begin_script, index_end_script - index_begin_script + "</style>".Length);
            }
         // File.WriteAllText("F:\\3.txt", strContent);
          while (1 == 1)
          {
              index_begin_script = strContent.IndexOf("<a>", StringComparison.CurrentCultureIgnoreCase);
              if (index_begin_script < 0)
                  index_begin_script = strContent.IndexOf("<a ", StringComparison.CurrentCultureIgnoreCase);
              if (index_begin_script < 0) break;
              index_end_script = strContent.IndexOf("</a>", index_begin_script + "<a".Length + 1, StringComparison.CurrentCultureIgnoreCase);
              if (index_end_script < 0) break;
              strContent = strContent.Remove(index_begin_script, index_end_script - index_begin_script + "</a>".Length);
          }
          //File.WriteAllText("F:\\4.txt", strContent);
           //int count = 0;
          //  strContent = strContent.Replace("</a>", "</a>\r\n").Replace("</p>","</p>\r\n").Replace("<br />","\r\n").Replace("<br>","\r\n");
          strContent = strContent.Replace("</p>", "</p>\r\n").Replace("<br />", "\r\n").Replace("<br>", "\r\n");
           char[] charspilit = new char[2] { '\r', '\n' };
           string[] lines = strContent.Split(charspilit, StringSplitOptions.RemoveEmptyEntries);
           List<string> lstTemp = new List<string>();
           StringBuilder strb = new StringBuilder();
           for (int i = 0; i < lines.Length; i++)
           {
               //count++;
               //if (count % 300 == 0)
               //    MessageBox.Show(count.ToString());
               lines[i] = lines[i].Replace("\t", "").Trim();
               //try
               //{
               //    if ((lines[i].Substring(0, 2) == "<a") && (lines[i].Substring(lines[i].Length - 4, 4) == "</a>"))
               //        continue;
               //}
               //catch
               //{
               //   // continue;
               //}
               if (lines[i].IndexOf("href=", StringComparison.CurrentCultureIgnoreCase) >= 0 || lines[i].IndexOf("src=", StringComparison.CurrentCultureIgnoreCase) >= 0 || lines[i].IndexOf("class=", StringComparison.CurrentCultureIgnoreCase) >= 0) continue;
               strb.Append(lines[i] + "\r\n");
           }
           strContent = strb.ToString();
           while (1 == 1)
           {
               index_begin_script = strContent.IndexOf("<a>", StringComparison.CurrentCultureIgnoreCase);
               if (index_begin_script < 0)
                   index_begin_script = strContent.IndexOf("<a ", StringComparison.CurrentCultureIgnoreCase);
               if (index_begin_script < 0) break;
               index_end_script = strContent.IndexOf("</a>", index_begin_script + "<a".Length + 1, StringComparison.CurrentCultureIgnoreCase);
               if (index_end_script < 0) break;
               strContent = strContent.Remove(index_begin_script, index_end_script - index_begin_script + "</a>".Length);
           }
          // File.WriteAllText("f:\\5.txt", strContent);
           
            strContent = GetPlainTextFromHtml_New(strb.ToString());
            int indexTitle = strContent.LastIndexOf(strTitle,StringComparison.CurrentCultureIgnoreCase);
            if (indexTitle > 0)
                strContent = "\t" + strContent.Substring(indexTitle);
         //  File.WriteAllText("f:\\6.txt", strContent);
            return strContent;
        }
        #endregion
        #region {GetYoutubeViews}
        static public string GetYoutubeViews(string strURL)
        {
            try
            {
                string strContent = GetWebPageContent(strURL);
                int index1 = 0, index2 = 0;
                index1 = strContent.IndexOf("<div class=\"watch-view-count\">");
                if (index1 > 0)
                {
                    index2 = strContent.IndexOf("</div>", index1);
                    if (index2 > 0)
                        return strContent.Substring(index1 + "<div class=\"watch-view-count\">".Length, index2 - index1 - "<div class=\"watch-view-count\">".Length).Replace("views","lượt xem");
                }
            }
            catch { }
            return "";
        }
        #endregion
        #region {GetYoutubeViews}
        static public void GetYoutubeViews_Name(string strURL, ref string strViews, ref string strName)
        {
            try
            {
                string strContent = GetWebPageContent(strURL);
                int index1 = 0, index2 = 0;
                index1 = strContent.IndexOf("<div class=\"watch-view-count\">");
                if (index1 > 0)
                {
                    index2 = strContent.IndexOf("</div>", index1);
                    if (index2 > 0)
                        strViews = strContent.Substring(index1 + "<div class=\"watch-view-count\">".Length, index2 - index1 - "<div class=\"watch-view-count\">".Length).Replace("views", "lượt xem");
                }
                index1 = strContent.IndexOf("\"name\": \"");
                if (index1 > 0)
                {
                    index2 = strContent.IndexOf("\"", index1 + "\"name\": \"".Length);
                    if (index2 > 0)
                        strName = strContent.Substring(index1 + "\"name\": \"".Length, index2 - index1 - "\"name\": \"".Length).Trim();
                }
            }
            catch { }
        }
        #endregion
        #region encrypt, decrypt
        static public string Encrypt01(string strText)
        {
            if (strText == "") return "";
            try
            {
                string strInput1 = "Ncày xun con én đưa toi thiều qung chn chục đã ngoi sáu mtơi nử chừng xuân thot gãy cành thin";
                while (strText.Length > strInput1.Length)
                    strInput1 += strInput1;
                byte[] arrText = GetBytes(strText);
                byte[] arrInput1 = GetBytes(strInput1);
                for (int i = 0; i < arrText.Length; i++)
                {
                    arrText[i] = (byte)((arrText[i] + arrInput1[i]) % 256);
                }
                for (int i = 0; i < arrText.Length / 2; i++)
                {
                    byte x = arrText[i];
                    arrText[i] = arrText[arrText.Length - 1 - i];
                    arrText[arrText.Length - 1 - i] = x;
                }
                for (int i = 0; i < arrText.Length; i++)
                {
                    arrText[i] = (byte)((arrText[i] + arrInput1[i]) % 256);
                }

               return Convert.ToBase64String(arrText);
                //StringBuilder strb = new StringBuilder();
                //for (int i = 0; i < arrText.Length; i++)
                //    strb.Append(arrText[i].ToString().PadLeft(3, '0'));
                //return strb.ToString();
            }
            catch { return ""; }
        }
       static public string Decrypt(string strText)
        {
            if (strText == "") return "";
            try
            {
                byte[] arrText = Convert.FromBase64String(strText);
                string strInput1 = "Ncày xun con én đưa toi thiều qung chn chục đã ngoi sáu mtơi nử chừng xuân thot gãy cành thin";
                while (arrText.Length > strInput1.Length)
                    strInput1 += strInput1;
               // byte[] arrText = new byte[strText.Length / 3];
               
                byte[] arrInput1 = GetBytes(strInput1);

                //for (int i = 0; i < arrText.Length; i++)
                //{
                //    arrText[i] = byte.Parse(strText.Substring(i * 3, 3));
                //}

                for (int i = 0; i < arrText.Length; i++)
                {
                    if (arrText[i] >= arrInput1[i])
                        arrText[i] = (byte)(arrText[i] - arrInput1[i]);
                    else
                        arrText[i] = (byte)(arrText[i] + 256 - arrInput1[i]);
                }
                for (int i = 0; i < arrText.Length / 2; i++)
                {
                    byte x = arrText[i];
                    arrText[i] = arrText[arrText.Length - 1 - i];
                    arrText[arrText.Length - 1 - i] = x;
                }
                for (int i = 0; i < arrText.Length; i++)
                {
                    if (arrText[i] >= arrInput1[i])
                        arrText[i] = (byte)(arrText[i] - arrInput1[i]);
                    else
                        arrText[i] = (byte)(arrText[i] + 256 - arrInput1[i]);

                }
                return GetString(arrText);
            }
            catch { return ""; }
        }
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
        #endregion
        #region {GetDb_Collection}
        static public void GetDb_Collection(string strLevelMonitoring, string strNewsCatalog, ref string strNewsDatabase, ref string strNewsCollection)
        {
            DateTime dtTime = DateTime.Now;
            strNewsDatabase = "_" + dtTime.Year.ToString();
            if (dtTime.Month >= 1 && dtTime.Month <= 3)
                strNewsDatabase += "_Q1";
            if (dtTime.Month >= 4 && dtTime.Month <= 6)
                strNewsDatabase += "_Q2";
            if (dtTime.Month >= 7 && dtTime.Month <= 9)
                strNewsDatabase += "_Q3";
            if (dtTime.Month >= 10 && dtTime.Month <= 12)
                strNewsDatabase += "_Q4";

            switch (strLevelMonitoring)
            {
                case "High priority":
                    strNewsCollection = "HighPriority_NewsDetail";
                    break;
                case "Medium priority":
                    strNewsCollection = "MediumPriority_NewsDetail";
                    break;
                case "Low priority":
                    strNewsCollection = "LowPriority_NewsDetail";
                    break;
                default:
                    break;
            }

            switch (strNewsCatalog)
            {
                case "VietNam":
                    strNewsDatabase = "dbVietNamNews" + strNewsDatabase;
                    break;
                case "AntiVietNam":
                    strNewsDatabase = "dbAntiVietNamNews" + strNewsDatabase;
                    break;
                case "CyberSecurity":
                    strNewsDatabase = "dbCyberSecurity" + strNewsDatabase;
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region {Delete Documents}

        #endregion
        #region {GetWebPageContent_GoogleSearch}
        static public string GetWebPageContent_GoogleSearch(string url)
        {
            string result = "";
            try
            {
                HttpWebRequest myRequest1 = (HttpWebRequest)WebRequest.Create(url);
                myRequest1.Proxy = new WebProxy("127.0.0.1", 9666);
                myRequest1.Timeout = 30000;
                WebResponse myResponse1 = myRequest1.GetResponse();
                StreamReader sr1;
                sr1 = new StreamReader(myResponse1.GetResponseStream(), Encoding.UTF8);
                result = sr1.ReadToEnd();
                sr1.Close();
                myResponse1.Close();
            }
            catch { }

            return HttpUtility.HtmlDecode(result);
        }
        #endregion
        #region {GetListYoutube}
        static public void GetListYoutube(string strContent, ref List<string> lstSite, ref List<string> lstChannelName)
        {
            lstSite.Clear();
            lstChannelName.Clear();
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            List<string> lstContent = new List<string>();
            document.LoadHtml(strContent);
            string strXpath = "//div[contains(@class,'yt-lockup-byline')]";
            try
            {
                foreach (HtmlNode element in document.DocumentNode.SelectNodes(strXpath)) //   //div[contains(@class,'Enter Value In Here')]
                {
                    try
                    {
                        string strTemp = element.InnerHtml, strLink = "", strName = "";
                        int index1 = 0, index2 = 0;
                        if (strTemp.Contains("<a href=\"/user/") == true)
                        {
                            index1 = strTemp.IndexOf("<a href=\"/user/");
                            index2 = strTemp.IndexOf("\"", index1 + "<a href=\"/user/".Length);
                            strLink = "https://www.youtube.com/user/" + strTemp.Substring(index1 + "<a href=\"/user/".Length, index2 - index1 - "<a href=\"/user/".Length);
                            index1 = strTemp.IndexOf(">", index2);
                            index2 = strTemp.IndexOf("</a>", index1);
                            strName = strTemp.Substring(index1 + 1, index2 - index1 - 1).Trim();
                            if (lstSite.Contains(strLink) == false)
                            {
                                lstSite.Add(strLink);
                                lstChannelName.Add(strName);
                            }
                        }
                        else
                            if (strTemp.Contains("<a href=\"/channel/") == true)
                        {
                            index1 = strTemp.IndexOf("<a href=\"/channel/");
                            index2 = strTemp.IndexOf("\"", index1 + "<a href=\"/channel/".Length);
                            strLink = "https://www.youtube.com/channel/" + strTemp.Substring(index1 + "<a href=\"/channel/".Length, index2 - index1 - "<a href=\"/channel/".Length);
                            index1 = strTemp.IndexOf(">", index2);
                            index2 = strTemp.IndexOf("</a>", index1);
                            strName = strTemp.Substring(index1 + 1, index2 - index1 - 1).Trim();
                            if (lstSite.Contains(strLink) == false)
                            {
                                lstSite.Add(strLink);
                                lstChannelName.Add(strName);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
        static public void GetListYoutube1(string strContent, ref List<string> lstSite)
        {
            List<string> lstChannelName = new List<string>();
            try
            {
                lstSite.Clear();

                if (strContent == "") return;
                string[] lines = strContent.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Trim();
                    if (lines[i].Length < "https://www.youtube.com/watch?v=".Length) continue;
                    if (lines[i].Substring(0, "https://www.youtube.com/watch?v=".Length) == "https://www.youtube.com/watch?v=")
                    {
                        //copy firefox
                        string strChannel = "";
                        int index = lines[i + 1].IndexOf("Uploaded by ");
                        if (index > 0) strChannel = lines[i + 1].Substring(index + "Uploaded by ".Length, lines[i + 1].Length - index - "Uploaded by ".Length).Trim();
                        if (lstChannelName.IndexOf(strChannel) < 0)
                        {
                            string strresult = GetWebPageContentProxy_Ultrasurf(lines[i].Trim());
                            if (strresult == "") continue;
                            index = strresult.IndexOf("<div class=\"yt-user-info\">");
                            if (index < 0) continue;
                            index = strresult.IndexOf("<a href=\"/", index + "<div class=\"yt-user-info\">".Length);
                            if (index < 0) continue;
                            int index1 = strresult.IndexOf("\"", index + "<a href=\"/".Length);
                            if (index1 < 0) continue;
                            lstSite.Add("https://www.youtube.com/" + strresult.Substring(index + "<a href=\"/".Length, index1 - "<a href=\"/".Length - index));
                            lstChannelName.Add(strChannel);
                        }
                        i += 1;

                        //copy internet explorer
                        /* int index = lines[i + 1].IndexOf("Uploaded by ");
                         if (index < 0) continue;
                         string strChannel = lines[i + 1].Substring(index + "Uploaded by ".Length, lines[i + 1].Length - index - "Uploaded by ".Length).Trim();
                         if (lstChannelName.IndexOf(strChannel) < 0)
                         {
                             string strresult = GetWebPageContentProxy_Ultrasurf(lines[i].Trim());
                             if (strresult == "") continue;
                             index = strresult.IndexOf("<div class=\"yt-user-info\">");
                             if (index < 0) continue;
                             index = strresult.IndexOf("<a href=\"/", index + "<div class=\"yt-user-info\">".Length);
                             if (index < 0) continue;
                             int index1 = strresult.IndexOf("\"", index + "<a href=\"/".Length);
                             if (index1 < 0) continue;
                             lstSite.Add("https://www.youtube.com/" + strresult.Substring(index + "<a href=\"/".Length, index1 - "<a href=\"/".Length - index));
                             lstChannelName.Add(strChannel);
                         }*/
                    }
                }
            }
            catch { }
        }
        #endregion
        #region {GetWebPageContent}
        static public string GetWebPageContentProxy_Ultrasurf(string url)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string result = "";
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.UserAgent = csGlobal.agent_firefox;
                myRequest.Proxy = new WebProxy("192.168.92.200", 9666);
                myRequest.Timeout = 30000;
                WebResponse myResponse = myRequest.GetResponse();
                StreamReader sr = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                result = sr.ReadToEnd();
                sr.Close();
                myResponse.Close();
            }
            catch { result = ""; }
            return result;
        }
        #endregion
        #region {CheckSiteExist}
        static public void CheckTargetExist_Youtube(ref List<string> lstSiteAddress, ref List<string> lstChannelName)
        {
            var db = csGetMongoData.GetDatabase(csGlobal.strMongoDatabase);
            var collection01 = db.GetCollection<Mongo_Target>("ImportantTarget");
            string strSiteAddress = "";
            for (int i = lstSiteAddress.Count - 1; i >= 0; i--)
            {
                strSiteAddress = lstSiteAddress[i];//.Replace("https://", "").Replace("http://", "");
                if (collection01.AsQueryable().Where(p => p.SiteAddress.Contains(strSiteAddress)).Select(p => new { p.SiteAddress }).Count() > 0)
                {
                    lstSiteAddress.RemoveAt(i);
                    lstChannelName.RemoveAt(i);
                }
            }
            if (lstSiteAddress.Count == 0) return;

            var collection02 = db.GetCollection<Mongo_NewTarget>("NewTarget");
            for (int i = lstSiteAddress.Count - 1; i >= 0; i--)
            {
                //strSiteAddress = lstSiteAddress[i].Replace("https://", "").Replace("http://", "");
                if (collection02.AsQueryable().Where(p => p.SiteAddress.Contains(strSiteAddress)).Select(p => new { p.SiteAddress }).Count() > 0)
                {
                    lstSiteAddress.RemoveAt(i);
                    lstChannelName.RemoveAt(i);
                }
            }
            if (lstSiteAddress.Count == 0) return;

            var collection03 = db.GetCollection<Mongo_NormalTarget>("NormalTarget");
            for (int i = lstSiteAddress.Count - 1; i >= 0; i--)
            {
                //  strSiteAddress = lstSiteAddress[i].Replace("https://", "").Replace("http://", "");
                if (collection03.AsQueryable().Where(p => p.SiteAddress.Contains(strSiteAddress)).Select(p => new { p.SiteAddress }).Count() > 0)
                {
                    lstSiteAddress.RemoveAt(i);
                    lstChannelName.RemoveAt(i);
                }
            }
        }
        #endregion
        #region {GetSiteTitle}
        static public string GetSiteTitle(string strPageContent)
        {
            try
            {
                int index1 = 0, index2 = 0;
                index1 = strPageContent.IndexOf("\"og:title\"");
                if (index1 < 0) index1 = strPageContent.IndexOf("'og:title'");
                if (index1 > 0)
                {
                    index2 = index1;
                    do
                    {
                        index1 -= 1;
                        if (strPageContent.Substring(index1, 1) == "<")
                            break;
                    }
                    while (index1 > 0);
                    do
                    {
                        index2 += 1;
                        string str = strPageContent.Substring(index2, 1);
                        if (strPageContent.Substring(index2, 1) == ">")
                            break;
                    }
                    while (index2 < index1 + 500);
                    strPageContent = strPageContent.Substring(index1, index2 - index1);
                    index1 = strPageContent.IndexOf("content=\"");
                    if (index1 < 0) index1 = strPageContent.IndexOf("content='");
                    if (index1 > 0)
                    {
                        index2 = strPageContent.IndexOf("\"", index1 + "content=\"".Length);
                        if (index2 < 0)
                            index2 = strPageContent.IndexOf("'", index1 + "content=\"".Length);
                        if (index2 >= 0)
                            return strPageContent.Substring(index1 + "content=\"".Length, index2 - index1 - "content=\"".Length);
                    }
                }

                index1 = strPageContent.IndexOf("<title>");
                if (index1 > 0)
                {
                    index2 = strPageContent.IndexOf("</title>", index1 + "<title>".Length);
                    if (index2 > 0)
                        return strPageContent.Substring(index1 + "<title>".Length, index2 - index1 - "<title>".Length).Trim();
                }
            }
            catch { }
            return "";
        }
        #endregion
    }

    public class CookieAwareWebClient : WebClient
    {
        private readonly CookieContainer m_container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest webRequest = request as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.CookieContainer = m_container;
            }
            return request;
        }
    }
}
