using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Data.SqlClient;
using System.Xml;
using RSSListener.Data.Model;
using RSSListener.SQL;

namespace RSSListener
{
    public partial class RSSListenerCore : ServiceBase
    {
        private cLogger logger = new cLogger();
        private System.Configuration.AppSettingsReader apr = new System.Configuration.AppSettingsReader();
        private SqlConnection connection = null;
        private RSSListener.SQL.Management DBContext = null;
        private System.Timers.Timer t = new System.Timers.Timer();
        private int downloadperc = -1;
        
        private bool iscurrentlyinuse = false;
        public RSSListenerCore()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
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
                //Database
                connection = new SqlConnection(connectionstring);
                connection.Open();
                DBContext = new RSSListener.SQL.Management(connection, logger);
                //timer
                //t.Interval = 3600000;
                t.Interval = 60000;
                t.Elapsed += t_Elapsed;
                
                //Operation
                logger.addEvent(new cEvent(Severity.Information, "RSS Feed Service Loaded"));
                try
                {
                    //executeRefresh();
                    t.Enabled = true;
                    t.Start();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.EventLog.WriteEntry("GrebiszDEVEventlog", ex.ToString() + "\n\n" + ex.StackTrace, EventLogEntryType.Error);
                }
                
                //End Of Application
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("GrebiszDEVEventlog", ex.ToString() + "\n\n" + ex.StackTrace, EventLogEntryType.Error);
                this.Stop();
            }
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            executeRefresh();
        }
        private void executeRefresh()
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
                    this.Stop();
                }
                iscurrentlyinuse = false;
            }
        }
        protected override void OnStop()
        {
            t.Stop();
            if (connection != null)
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                {
                    try
                    {
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        logger.addEvent(new cEvent(Severity.Warning, "Could not close connection to SQL", ex));
                    }
                }
            }
            logger.addEvent(new cEvent(Severity.Information, "RSS Feed Service Ended"));
        }

        public void downloadXMLData(PodCast p)
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
        private void fetchPodCastsFromXML(PodCast p, string pathtoxml)
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
