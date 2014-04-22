using System;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using SqlSyncUsageMeter.Rss;
using System.Collections.Generic;
using System.IO;
using System.Text;
/// <summary>
/// Summary description for UsageMeter
/// </summary>
[WebService(Namespace = "http://www.globalcrossing.com/SqlSyncUsageMeter/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class UsageMeter : System.Web.Services.WebService
{

    public UsageMeter()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string RecordUsageRecord(SqlSync.UsageMeter.UsageRecord[] usageRecords)
    {
        string message = "Successful";
        System.Configuration.AppSettingsReader app = new System.Configuration.AppSettingsReader();
        string fileName = (string)app.GetValue("feedFile",typeof(string));
        rss feed = ReadFeedXml(fileName,out message);
        if (feed == null)
            return message;
            
        List<SqlSyncUsageMeter.Rss.item> lst = new List<item>();
        lst.AddRange(feed.channel.item);
        for(int i=0;i<usageRecords.Length;i++)
        {
            SqlSyncUsageMeter.Rss.item item = new item();
            item.title = usageRecords[i].BuildFileName + " @ " + usageRecords[i].FileOpenUTC.ToLocalTime().ToString() + "(" + TimeZone.CurrentTimeZone.GetUtcOffset(usageRecords[i].FileOpenUTC).ToString() + ")";
            item.description = "Version " + usageRecords[i].SqlSyncVersion + "<BR>" +
                usageRecords[i].User + "<BR>" +
                usageRecords[i].HostMachineName;
            lst.Add(item);
        }
        feed.channel.item = lst.ToArray();
        SaveFeedXml(fileName, feed, out message);
        return message;
    }
    public void SaveFeedXml(string fileName, rss data, out string message)
    {
        message = "";
        System.Xml.XmlTextWriter tw = null;
        try
        {
            XmlSerializer xmlS = new XmlSerializer(typeof(rss));
            tw = new System.Xml.XmlTextWriter(fileName, Encoding.UTF8);
            tw.Formatting = System.Xml.Formatting.Indented;
            tw.Indentation = 3;
            xmlS.Serialize(tw,data);
            return;
        }
        catch (Exception exe)
        {
            message = exe.Message;
            return;
        }
        finally
        {
            if (tw != null)
                tw.Close();
        }

    }
    public static rss ReadFeedXml(string fileName,out string message)
    {
        message = "";
        if (!File.Exists(fileName))
        {
            message = "Can't find feed file";
            return null;
        }

        rss feed = null;
        using (StreamReader sr = new StreamReader(fileName))
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(rss));
                object obj = serializer.Deserialize(sr);
                feed = (rss)obj;
            }
            catch (Exception exe)
            {
                message = exe.Message;
            }

        }
        return feed;
    }

}

