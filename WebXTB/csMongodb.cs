using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core;

namespace WebsiteMonitoring
{
    #region {define class}
    public class Mongo_Target
    {
        public ObjectId _id { get; set; }
        public string NewsCatalog { get; set; }
        public string Group { get; set; }
        public string Type { get; set; }
        public int TotalViews { get; set; }
        public int Subscribers { get; set; }
        public string SiteAddress { get; set; }
        public string SiteTitle { get; set; }
        public string PriorityLevel { get; set; }
        public string AccessStatus { get; set; }
        public string FirstSeenDate { get; set; }
        public string LastPostedDate { get; set; }
        public string DetectedDate { get; set; }
        public string CrawlerArea { get; set; }
        public string Info { get; set; }
        public string Charset { get; set; }
        public string Proxy { get; set; }

        public Mongo_Target()
        {
            _id = ObjectId.Empty;
            NewsCatalog = string.Empty;
            Group = string.Empty;
            SiteAddress = string.Empty;
            SiteTitle = string.Empty;
            PriorityLevel = string.Empty;
            AccessStatus = string.Empty;
            FirstSeenDate = string.Empty;
            LastPostedDate = string.Empty;
            DetectedDate = string.Empty;
            CrawlerArea = string.Empty;
            Info = string.Empty;
            Charset = string.Empty;
            Proxy = string.Empty;
        }
    }
    public class Mongo_Keyword
    {
        public ObjectId _id { get; set; }
        public string Keyword { get; set; }
        public Mongo_Keyword()
        {
            _id = ObjectId.Empty;
            Keyword = string.Empty;
        }
    }
    public class Mongo_NewsFilterKeyword
    {
        public ObjectId _id { get; set; }
        public string Keyword { get; set; }
        public string NewsCatalog { get; set; }
        public Mongo_NewsFilterKeyword()
        {
            _id = ObjectId.Empty;
            Keyword = string.Empty;
            NewsCatalog = string.Empty;
        }
    }
    public class Mongo_DataSender
    {
        public ObjectId _id { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string NewsCatalog { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public string Keywords { get; set; }
        public string SendData { get; set; }
        public Mongo_DataSender()
        {
            _id = ObjectId.Empty;
            Username = string.Empty;
            EmailAddress = string.Empty;
            NewsCatalog = string.Empty;
            BeginDate = string.Empty;
            EndDate = string.Empty;
            Keywords = string.Empty;
            SendData = string.Empty;
        }
    }
    public class Mongo_Event
    {
        public ObjectId _id { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public string Keywords { get; set; }
        public string Annual { get; set; }
        public string Upcomming { get; set; }
        public Mongo_Event()
        {
            _id = ObjectId.Empty;
            EventName = string.Empty;
            Description = string.Empty;
            BeginDate = string.Empty;
            EndDate = string.Empty;
            Annual = string.Empty;
            Keywords = string.Empty;
            Upcomming = string.Empty;

        }
    }
    public class Mongo_KeywordCalc
    {
        public ObjectId _id { get; set; }
        public string Keyword { get; set; }
        public string NewValue { get; set; }
        public Mongo_KeywordCalc()
        {
            _id = ObjectId.Empty;
            Keyword = string.Empty;
            NewValue = string.Empty;

        }
    }
    public class Mongo_Zalo
    {
        public ObjectId _id { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string FoundTime { get; set; }
        public string Request { get; set; }
        public byte[] Image { get; set; }
        public Mongo_Zalo()
        {
            _id = ObjectId.Empty;
            PhoneNumber = string.Empty;
            Name = string.Empty;
            FoundTime = string.Empty;
            Request = "No";
            Image = null;
        }
    }
    public class Mongo_NetID
    {
        public ObjectId _id { get; set; }
        public string NetID { get; set; }
        public Mongo_NetID()
        {
            _id = ObjectId.Empty;
            NetID = string.Empty;
        }
    }
    public class Mongo_EarlyWarningKeyword
    {
        public ObjectId _id { get; set; }
        public string Username { get; set; }
        public string Subject { get; set; }
        public string Keyword { get; set; }
        public bool Alert { get; set; }
        public Mongo_EarlyWarningKeyword()
        {
            _id = ObjectId.Empty;
            Username = string.Empty;
            Subject = string.Empty;
            Keyword = string.Empty;
            Alert = false;
        }
    }
    public class Mongo_NewTarget
    {
        public ObjectId _id { get; set; }
        public string SiteAddress { get; set; }
        public string SiteTitle { get; set; }
        public string FirstSeenDate { get; set; }
        public string DetectedDate { get; set; }
        public byte[] Image { get; set; }
        public Mongo_NewTarget()
        {
            _id = ObjectId.Empty;
            SiteAddress = string.Empty;
            SiteTitle = string.Empty;
            FirstSeenDate = string.Empty;
            DetectedDate = string.Empty;
            Image = null;
        }
    }
    public class Mongo_NormalTarget
    {
        public ObjectId _id { get; set; }
        public string SiteAddress { get; set; }
        public string SiteTitle { get; set; }
        public string UpdatedTime { get; set; }
        public Mongo_NormalTarget()
        {
            _id = ObjectId.Empty;
            SiteAddress = string.Empty;
            SiteTitle = string.Empty;
            UpdatedTime = string.Empty;
        }
    }
    public class Mongo_News
    {
        public ObjectId _id { get; set; }
        public string SiteAddress { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public string PostedDate { get; set; }
        public string NewsContent { get; set; }
        public Mongo_News()
        {
            _id = ObjectId.Empty;
            SiteAddress = string.Empty;
            Title = string.Empty;
            URL = string.Empty;
            PostedDate = string.Empty;
            NewsContent = string.Empty;
        }
    }
    public class Mongo_SocialNetworkAccount
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Network { get; set; }
        public string Catalog { get; set; }
        public string Followed { get; set; }
        public string Friends { get; set; }
        public string ProfileID { get; set; }
        public string ProfileURL { get; set; }
        public Mongo_SocialNetworkAccount()
        {
            _id = ObjectId.Empty;
            Name = string.Empty;
            Network = string.Empty;
            Catalog = string.Empty;
            Followed = "0";
            Friends = "0";
            ProfileID = string.Empty;
            ProfileURL = string.Empty;
        }
    }
    public class Mongo_MindsAccount
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Network { get; set; }
        public string Catalog { get; set; }
        public string Subscribers { get; set; }
        public string Views { get; set; }
        public string ProfileID { get; set; }
        public string ProfileURL { get; set; }
        public Mongo_MindsAccount()
        {
            _id = ObjectId.Empty;
            Name = string.Empty;
            Network = string.Empty;
            Catalog = string.Empty;
            Subscribers = "0";
            Views = "0";
            ProfileID = string.Empty;
            ProfileURL = string.Empty;
        }
    }

    public class Mongo_SocialNetworkData
    {
        public ObjectId _id { get; set; }
        public string TCatcherUser { get; set; }
        public string Time { get; set; }
        public string Domain { get; set; }
        public string Sumary { get; set; }
        public string NewsURL { get; set; }
        public string ProfileURL { get; set; }
        public string AccountName { get; set; }
        public string AccountID { get; set; }
        public string Followed { get; set; }
        public string Catalog { get; set; }
        public Mongo_SocialNetworkData()
        {
            _id = ObjectId.Empty;
            TCatcherUser = string.Empty;
            Time = string.Empty;
            Domain = string.Empty;
            Sumary = string.Empty;
            NewsURL = string.Empty;
            ProfileURL = string.Empty;
            AccountName = string.Empty;
            AccountID = string.Empty;
            Followed = "0";
            Catalog = "LT";
        }
    }
    public class StatisticsSocialNetworkNews
    {
        public string TitleNews { get; set; }
        public int Count { get; set; }

        public StatisticsSocialNetworkNews(string title, int count)
        {
            TitleNews = title;
            Count = count;
        }
    }
    public class StatisticsSocialNetworkAccount
    {
        public string AccountID { get; set; }
        public int Count { get; set; }

        public StatisticsSocialNetworkAccount(string id, int count)
        {
            AccountID = id;
            Count = count;
        }
    }
    public class StatisticsAccountFollowNews
    {
        public string AccountID { get; set; }
        public string AccountName { get; set; }
        public string ProfileURL { get; set; }
        public string FirstPost { get; set; }
        public string LastPost { get; set; }
        public int Count { get; set; }

        public StatisticsAccountFollowNews(string id,string name, string profile, string strfirstpost, string strlastpost, int count)
        {
            AccountID = id;
            AccountName = name;
            ProfileURL = profile;
            FirstPost = strfirstpost;
            LastPost = strlastpost;
            Count = count;
        }
    }
    public class StatisticsNewsFollowAccount
    {
        public string Sumary { get; set; }
        public string NewsURL { get; set; }
        public string FirstPost { get; set; }
        public string LastPost { get; set; }
        public int Count { get; set; }

        public StatisticsNewsFollowAccount(string strsumary, string strnewsurl,  string strfirstpost, string strlastpost, int count)
        {
            Sumary = strsumary;
            NewsURL = strnewsurl;
            FirstPost = strfirstpost;
            LastPost = strlastpost;
            Count = count;
        }
    }
    public class Mongo_EarlyWarningNews
    {
        public ObjectId _id { get; set; }
        public string Username { get; set; }
        public string Subject { get; set; }
        public string Keyword { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }
        public string NewsCatalog { get; set; }
        public string PostedDate { get; set; }
        public bool Viewed { get; set; }
        public Mongo_EarlyWarningNews()
        {
            _id = ObjectId.Empty;
            Username = string.Empty;
            Subject = string.Empty;
            Keyword = string.Empty;
            URL = string.Empty;
            Title = string.Empty;
            NewsCatalog = string.Empty;
            PostedDate = string.Empty;
            Viewed = false;
        }
    }

    public class Mongo_PottyTitleOfImportantTarget
    {
        public ObjectId _id { get; set; }
        public string SiteName { get; set; }
        public string Title { get; set; }
        public Mongo_PottyTitleOfImportantTarget()
        {
            _id = ObjectId.Empty;
            SiteName = string.Empty;
            Title = string.Empty;
        }
    }
    public class Mongo_StringNoiseInContent
    {
        public ObjectId _id { get; set; }
        public string SiteName { get; set; }
        public string StringNoise { get; set; }
        public Mongo_StringNoiseInContent()
        {
            _id = ObjectId.Empty;
            SiteName = string.Empty;
            StringNoise = string.Empty;
        }
    }
    public class Mongo_PottyLinkOfImportantTarget
    {
        public ObjectId _id { get; set; }
        public string Link { get; set; }
        public Mongo_PottyLinkOfImportantTarget()
        {
            _id = ObjectId.Empty;
            Link = string.Empty;
        }
    }

    public class Mongo_Account
    {
         public ObjectId _id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Permission { get; set; }
        public string AccessStatus { get; set; }
        public string LastLoginTime { get; set; }
     
        public Mongo_Account()
        {
            _id = ObjectId.Empty;
            Username = string.Empty;
            Password = string.Empty;
            Permission = string.Empty;
            AccessStatus = string.Empty;
            LastLoginTime = string.Empty;
      }
    }
      public class Mongo_ScreenshotOfServer
    {
        public ObjectId _id { get; set; }
        public string IndexImage { get; set; }
        public byte[] Image { get; set; }
        public Mongo_ScreenshotOfServer()
        {
            _id = ObjectId.Empty;
            IndexImage = string.Empty;
            Image = null;
        }
    }
    #endregion
    public class csGetMongoData1
    {
        public csGetMongoData1()
        { }
        #region {Get Database}
        static public IMongoDatabase GetDatabase()
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(csGlobal.strMongodbConnection));
            MongoClient mongoClient = new MongoClient(settings);
            return mongoClient.GetDatabase(csGlobal.strMongoDatabase1);

        }
        #endregion
    }
    public class csGetMongoData
    {
        public csGetMongoData()
        { }
        #region {Get Database}
        static public IMongoDatabase GetDatabase(string strDB)
        {
            try
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(csGlobal.strMongodbConnection));
                MongoClient mongoClient = new MongoClient(settings);
            return mongoClient.GetDatabase(strDB);
            }
             catch { }
            return null;
        }
        #endregion
        #region
        static public IMongoDatabase GetDatabase1()
        {
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(csGlobal.strMongodbConnection));
            MongoClient mongoClient = new MongoClient(settings);
            return mongoClient.GetDatabase(csGlobal.strMongoDatabase1);

        }
        #endregion

        #region {GetNewsContent}
        static public string GetNewsContent(string strURL,  string strDB, string strCollection)
        {
            if (strDB == "dbAntiVietNamNews")
                strCollection = "HighPriority_NewsDetail";
            var db = GetDatabase(strDB);
            var collection = db.GetCollection<Mongo_News>(strCollection);
            var result = collection.AsQueryable().Where(e => e.URL == strURL).Select(p => new { p.NewsContent });
            if (result.Count() > 0)
            {
                foreach (var row in result)
                    return row.NewsContent;
            }
            return "--------- News article summary will arrive soon! ---------";
        }
       
        #endregion
        #region {UpdateNewsContent}
        static public void UpdateNewsContent(string strNewsContent, string strURL, string strDB, string strCollection)
        {
            var db = GetDatabase(strDB);
            var collection = db.GetCollection<Mongo_News>(strCollection);
            var filter = Builders<Mongo_News>.Filter.Eq(e => e.URL, strURL);
            var update = Builders<Mongo_News>.Update.Set(e => e.NewsContent, strNewsContent);
            collection.UpdateOne(filter, update);
        }
        #endregion
        #region {UpdateNewSite}
        static public void UpdateOne(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update, string strDB, string strCollection)
        {
            var db = GetDatabase(strDB);
            var collection = db.GetCollection<BsonDocument>(strCollection);
            collection.UpdateOne(filter, update);
        }
        #endregion

        #region {GetLisNewsFiltertKeyword}
        static public void GetLisNewsFiltertKeyword(ref List<string> lst, string strCollection, string strNewsCatalog)
        {
            lst.Clear();
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_NewsFilterKeyword>(strCollection);

            var result = collection.AsQueryable().Where(p=>p.NewsCatalog==strNewsCatalog).Select(p => new { p.Keyword }).OrderBy(p => p.Keyword);
            if (result.Count() > 0)
                foreach (var row in result)
                {
                    lst.Add(row.Keyword);
                }
        }
        #endregion
        #region {GetListSocialNetworkID}
        static public void GetListSocialNetworkID(ref List<string> lst, string strCollection, string strNewsCatalog)
        {
            lst.Clear();
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_SocialNetworkAccount>(strNewsCatalog);

            var result = collection.AsQueryable().Select(p => new { p.ProfileID });
            if (result.Count() > 0)
                foreach (var row in result)
                {
                    lst.Add(row.ProfileID);
                }
        }
        #endregion
        #region {GetListKeyword}
        static public void GetListKeyword(ref List<string> lst,  string strCollection)
        {
            lst.Clear();
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_Keyword>(strCollection);

            var result = collection.AsQueryable().Select(p => new { p.Keyword }).OrderBy(p=>p.Keyword);
            if (result.Count() > 0)
                foreach (var row in result)
                {
                    lst.Add(row.Keyword);
                }
        }
        #endregion

        #region {GetListTarget}
        static public void GetListTarget(ref List<string> lst, string strCollection)
        {
            lst.Clear();
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_Target>(strCollection);

            var result = collection.AsQueryable().Where(p=>p.NewsCatalog=="AntiVietNam").Select(p => new { p.SiteAddress }).OrderBy(p => p.SiteAddress);
            if (result.Count() > 0)
                foreach (var row in result)
                {
                    lst.Add(row.SiteAddress);
                }
        }
        #endregion
        #region {GetListTarget}
        static public void GetListYoutubeChannel(ref List<string> lst, string strCollection)
        {
            lst.Clear();
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_Target>(strCollection);

            var result = collection.AsQueryable().Where(p => p.NewsCatalog == "AntiVietNam" && p.SiteAddress.Contains("youtube.com")).Select(p => new { p.SiteTitle }).OrderBy(p => p.SiteTitle);
            if (result.Count() > 0)
                foreach (var row in result)
                {
                    lst.Add(row.SiteTitle);
                }
        }
        #endregion

        #region {GetTargetTitle}
        static public string GetTargetTitle(string strTarget, string strCollection) 
        {
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_Target>(strCollection);

            var result = collection.AsQueryable().Where(p => p.SiteAddress.Contains(strTarget)).Select(p => new { p.SiteTitle });
            if (result.Count() > 0)
                foreach (var row in result)
                {
                    return row.SiteTitle.Trim();
                }
            return "";
        }

        static public string GetMindsAccountName(string strTarget, string strCollection)
        {
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_MindsAccount>(strCollection);

            var result = collection.AsQueryable().Where(p => p.ProfileURL.Contains(strTarget)).Select(p => new { p.ProfileID, p.Name });
            if (result.Count() > 0)
                foreach (var row in result)
                {
                    return row.Name.Trim()+" - @"+row.ProfileID;
                }
            return strTarget.Replace("https://www.minds.com/","");
        }
        #endregion
        #region {GetCharset_Proxy}
        static public void GetCharset_Proxy(ref string strProxy, ref string strCharset, string strSiteName, string strCollection)
        {
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_Target>(strCollection);
            var result = collection.AsQueryable().Where(p => p.SiteAddress.Contains(strSiteName)).Select(p => new { p.Proxy, p.Charset });
            if (result.Count() > 0)
                foreach (var row in result)
                {
                    strProxy = row.Proxy;
                    strCharset = row.Charset;
                    return;
                }
        }
        #endregion
        #region {UpdateCharset_Proxy}
        static public void UpdateCharset_Proxy(string strProxy, string strCharset, string strSiteName,  string strCollection)
        {
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_Target>(strCollection);
       
            var filter = Builders<Mongo_Target>.Filter.Regex(e => e.SiteAddress, new BsonRegularExpression(strSiteName));
            var update = Builders<Mongo_Target>.Update.Set(e => e.Charset, strCharset).Set(e => e.Proxy, strProxy);
            collection.UpdateOne(filter, update);
        }
        #endregion
        #region {UpdateNews}
        static public void UpdateNews(string strTitle, string strNewsContent, string strURL, string strDB, string strCollection)
        {
            var db = GetDatabase(strDB);
            var collection = db.GetCollection<Mongo_News>(strCollection);
            var filter = Builders<Mongo_News>.Filter.Eq(e => e.URL, strURL);
            var update = Builders<Mongo_News>.Update.Set(e => e.Title, strTitle).Set(e => e.NewsContent, strNewsContent);
            collection.UpdateOne(filter, update);
        }
        #endregion

        #region {DeleteDocument}
        static public void DeleteDocument(FilterDefinition<BsonDocument> filter, string strDB, string strCollection)
        {
            var db = GetDatabase(strDB);
            var collection = db.GetCollection<BsonDocument>(strCollection);
            collection.DeleteMany(filter);
        }
        #endregion

        #region {AddPottyLinkOfSite}
        static public void AddPottyLinkOfImportantTarget(string strLink, string strCollection)
        {
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_PottyLinkOfImportantTarget>(strCollection);
            var result = collection.AsQueryable().Where(p => p.Link == strLink).Select(p => new { p.Link });
            if (result.Count() > 0) return;
            Mongo_PottyLinkOfImportantTarget document = new Mongo_PottyLinkOfImportantTarget();
            document.Link = strLink;
            collection.InsertOne(document);
        }
        #endregion
        #region {AddPottyTitleOfSite}
        static public void AddPottyTitleOfSite(string strSiteName, string strTitle, string strCollection)
        {
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_PottyTitleOfImportantTarget>(strCollection);
            var result = collection.AsQueryable().Where(p => p.SiteName == strSiteName && p.Title == strTitle).Select(p => new { p.SiteName });
            if (result.Count() > 0) return;
            Mongo_PottyTitleOfImportantTarget document = new Mongo_PottyTitleOfImportantTarget();
            document.SiteName = strSiteName;
            document.Title = strTitle;
            collection.InsertOne(document);
        }
        #endregion
        #region {AddPottyTitleOfNormalTarget}
        static public void AddPottyTitleOfNormalTarget(string strTitle, string strCollection)
        {
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_Keyword>(strCollection);
            var result = collection.AsQueryable().Where(p => p.Keyword == strTitle).Select(p => new { p.Keyword });
            if (result.Count() > 0) return;
            Mongo_Keyword document = new Mongo_Keyword();
            document.Keyword = strTitle;
            collection.InsertOne(document);
        }
        #endregion
        #region {Insert into NormalTarget}
        static public void InsertIntoNormalTarget(string strSiteAddress, string strTitle)
        {
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_NormalTarget>("NormalTarget");
            var count = collection.AsQueryable().Where(p => p.SiteAddress == strSiteAddress).Select(p => new { p.SiteAddress }).Count();
            if (count > 0) return;
            DateTime dtNow = DateTime.Now;
            string strUpdatedTime = dtNow.Year.ToString() + "/" + dtNow.Month.ToString().PadLeft(2, '0') + "/" + dtNow.Day.ToString().PadLeft(2, '0') + " " +
                          dtNow.Hour.ToString().PadLeft(2, '0') + ":" + dtNow.Minute.ToString().PadLeft(2, '0') + ":" + dtNow.Second.ToString().PadLeft(2, '0');

            Mongo_NormalTarget document = new Mongo_NormalTarget();
            document.SiteAddress = strSiteAddress;
            document.SiteTitle = strTitle;
            document.UpdatedTime = strUpdatedTime;
            collection.InsertOne(document);
        }
        #endregion
        #region {GetListNetID}
        static public void GetListNetID(ref List<string> lst)
        {
            lst.Clear();
            var db = GetDatabase(csGlobal.strMongoDatabase);
            var collection = db.GetCollection<Mongo_NetID>("NetID");

            var result = collection.AsQueryable().Select(p => new { p.NetID });
            if (result.Count() > 0)
                foreach (var row in result)
                {
                    lst.Add(row.NetID);
                }
        }
        #endregion
        #region {Insert into InsertIntoNewTarget}
        static public void InsertIntoNewTarget(string strSiteAddress, string strDB, string strTitle, string strHtmlSource)
        {
            var db = GetDatabase(strDB);
            var collection = db.GetCollection<Mongo_NewTarget>("NewTarget");
            var count = collection.AsQueryable().Where(p => p.SiteAddress == strSiteAddress).Select(p => new { p.SiteAddress }).Count();
            if (count > 0) return;
            DateTime dtNow = DateTime.Now;
            string strDetectedDate = dtNow.Year.ToString() + "/" + dtNow.Month.ToString().PadLeft(2, '0') + "/" + dtNow.Day.ToString().PadLeft(2, '0');

            Mongo_NewTarget document = new Mongo_NewTarget();
            document.SiteAddress = strSiteAddress;
            document.SiteTitle = strTitle;
            document.FirstSeenDate = "";
            document.DetectedDate = strDetectedDate;
            document.Image = null;
            collection.InsertOne(document);
        }
        #endregion
        #region {GetAlertKeyword}
        static public void GetAlertKeyword(ref List<string> lst, string strCollection)
        {
            try
            {
                lst.Clear();
                var db = GetDatabase(csGlobal.strMongoDatabase);
                var collection = db.GetCollection<Mongo_EarlyWarningKeyword>(strCollection);
                var result = collection.AsQueryable().Where(p => p.Alert == true).Select(p => new { p.Keyword }).OrderBy(p => p.Keyword);
                if (result.Count() > 0)
                {
                    foreach (var row in result)
                        lst.Add(row.Keyword);
                }
            }
            catch { }
        }

        #endregion

    }
}
