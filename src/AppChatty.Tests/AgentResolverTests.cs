using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppChatty.Tests
{
    /// <summary>
    /// Unit tests for <see cref="AgentResolver"/>.
    /// These tests are pure logic and have no UI/OS dependencies.
    /// </summary>
    [TestClass]
    public class AgentResolverTests
    {
        // ── M365 app mappings ──────────────────────────────────────────────

        [TestMethod]
        public void Resolve_ExcelExe_ReturnsExcelAgent()
        {
            var agent = AgentResolver.Resolve("EXCEL.EXE");
            Assert.AreEqual("https://m365.cloud.microsoft/chat?app=excel", agent.Url);
            Assert.AreEqual("Microsoft Excel", agent.Label);
        }

        [TestMethod]
        public void Resolve_WinwordExe_ReturnsWordAgent()
        {
            var agent = AgentResolver.Resolve("WINWORD.EXE");
            Assert.AreEqual("https://m365.cloud.microsoft/chat?app=word", agent.Url);
            Assert.AreEqual("Microsoft Word", agent.Label);
        }

        [TestMethod]
        public void Resolve_PowerpntExe_ReturnsPowerPointAgent()
        {
            var agent = AgentResolver.Resolve("POWERPNT.EXE");
            Assert.AreEqual("https://m365.cloud.microsoft/chat?app=powerpoint", agent.Url);
        }

        [TestMethod]
        public void Resolve_OutlookExe_ReturnsOutlookAgent()
        {
            var agent = AgentResolver.Resolve("OUTLOOK.EXE");
            Assert.AreEqual("https://m365.cloud.microsoft/chat?app=outlook", agent.Url);
        }

        [TestMethod]
        public void Resolve_Teams_ReturnsTeamsAgent()
        {
            var agent = AgentResolver.Resolve("Teams");
            Assert.AreEqual("https://m365.cloud.microsoft/chat?app=teams", agent.Url);
        }

        [TestMethod]
        public void Resolve_OneNoteExe_ReturnsOneNoteAgent()
        {
            var agent = AgentResolver.Resolve("ONENOTE.EXE");
            Assert.AreEqual("https://m365.cloud.microsoft/chat?app=onenote", agent.Url);
        }

        [TestMethod]
        public void Resolve_SAPLogon_ReturnsSapAgent()
        {
            var agent = AgentResolver.Resolve("SAPLogon");
            Assert.AreEqual("https://m365.cloud.microsoft/chat?app=sap", agent.Url);
        }

        [TestMethod]
        public void Resolve_Salesforce_ReturnsSalesforceAgent()
        {
            var agent = AgentResolver.Resolve("Salesforce");
            Assert.AreEqual("https://m365.cloud.microsoft/chat?app=salesforce", agent.Url);
        }

        [TestMethod]
        public void Resolve_OracleForms_ReturnsOracleAgent()
        {
            var agent = AgentResolver.Resolve("Oracle Forms");
            Assert.AreEqual("https://m365.cloud.microsoft/chat?app=oracle", agent.Url);
        }

        // ── Default / fallback ─────────────────────────────────────────────

        [TestMethod]
        public void Resolve_UnknownApp_ReturnsDefaultAgent()
        {
            var agent = AgentResolver.Resolve("notepad.exe");
            Assert.AreEqual("https://m365.cloud.microsoft/chat", agent.Url);
            Assert.AreEqual("Your Application", agent.Label);
        }

        [TestMethod]
        public void Resolve_EmptyString_ReturnsDefaultAgent()
        {
            var agent = AgentResolver.Resolve(string.Empty);
            Assert.AreEqual(AgentResolver.Default.Url, agent.Url);
        }

        [TestMethod]
        public void Resolve_NullString_ReturnsDefaultAgent()
        {
            var agent = AgentResolver.Resolve(null);
            Assert.AreEqual(AgentResolver.Default.Url, agent.Url);
        }

        [TestMethod]
        public void Resolve_WhitespaceString_ReturnsDefaultAgent()
        {
            var agent = AgentResolver.Resolve("   ");
            Assert.AreEqual(AgentResolver.Default.Url, agent.Url);
        }

        // ── Case-insensitivity ─────────────────────────────────────────────

        [TestMethod]
        public void Resolve_IsCaseInsensitive()
        {
            string urlUpper = AgentResolver.Resolve("EXCEL").Url;
            string urlLower = AgentResolver.Resolve("excel").Url;
            string urlMixed = AgentResolver.Resolve("Excel").Url;

            Assert.AreEqual(urlUpper, urlLower);
            Assert.AreEqual(urlUpper, urlMixed);
        }

        // ── Default sentinel ───────────────────────────────────────────────

        [TestMethod]
        public void Default_HasEmptyMatchKey()
        {
            Assert.AreEqual(string.Empty, AgentResolver.Default.MatchKey);
        }

        [TestMethod]
        public void Default_HasExpectedUrl()
        {
            Assert.AreEqual("https://m365.cloud.microsoft/chat", AgentResolver.Default.Url);
        }

        // ── AgentEntry construction ────────────────────────────────────────

        [TestMethod]
        public void AgentEntry_NullArguments_AreTreatedAsEmpty()
        {
            var entry = new AgentEntry(null, null, null);
            Assert.AreEqual(string.Empty, entry.MatchKey);
            Assert.AreEqual(string.Empty, entry.Label);
            Assert.AreEqual(string.Empty, entry.Url);
        }

        [TestMethod]
        public void AgentEntry_StoresPropertiesCorrectly()
        {
            var entry = new AgentEntry("excel", "Microsoft Excel",
                "https://m365.cloud.microsoft/chat?app=excel");

            Assert.AreEqual("excel",                  entry.MatchKey);
            Assert.AreEqual("Microsoft Excel",        entry.Label);
            Assert.AreEqual("https://m365.cloud.microsoft/chat?app=excel", entry.Url);
        }
    }
}
