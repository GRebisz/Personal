using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSListener.Data.Model
{
    public class Show
    {
        public Guid id { get; set; }
        public string title = "";
        public string album = "";
        public string filename = "";
        public string url = "";
        public string dnbpath = "";
        public PodCast parentpodcast = null;
        public Artist artist = null;
        public DateTime Recorded = DateTime.MinValue;
        public Genre genre = null;
        private cLogger logger = null;
        private int downloadperc = -1;
        public bool fileExists = false;
        public bool needsDownload = false;
        public bool downloadComplete = false;
        public string Tracklist = "";
        public Show(string title, Artist artist, string album, string url, Genre genre, string description, string path, cLogger logger, PodCast p)
        {
            this.logger = logger;
            this.title = title;
            this.album = album;
            this.url = url;
            this.artist = artist;
            this.filename = url.Substring(url.LastIndexOf("/") + 1);
            this.genre = genre;
            Tracklist = description;
            this.dnbpath = path;
            parentpodcast = p;
            bool exists = fileAlreadyExists(path);
            if (!exists)
            {
                logger.addEvent(new cEvent(Severity.Information, "File: " + filename + " is new"));
            }
            else
            {
                writeTagInfo(Capitalise(title), dnbpath + filename, Capitalise(genre.GenreName), Capitalise(artist.Artistname));
            }
        }
        private bool fileAlreadyExists(string dnbpath)
        {
            string[] filelist = null;
            filelist = System.IO.Directory.GetFiles(dnbpath, "*" + filename);
            if (filelist.Length == 1)
            {
                fileExists = true;
                return true;
            }
            else if(filelist.Length > 1)
            {
                if (logger != null)
                {
                    logger.addEvent(new cEvent(Severity.Error, "There was more than one copy found of: " + filename + " - in folder: " + dnbpath));
                }
                //Throw an exception of some kind if multiples are found
            }
            return false;
        }
        public void downloadFile(string url)
        {
            string notification = "Title: " + title + "\n" +
                                    "URL: " + url;
            //Starting download
            logger.addEvent(new cEvent(Severity.Information, "NEW DOWNLOAD!!!\n\n" + notification));
            //Download completion
            string downloadedpath = "";
            try
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                wc.DownloadFileAsync(new Uri(url), dnbpath + url.Substring(url.LastIndexOf("/") + 1));
                Console.WriteLine("Downloading: \nArtist: " + Capitalise(artist.Artistname) + "\nTitle: " + Capitalise(title));
                while (wc.IsBusy) { }
                downloadedpath = dnbpath + url.Substring(url.LastIndexOf("/") + 1);
                downloadperc = -1;
                logger.addEvent(new cEvent(Severity.Information, "Download complete - " + url));
                writeTagInfo(Capitalise(title), downloadedpath, Capitalise(genre.GenreName), Capitalise(artist.Artistname));
            }
            catch (System.Net.WebException webex)
            {
                logger.addEvent(new cEvent(Severity.Error, "There was an error downloading: " + url + "\n\n" + webex.Message + "\n\n", webex));
            }
        }
        public override string ToString()
        {
            return this.title;
        }
        private void writeTagInfo(string title, string path, string genre, string artist)
        {
            try
            {
                bool changed = false;
                TagLib.Mpeg.AudioFile file = new TagLib.Mpeg.AudioFile(path);
                if (String.IsNullOrWhiteSpace(file.Tag.Title) || file.Tag.Title.ToLower() != title.ToLower()) { file.Tag.Title = Capitalise(title); changed = true; };
                if (file.Tag.Genres.Length == 0 || file.Tag.Genres[0].ToLower() != genre.ToLower()) { file.Tag.Genres = new string[1] { Capitalise(genre) }; changed = true; };
                if (file.Tag.Performers.Length == 0 || file.Tag.Performers[0] != artist.ToLower()) { file.Tag.Performers = new string[1] { Capitalise(artist) }; changed = true; };
                if (file.Tag.Lyrics == null) { file.Tag.Lyrics = Tracklist; changed = true; };
                if (file.Tag.Album == null || file.Tag.Album.ToLower() != album.ToLower()) { file.Tag.Album = Capitalise(album); changed = true; }
                if (changed)
                {
                    Console.WriteLine("CHG");
                    file.Save();
                }
            }
            catch (Exception ex)
            {
                logger.addEvent(new cEvent(Severity.Error, "An error occured while writing tags for the current file:\r\n" + "Title: " + title + "\r\n" +
                    "Path: " + path + "\r\nGenre: " + genre + "\r\n" + artist, ex));
            }
        }

        private string Capitalise(string input)
        {
            string[] spaced = input.Split(' ');
            string newstring = "";
            foreach (string s in spaced)
            {
                if (s.Length > 1)
                {
                    newstring += s[0].ToString().ToUpper() + s.Substring(1) + " ";
                }
                else
                {
                    newstring += s.ToString().ToUpper();
                }
            }
            return newstring;
        }

        void wc_DownloadDataCompleted(object sender, System.Net.DownloadDataCompletedEventArgs e)
        {
            downloadperc = -1;
        }

        void wc_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {

            if (e.ProgressPercentage > downloadperc)
            {
                Console.WriteLine(e.BytesReceived + "bytes / " + e.TotalBytesToReceive + "bytes" + " = " + e.ProgressPercentage + "% complete");
                downloadperc++;
                if (downloadperc % 10 == 0)
                {
                    logger.addEvent(new cEvent(Severity.Information, e.BytesReceived + "bytes / " + e.TotalBytesToReceive + "bytes = " + e.ProgressPercentage + "% complete"));
                }
            }
        }
    }
}
