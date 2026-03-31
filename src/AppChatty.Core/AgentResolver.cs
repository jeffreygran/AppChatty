using System;
using System.Collections.Generic;

namespace AppChatty
{
    /// <summary>
    /// Describes a single M365 Copilot agent entry.
    /// </summary>
    public sealed class AgentEntry
    {
        /// <summary>
        /// Lower-case substring used to match against the active process name.
        /// An empty string denotes the default/fallback entry.
        /// </summary>
        public string MatchKey { get; }

        /// <summary>Friendly display name shown in the panel banner.</summary>
        public string Label { get; }

        /// <summary>M365 Copilot agent URL to load in the WebView2.</summary>
        public string Url { get; }

        /// <summary>
        /// <see langword="true"/> when this entry is the fallback/default
        /// (i.e. no application-specific agent was matched).
        /// </summary>
        public bool IsDefault => string.IsNullOrEmpty(MatchKey);

        public AgentEntry(string matchKey, string label, string url)
        {
            MatchKey = matchKey ?? string.Empty;
            Label    = label   ?? string.Empty;
            Url      = url     ?? string.Empty;
        }
    }

    /// <summary>
    /// Maps a foreground-application process name to the corresponding
    /// M365 Copilot agent URL using a first-match strategy.
    /// </summary>
    public static class AgentResolver
    {
        /// <summary>
        /// Ordered list of agent entries.  Entries are matched in order;
        /// the first entry whose <see cref="AgentEntry.MatchKey"/> is
        /// contained in the (lower-cased) process name wins.
        /// </summary>
        private static readonly IReadOnlyList<AgentEntry> AgentMap =
            new List<AgentEntry>
            {
                new AgentEntry("excel",       "Microsoft Excel",       "https://m365.cloud.microsoft/chat?app=excel"),
                new AgentEntry("winword",     "Microsoft Word",        "https://m365.cloud.microsoft/chat?app=word"),
                new AgentEntry("word",        "Microsoft Word",        "https://m365.cloud.microsoft/chat?app=word"),
                new AgentEntry("powerpnt",    "Microsoft PowerPoint",  "https://m365.cloud.microsoft/chat?app=powerpoint"),
                new AgentEntry("powerpoint",  "Microsoft PowerPoint",  "https://m365.cloud.microsoft/chat?app=powerpoint"),
                new AgentEntry("outlook",     "Microsoft Outlook",     "https://m365.cloud.microsoft/chat?app=outlook"),
                new AgentEntry("teams",       "Microsoft Teams",       "https://m365.cloud.microsoft/chat?app=teams"),
                new AgentEntry("onenote",     "Microsoft OneNote",     "https://m365.cloud.microsoft/chat?app=onenote"),
                new AgentEntry("sharepoint",  "SharePoint",            "https://m365.cloud.microsoft/chat?app=sharepoint"),
                new AgentEntry("dynamics",    "Microsoft Dynamics",    "https://m365.cloud.microsoft/chat?app=dynamics365"),
                new AgentEntry("sap",         "SAP",                   "https://m365.cloud.microsoft/chat?app=sap"),
                new AgentEntry("oracle",      "Oracle",                "https://m365.cloud.microsoft/chat?app=oracle"),
                new AgentEntry("salesforce",  "Salesforce",            "https://m365.cloud.microsoft/chat?app=salesforce"),
            };

        /// <summary>
        /// The fallback entry used when no specific agent is matched.
        /// </summary>
        public static readonly AgentEntry Default =
            new AgentEntry(string.Empty, "Your Application", "https://m365.cloud.microsoft/chat");

        /// <summary>
        /// Returns the best-matching <see cref="AgentEntry"/> for the given
        /// process name, or <see cref="Default"/> when nothing matches.
        /// </summary>
        /// <param name="processName">
        /// The active foreground application's process name (e.g. "EXCEL" or
        /// "EXCEL.EXE").  Case-insensitive.
        /// </param>
        public static AgentEntry Resolve(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return Default;

            string lower = processName.ToLowerInvariant();

            foreach (var entry in AgentMap)
            {
                if (!string.IsNullOrEmpty(entry.MatchKey) && lower.Contains(entry.MatchKey))
                    return entry;
            }

            return Default;
        }
    }
}
