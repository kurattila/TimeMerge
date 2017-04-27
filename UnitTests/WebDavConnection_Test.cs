using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeMerge.Utils;
using Moq;

namespace UnitTests
{
    [TestClass]
    public class WebDavConnection_Test
    {
        [TestMethod]
        public void Password_Encoded()
        {
            string hashed = WebDavConnection.EncodeSecret("secre7Passw0rd");

            Assert.AreEqual("270C0E172852220616271E5D1729", hashed);
        }

        [TestMethod]
        public void Password_Decoded()
        {
            var webDavConn = new WebDavConnection();
            string dummyUserId = Guid.Empty.ToString();
            string dummyUrlWebDav = "";

            webDavConn.Init(dummyUserId, "dummyLogin", "270C0E172852220616271E5D1729", dummyUrlWebDav);

            Assert.AreEqual("secre7Passw0rd", webDavConn.GenericPassword());
        }

        [TestMethod]
        public void WebDavXml_Address_WithLoginData()
        {
            var webDavConn = new Mock<WebDavConnection>();
            webDavConn.Setup(c => c.GenericPassword()).Returns("dummyPassword");
            webDavConn.Object.Init(Guid.Empty.ToString(), "dummyLogin", "someHashedPassword", "someUrl");

            string address = webDavConn.Object.XmlAddress();

            Assert.AreEqual("http://dummyLogin:dummyPassword@someUrl/00000000-0000-0000-0000-000000000000.xml", address);
        }

        [TestMethod]
        public void WebDavXml_Address_NoLoginData()
        {
            var webDavConn = new Mock<WebDavConnection>();
            webDavConn.Setup(c => c.GenericPassword()).Returns("dummyPassword");
            webDavConn.Object.Init(Guid.Empty.ToString(), "dummyLogin", "someHashedPassword", "someUrl");

            string address = webDavConn.Object.XmlAddressNoPassword();

            Assert.AreEqual("http://someUrl/00000000-0000-0000-0000-000000000000.xml", address);
        }
    }
}
