using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAcl.Providers;

namespace NAcl.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
#if SILVERLIGHT
    public class SLFixtures
#else
    public class Fixtures
#endif
    {
#if SILVERLIGHT
    public SLFixtures()
#else
        public Fixtures()
#endif
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize() {
        //    AclManager.DefaultProvider = new MemoryACLSecurityProvider();
        //}
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ShouldWork()
        {
            IAclProvider categories = new MemoryProvider();
            IAclProvider widgets = new MemoryProvider();
            IAclProvider urls = new MemoryProvider();

            widgets.SetAcls(
                new Deny("/", "read", "*"),
                new Allow("/", "read", "g1")
            );
            urls.SetAcls(
                new Allow("/c", "read", "g2"),
                new Deny("/c", "read", "g3"),
                new Allow("/d", "read", "g3"),
                new Deny("/d", "read", "*"),
                new Deny("/", "read", "g2")
            );

            RouterProvider router = new RouterProvider();
            router.Register("/a", widgets);
            router.Register("/a/b", urls);
            AclManager.DefaultProvider = router;

            Assert.AreEqual(5, router.GetAcls("/a/b/c", "read").Count());
            Assert.AreEqual(5, router.GetAcls("/a/b/d", "read").Count());

            Assert.IsTrue(AclManager.IsAllowed("/a/b/c", "read", "g1", "g2"));
            Assert.IsFalse(AclManager.IsAllowed("/a/b/c", "read", "g1", "g3"));
            Assert.IsTrue(AclManager.IsAllowed("/a/b/d", "read", "g3"));

            Assert.IsTrue(AclManager.IsAllowed("/a/b/d", "read", "g1", "g3"));
            Assert.IsFalse(AclManager.IsAllowed("/a/b/d", "read", "g1", "g2"));
            Assert.IsFalse(AclManager.IsAllowed("/a/b/c", "read", "g3"));
        }

        [TestMethod]
        public void ShouldWorkWithRootRegisteredOnRouter()
        {
            IAclProvider categories = new MemoryProvider();
            IAclProvider widgets = new MemoryProvider();
            IAclProvider urls = new MemoryProvider();

            widgets.SetAcls(
                new Deny("/", "read", "*"),
                new Allow("/", "read", "g1")
            );
            urls.SetAcls(
                new Allow("/c", "read", "g2"),
                new Deny("/c", "read", "g3"),
                new Allow("/d", "read", "g3"),
                new Deny("/d", "read", "*"),
                new Deny("/", "read", "g2")
            );

            RouterProvider router = new RouterProvider();
            router.Register("/", widgets);
            router.Register("/b", urls);
            AclManager.DefaultProvider = router;

            Assert.AreEqual(5, router.GetAcls("/b/c", "read").Count());
            Assert.AreEqual(5, router.GetAcls("/b/d", "read").Count());

            Assert.IsTrue(AclManager.IsAllowed("/b/c", "read", "g1", "g2"));
            Assert.IsFalse(AclManager.IsAllowed("/b/c", "read", "g1", "g3"));
            Assert.IsTrue(AclManager.IsAllowed("/b/d", "read", "g3"));

            Assert.IsTrue(AclManager.IsAllowed("/b/d", "read", "g1", "g3"));
            Assert.IsFalse(AclManager.IsAllowed("/b/d", "read", "g1", "g2"));
            Assert.IsFalse(AclManager.IsAllowed("/b/c", "read", "g3"));
        }

        [TestMethod]
        public void ShouldDenyEveryoneWhenNoRule()
        {
            AclManager.DefaultProvider = new MemoryProvider();
            Assert.IsFalse(AclManager.IsAllowed("/", "read"));
            Assert.IsFalse(AclManager.IsAllowed("/", "read", "s.ros"));
        }

        [TestMethod]
        public void ShouldDenyNotAuthorized()
        {
            AclManager.DefaultProvider = new MemoryProvider();

            AclManager.Allow("/travel", "read", "s.ros");

            ShouldDenyNotAuthorizedByConfiguration();
        }

        [TestMethod]
        public void Bug()
        {
            AclManager.DefaultProvider = new MemoryProvider();
            AclManager.Allow("/", "read", "*");
            AclManager.Deny("/travel", "read", "s.ros");

            Assert.IsFalse(AclManager.IsAllowed("/travel", "read", "s.ros"));
            Assert.IsFalse(AclManager.IsAllowed("/travel/asshole", "read", "s.ros"));
            Assert.IsTrue(AclManager.IsAllowed("/", "read", "s.ros"));
            Assert.IsTrue(AclManager.IsAllowed("/", "read", "peter"));
            Assert.IsTrue(AclManager.IsAllowed("/travel", "read", "peter"));
            Assert.IsTrue(AclManager.IsAllowed("/travel/asshole", "read", "peter"));
        }

#if !SILVERLIGHT
        [TestMethod]
#endif
        public void ShouldDenyNotAuthorizedByConfiguration()
        {
            Assert.IsTrue(AclManager.IsAllowed("/travel", "read", "s.ros"));
            Assert.IsTrue(AclManager.IsAllowed("/travel/asshole", "read", "s.ros"));
            Assert.IsFalse(AclManager.IsAllowed("/", "read", "s.ros"));
            Assert.IsFalse(AclManager.IsAllowed("/", "read", "peter"));
            Assert.IsFalse(AclManager.IsAllowed("/travel", "read", "peter"));
            Assert.IsFalse(AclManager.IsAllowed("/travel/asshole", "read", "peter"));
        }

        [TestMethod]
        public void ShouldHandleStarVerb()
        {
            RouterProvider router = new RouterProvider();
            AclManager.DefaultProvider = router;
            IAclProvider urls = new MemoryProvider();
            router.Register("/Widget/Rss/Urls", urls);
            IAclProvider actions = new MemoryProvider();
            router.Register("/Widget", actions);
            AclManager.Allow("/Widget", "*", "*");
            AclManager.Deny("/Widget/Rss/Urls", "access", "~/Widgets/ClientRss/ClientRssWidget.ascx");
            AclManager.Allow("/Widget/Rss/Urls/fr/happly", "Access", "~/Widgets/ClientRss/ClientRssWidget.ascx");
            AclManager.Deny("/Widget/Rss/Urls/fr/happly/knowledgebank", "access", "~/Widgets/ClientRss/ClientRssWidget.ascx");

            Assert.IsFalse(AclManager.IsAllowed("/Widget/Rss/Urls", "access", "~/Widgets/ClientRss/ClientRssWidget.ascx"));
            Assert.IsFalse(AclManager.IsAllowed("/Widget/Rss/Urls/fr/happly/knowledgebank", "access", "~/Widgets/ClientRss/ClientRssWidget.ascx"));
            Assert.IsTrue(AclManager.IsAllowed("/Widget/Rss/Urls/fr/happly/knoledgebank", "access", "~/Widgets/ClientRss/ClientRssWidget.ascx"));
            Assert.IsTrue(AclManager.IsAllowed("/Widget", "read", "~/Widgets/ClientRss/ClientRssWidget.ascx"));
            Assert.IsTrue(AclManager.IsAllowed("/Widget/Rss/Urls/fr/happly/knowledgebank", "read", "~/Widgets/ClientRss/ClientRssWidget.ascx"));

        }

#if !SILVERLIGHT
        [TestMethod]
        public void SqlAclShouldWork()
        {
            AclManager.DefaultProvider = new SqlAclProvider();

            AclManager.Allow("/", "read", "*");
            AclManager.Deny("/travel", "read", "s.ros");

            Assert.IsFalse(AclManager.IsAllowed("/travel", "read", "s.ros"));
            Assert.IsFalse(AclManager.IsAllowed("/travel/asshole", "read", "s.ros"));
            Assert.IsTrue(AclManager.IsAllowed("/", "read", "s.ros"));
            Assert.IsTrue(AclManager.IsAllowed("/", "read", "peter"));
            Assert.IsTrue(AclManager.IsAllowed("/travel", "read", "peter"));
            Assert.IsTrue(AclManager.IsAllowed("/travel/asshole", "read", "peter"));
        }
#endif

        [TestMethod]
        public void ShouldNotifyOnAclRuleChange()
        {
            var router = new RouterProvider();
            AclManager.DefaultProvider = router;
            router.Register("/travel", new MemoryProvider());
            AclManager.RegisterForRuleChange("/travel", s => TestContext.WriteLine("'{0}' has changed", s));
            AclManager.AclChanged += new Action<string>(s => TestContext.WriteLine("* '{0}' has changed", s));
            AclManager.Allow("/", "read", "*");
            AclManager.Deny("/travel", "read", "s.ros");
            AclManager.Deny("/travel/asshole", "read", "s.ros");
        }
    }
}