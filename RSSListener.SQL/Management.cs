using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using RSSListener.SQL.Selects;

namespace RSSListener.SQL
{
    public class Management
    {
        private SqlConnection connection = null;
        private Selects.SelectArtists artists = null;
        private Selects.SelectGenres genres = null;
        private Selects.SelectPodcast podcasts = null;
        private Selects.SelectShows shows = null;
        private Selects.SelectXMLDefinitions xmldefinitions = null;
        private Selects.SelectRulesV2 rules = null;
        public Management(SqlConnection conn, RSSListener.Data.Model.cLogger logger)
        {
            if (conn != null)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    connection = conn;
                    artists = new SelectArtists(connection, logger);
                    genres = new SelectGenres(connection, logger);
                    podcasts = new SelectPodcast(connection, this, logger);
                    shows = new SelectShows(connection, logger);
                    xmldefinitions = new SelectXMLDefinitions(conn, logger, this);
                    rules = new SelectRulesV2(conn, logger);
                }
                else
                {
                    throw new Exception("The SQL Connector's state is not open\r\n" + "State is: " + conn.State.ToString());
                }
            }
            else
            {
                throw new Exception("The connection object that was passed is null");
            }
        }
        #region Artist
        public RSSListener.Data.Model.Artist getArtistByName(string name, bool insertifnotfound=false)
        {
            RSSListener.Data.Model.Artist g = artists.getByName(name);
            if (g == null)
            {
                if (insertifnotfound)
                {
                    RSSListener.SQL.Inserts.InsertArtist ig = new Inserts.InsertArtist(connection, new Data.Model.Artist(name, false));
                    ig.writeInformation();
                    g = ig.artist;
                }
                else
                {
                    throw new Exception("Genre was not found and insertion flag is false");
                }
            }
            return g;
        }
        #endregion
        #region Genre
        public RSSListener.Data.Model.Genre getGenreByName(string name, bool insertifnotfound = false)
        {
            RSSListener.Data.Model.Genre g = genres.getByName(name);
            if (g == null)
            {
                if (insertifnotfound)
                {
                    RSSListener.SQL.Inserts.InsertGenre ig = new Inserts.InsertGenre(connection, new Data.Model.Genre(name));
                    ig.writeInformation();
                    g = ig.genre;
                }
                else
                {
                    throw new Exception("Genre was not found and insertion flag is false");
                }
            }
            return g;
        }
        public RSSListener.Data.Model.Genre getGenreByID(Guid id)
        {
            RSSListener.Data.Model.Genre g = genres.getByID(id);
            return g;
        }
#endregion
        #region Podcast
        public List<RSSListener.Data.Model.PodCast> getAllPodcasts()
        {
            List<RSSListener.Data.Model.PodCast> podcastlist = podcasts.getAll();

            return podcastlist;
        }
        public RSSListener.Data.Model.PodCast getPodcast(string name)
        {
            return podcasts.getByName(name);
        }
        #endregion
        #region Show
        public RSSListener.Data.Model.Show getShow(string name)
        {
            return shows.getByName(name);
        }
        public void InsertShow(RSSListener.Data.Model.Show s)
        {
            RSSListener.SQL.Inserts.InsertShow iss = new Inserts.InsertShow(connection, s);
            iss.writeInformation();
        }
        #endregion
        #region Rules
        public RSSListener.Data.Model.Rule2 getRule(Guid id)
        {
            return rules.getByID(id);
        }
        #endregion
        #region XMLPodCastData
        public void InsertPodcastXMLData(RSSListener.Data.Model.PodcastXMLData p)
        {
            RSSListener.SQL.Inserts.InsertPodcastXMLData ipd = new Inserts.InsertPodcastXMLData(connection, p);
            ipd.writeInformation();
        }
#endregion
        #region XMLDefinition
        public RSSListener.Data.Model.XMLDefinition getDefinitionByPodcast(Data.Model.PodCast podcast)
        {
            Data.Model.XMLDefinition definition = xmldefinitions.getByPodcast(podcast);
            return definition;
        }
        #endregion
    }
}
