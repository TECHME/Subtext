﻿using System;
using System.Data.SqlClient;
using log4net;
using Subtext.Framework.Components;
using Subtext.Framework.Logging;
using System.Text.RegularExpressions;
using Subtext.Framework.Format;

namespace Subtext.Framework.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly static ILog Log = new Log();

        public StatisticsService(ISubtextContext context, Subtext.Framework.Configuration.Tracking settings) {
            SubtextContext = context;
            Settings = settings;
        }

        public ISubtextContext SubtextContext {
            get;
            private set;
        }

        public Subtext.Framework.Configuration.Tracking Settings {
            get;
            private set;
        }

        public void RecordAggregatorView(EntryView entryView) {
            if (!Settings.EnableAggBugs || SubtextContext.HttpContext.Request.HttpMethod == "POST") {
                return;
            }

            entryView.PageViewType = PageViewType.AggView;

            try {
                SubtextContext.Repository.TrackEntry(entryView);
            }
            catch (SqlException e) {
                Log.Error("Could not record Aggregator view", e);
            }
        }

        public void RecordWebView(EntryView entryView) {
            if (!Settings.EnableWebStats || SubtextContext.HttpContext.Request.HttpMethod == "POST") {
                return;
            }

            entryView.ReferralUrl = GetReferral(SubtextContext);

            //todo in the future do this async if we can do true IO async?
            entryView.PageViewType = PageViewType.WebView;
            try {
                SubtextContext.Repository.TrackEntry(entryView);
            }
            catch (Exception e) { // extra precautions for web view because it's not done via image bug.
                Log.Error("Could not record Web view", e);
            }
        }

        private string GetReferral(ISubtextContext context)
        {
            var request = context.HttpContext.Request;
            Uri uri = UrlFormats.GetUriReferrerSafe(request);

            if (uri == null) {
                return null;
            }

            string referrerDomain = Blog.StripWwwPrefixFromHost(uri.Host);
            string blogDomain = Blog.StripWwwPrefixFromHost(context.UrlHelper.BlogUrl().ToFullyQualifiedUrl(context.Blog).Host);

            if (String.Equals(referrerDomain, blogDomain, StringComparison.OrdinalIgnoreCase)) {
                return null;
            }

            if (referrerDomain.Length == 0) {
                Log.Warn("Somehow the referral was an empty string and not null.");
                return null;
            }

            return uri.ToString();
        }
    }
}