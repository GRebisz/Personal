using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using RSSListener.Data.Model;
using RSSListener.SQL;
using System.Data.SqlClient;

namespace RSSListenerConsole
{
    class Program
    {
        private static cLogger logger = new cLogger();
        private static System.Configuration.AppSettingsReader apr = new System.Configuration.AppSettingsReader();
        private static SqlConnection connection = null;
        private static RSSListener.SQL.Management DBContext = null;
        private static int downloadperc = -1;
        private static bool iscurrentlyinuse = false;
        static void Main(string[] args)
        {
            try
            {
                //Get Settings
                bool writetoeventfile = bool.Parse(apr.GetValue("WriteToTextOutput", "".GetType()).ToString());
                bool writetoeventlog = bool.Parse(apr.GetValue("WriteToEventLogOutput", "".GetType()).ToString());
                string pathtooutputtext = apr.GetValue("TextOutputPath", "".GetType()).ToString();
                bool overwritetextiffound = bool.Parse(apr.GetValue("OverwriteTextIfFound", "".GetType()).ToString());
                string connectionstring = apr.GetValue("DBConnectionString", "".GetType()).ToString(); 
                
                //Apply Settings
                logger.overwriteiffound = overwritetextiffound;
                logger.writeevents = writetoeventlog;
                logger.writetext = writetoeventfile;
                logger.wrtextdesc = pathtooutputtext;
                connection = new SqlConnection(connectionstring);
                connection.Open();
                DBContext = new RSSListener.SQL.Management(connection, logger);
                //Operation
                logger.addEvent(new cEvent(Severity.Information, "RSS Feed Service Loaded"));
                //RunForXML("http://feeds.feedburner.com/dnbradioarchive?format=xml", "c:\\DNBTest\\", false);
                executeRefresh();
                //RunForXML(@"c:\temp\pressure.xml", @"C:\homeboy\DnbRadio.com - Fresh Jungle, DnB, Drum and Bass, and Dubstep (Drum&Bass)");             //Local XML
                //RunForXML(@"c:\temp\dnbradioarchive.XML", @"C:\homeboy\DnbRadio.com - Fresh Jungle, DnB, Drum and Bass, and Dubstep (Drum&Bass)");      //Local XML
                //End Of Application
                logger.addEvent(new cEvent(Severity.Information, "RSS Feed Service Ended"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("GrebiszDEVEventlog", ex.ToString() + "\n\n" + ex.StackTrace, EventLogEntryType.Error);
            }
        }
        private static void executeRefresh()
        {
            if (!iscurrentlyinuse)
            {
                iscurrentlyinuse = true;
                try
                {
                    List<PodCast> podcasts = DBContext.getAllPodcasts();
                    foreach (PodCast p in podcasts)
                    {
                        downloadXMLData(p);
                        foreach (Show s in p.shows)
                        {
                            if (!s.fileExists && p.AutomaticDownload)
                            {
                                s.downloadFile(s.url);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.addEvent(new cEvent(Severity.FATAL, "Exception occured during an iteration", ex));
                }
                iscurrentlyinuse = false;
            }
        }
        private static void RunForXMLv2(string path, string dnbpath, bool islocalxml = true, string Show="", string Genre="")
        {
            Genre g = DBContext.getGenreByName(Genre);
            PodCast p = new PodCast(path, Show,dnbpath, islocalxml, g, false, logger);
            RSSListener.SQL.Inserts.InsertPodcast podcast = new RSSListener.SQL.Inserts.InsertPodcast(connection, p);
            //List<Artist> artists = checkallartists(p.shows);
            //Console.Read();
        }
        public static void downloadXMLData(PodCast p)
        {
            //Download XML Data
            string pathtolocal = @"c:\temp\myxml.xml";
            //Ensure there is no file
            try
            {
                if (System.IO.File.Exists(pathtolocal)) System.IO.File.Delete(pathtolocal);
            }
            catch (System.IO.IOException ioex)
            {
                if (logger != null)
                {
                    logger.addEvent(new cEvent(Severity.Warning, "There was an error attempting to delete the file: " + pathtolocal, ioex));
                }
                return;
            }
            //Download from URL in 'path' variable
            try
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.DownloadFile(p.URL, pathtolocal);
                if (logger != null)
                {
                    logger.addEvent(new cEvent(Severity.Information, "File successfully downloaded from: " + p.URL));
                }
            }
            catch (System.Net.WebException webex)
            {
                if (logger != null)
                {
                    logger.addEvent(new cEvent(Severity.FATAL, "Could not download the RSS feed data from: " + p.URL, webex));
                }
                return;
            }
            string xml = System.IO.File.ReadAllText(pathtolocal);
            fetchPodCastsFromXML(p, pathtolocal);
            if (p.AutomaticDownload)
            {
                foreach (Show s in p.shows)
                {
                    if (!s.fileExists)
                    {
                        s.downloadFile(s.url);
                    }
                }
            }
        }
        private static void fetchPodCastsFromXML(PodCast p, string pathtoxml)
        {
            XMLDefinition definition = DBContext.getDefinitionByPodcast(p);
            if (definition != null)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(pathtoxml);
                    PodcastXMLData pxml = new PodcastXMLData();
                    pxml.fk_podcast = p.id.ToString();
                    pxml.lastbuilddate = doc.SelectSingleNode(definition.xpathToLastBuildDate).InnerText;
                    pxml.podcastdata = System.IO.File.ReadAllText(pathtoxml);
                    DBContext.InsertPodcastXMLData(pxml);
                    XmlNodeList nl = doc.SelectNodes(definition.xpathToItems);
                    //int count = 0;
                    List<Show> shows = new List<Show>();
                    foreach (XmlNode n in nl)
                    {
                        //if (count == 5) { return; }
                        try
                        {
                            string showtitle = "";
                            string artist = "";
                            string readinrecorded = "";
                            string url = "";
                            string description = "";
                            if (definition.TitleRule != null && !String.IsNullOrWhiteSpace(definition.TitleTag))
                            {
                                try
                                {
                                    showtitle = RSSListener.Rules.RuleEngineV2.RunRule(definition.TitleRule, definition, RSSListener.Rules.RuleEngineV2.WrappedXMLDef.TitleTag, n);
                                }
                                catch
                                {
                                    showtitle = n.SelectSingleNode(definition.TitleTag).InnerText;
                                }
                            }
                            else
                            {
                                showtitle = n.SelectSingleNode(definition.TitleTag).InnerText;
                            }
                            if (p.useDefault)
                            {
                                artist = p.DefaultArtistName;
                            }
                            else
                            {
                                if (definition.ArtistRule != null)
                                {
                                    artist = RSSListener.Rules.RuleEngineV2.RunRule(definition.ArtistRule, definition, RSSListener.Rules.RuleEngineV2.WrappedXMLDef.TitleTag, n);
                                }
                                else
                                {
                                    artist = "Unknown";
                                }
                            }
                            if (definition.PublishedRule != null)
                            {
                                readinrecorded = RSSListener.Rules.RuleEngineV2.RunRule(definition.PublishedRule, n.SelectSingleNode(definition.PublishedTag).InnerText);
                            }
                            else
                            {
                                readinrecorded = DateTime.MinValue.ToString();
                            }
                            DateTime recorded = DateTime.Parse(readinrecorded);
                            if (definition.EnclosureRule != null && !String.IsNullOrWhiteSpace(definition.EnclosureTag))
                            {
                                url = RSSListener.Rules.RuleEngineV2.RunRule(definition.EnclosureRule, definition, RSSListener.Rules.RuleEngineV2.WrappedXMLDef.EnclosureTag, n);
                            }
                            else
                            {
                                throw new Exception("Could not disassemble url!\r\n" + n.OuterXml + "\r\nRule:\r\n" + definition.EnclosureRule.id.ToString() + "\r\n" + "Podcast: " + p.PodcastName + " {" + p.id + "}");
                            }
                            if (definition.DescriptionRule != null && !String.IsNullOrWhiteSpace(definition.DescriptionTag))
                            {
                                description = RSSListener.Rules.RuleEngineV2.RunRule(definition.DescriptionRule, n.SelectSingleNode(definition.DescriptionTag).InnerText);
                            }
                            Artist artistobj = DBContext.getArtistByName(artist, true);
                            Genre g = DBContext.getGenreByID(Guid.Parse(p.fk_genre));
                            Show show = new Show(showtitle, artistobj, p.PodcastName, url, g, description, p.podcastpath, logger, p);
                            show.Recorded = recorded;
                            DBContext.InsertShow(show);
                            shows.Add(show);
                        }
                        catch (Exception ex2)
                        {
                            logger.addEvent(new cEvent(Severity.Error, "There was an error trying to parse:\r\n" + n.InnerXml + "\r\n\r\nWith Defition: " + definition.id + "\r\n" + "For Podcast: " + p.PodcastName, ex2));
                        }
                    }
                    p.shows = shows;
                }
                catch (Exception ex)
                {
                    logger.addEvent(new cEvent(Severity.Error, "There was an error during processing of an xml for podcast: " + p.PodcastName, ex));
                }
            }
            else
            {
                logger.addEvent(new cEvent(Severity.Warning, "There is no definition for this podcast: " + p.PodcastName + " {" + p.id + "}"));
            }
        }
    }
}
