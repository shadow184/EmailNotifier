﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailNotifier.Interfaces;
using EmailNotifier.Models;
using Newtonsoft.Json;
using ApiLibrary.Classes;
using ApiLibrary.Interfaces;
using ApiLibrary.Models;
using MoreLinq;

namespace EmailNotifier.Classes
{
    public class DataProcessor : IDataProcessor
    {
        private readonly IApiHelper _apiHelper;
        private readonly IEmailHelper _emailHelper;
        private readonly IDataHelper _dataHelper;
        private readonly IConfigurationHelper _configurationHelper;

        public DataProcessor(IApiHelper apiHelper, IConfigurationHelper configurationHelper, IEmailHelper emailHelper, IDataHelper dataHelper)
        {
            _apiHelper = apiHelper;
            _configurationHelper = configurationHelper;
            _emailHelper = emailHelper;
            _dataHelper = dataHelper;
        }

        public void ProcessShows()
        {
            var json = _apiHelper.Get(_dataHelper.GetApiCall("api/history", "sortKey=date&sortDir=desc"));
            var data = JsonConvert.DeserializeObject<DownloadsRoot>(json);

            var dbShows = _dataHelper.GetDbShows();

            var emails = GetEmailsFromShows(dbShows);

            var subjectLines = new List<string>();

            var grabbed = data.Episodes.Where(x => string.Equals(x.EventType, "grabbed")).ToList();

            foreach (var e in grabbed)
            {
                if (!_configurationHelper.KaylaShows.Split('|').Contains(e.Series.Title, StringComparer.InvariantCultureIgnoreCase))
                {
                    if (DateTime.Parse(e.Date) >= DateTime.Now.AddHours(-1))
                    {
                        CreateShowList(e, subjectLines);
                    }
                }

                if (DateTime.Parse(e.Date) >= DateTime.Now.AddHours(-1))
                {

                }
            }

            if (subjectLines.Count > 0)
            {
                SendEmail(subjectLines, "jeffrey.sugget@gmail.com");
            }
        }

        public void ProcessOtherShows()
        {
            var json = _apiHelper.Get(_dataHelper.GetApiCall("api/history", "sortKey=date&sortDir=desc"));
            var data = JsonConvert.DeserializeObject<DownloadsRoot>(json);

            var kaylaShows = _configurationHelper.KaylaShows.Split('|');
            var subjectLines = new List<string>();

            var grabbed = data.Episodes.Where(x => string.Equals(x.EventType, "grabbed"));

            foreach (var e in grabbed)
            {
                if (kaylaShows.Contains(e.Series.Title, StringComparer.InvariantCultureIgnoreCase))
                {
                    foreach (var s in kaylaShows)
                    {
                        if (e.Series.Title.ToLower() == s.ToLower())
                        {
                            if (DateTime.Parse(e.Date) >= DateTime.Now.AddHours(-1))
                            {
                                CreateShowList(e, subjectLines);
                            }
                        }
                    }
                }
            }

            if (subjectLines.Count > 0)
            {
                SendEmail(subjectLines, "kayla.sugget@gmail.com");
            }
        }

        private void CreateShowList(EpisodeData episodeData, List<string> subjectLines)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Series: {episodeData.Series.Title}");
            sb.AppendLine();
            sb.AppendLine($"Episode Title: {episodeData.Episode.Title}");
            sb.AppendLine();
            sb.AppendLine($"Episode Number: {episodeData.Episode.EpisodeNumber}");
            sb.AppendLine();

            subjectLines.Add(sb.ToString());
        }

        private void SendEmail(List<string> subjectLines, string email)
        {
            var sb = new StringBuilder();

            sb.AppendLine("The following shows have been downloaded");
            sb.AppendLine();
            
            foreach (var l in subjectLines)
            {
                sb.AppendLine(l);
            }

            _emailHelper.SendEmail(_emailHelper.CreateEmail(sb.ToString(), "Shows Downloaded", email));
        }

        private List<string> GetEmailsFromShows(IEnumerable<Show> shows)
        {
            return shows.DistinctBy(x => x.EmailAddress).Select(x => x.EmailAddress).ToList();
        }
    }
}
