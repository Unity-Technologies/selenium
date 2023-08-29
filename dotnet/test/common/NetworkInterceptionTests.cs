using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium.DevTools;

namespace OpenQA.Selenium
{
    [TestFixture]
    public class NetworkInterceptionTests : DriverTestFixture
    {
        [Test]
        [IgnoreBrowser(Browser.Firefox, "Firefox does not implement the CDP Fetch domain required for network interception")]
        public async Task TestCanInterceptNetworkCalls()
        {
            if (driver is IDevTools)
            {
                INetwork network = driver.Manage().Network;
                NetworkResponseHandler handler = new NetworkResponseHandler();
                handler.ResponseMatcher = (responseData) => responseData.Url.Contains("simpleTest.html");
                handler.ResponseTransformer = (responseData) =>
                {
                    responseData.Body = "<html><body><p>I intercepted you</p></body></html>";
                    return responseData;
                };
                network.AddResponseHandler(handler);
                await network.StartMonitoring();
                driver.Url = simpleTestPage;
                string text = driver.FindElement(By.CssSelector("p")).Text;
                await network.StopMonitoring();
                Assert.AreEqual("I intercepted you", text);
            }
        }

        [Test]
        [IgnoreBrowser(Browser.Firefox, "Firefox does not implement the CDP Fetch domain required for network interception")]
        public async Task TestCanUseAuthorizationHandler()
        {
            if (driver is IDevTools)
            {
                INetwork network = driver.Manage().Network;
                NetworkAuthenticationHandler handler = new NetworkAuthenticationHandler()
                {
                    UriMatcher = (uri) => uri.PathAndQuery.Contains("basicAuth"),
                    Credentials = new PasswordCredentials("test", "test")
                };
                network.AddAuthenticationHandler(handler);
                await network.StartMonitoring();
                driver.Url = authenticationPage;
                string text = driver.FindElement(By.CssSelector("h1")).Text;
                await network.StopMonitoring();
                Assert.AreEqual("authorized", text);
            }
        }
    }
}
