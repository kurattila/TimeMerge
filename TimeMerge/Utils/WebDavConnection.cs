using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TimeMerge.Utils
{
    public interface IWebDavConnection
    {
        void Init(string userGuid, string login, string hashedPass, string urlWebDav);
        void Upload(string filename);
        string XmlAddressNoPassword();
        string XmlAddress();
        string GenericLogin();
        string GenericPassword();
    }

    public class WebDavConnection : IWebDavConnection
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(WebDavConnection));

        static string _key;
        static WebDavConnection()
        {
            System.Reflection.Assembly app = System.Reflection.Assembly.GetExecutingAssembly();
            var assemblyTitle = (System.Reflection.AssemblyTitleAttribute)app.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false)[0];
            _key = assemblyTitle.Title;
        }

        string _userGuid;
        string _genericLogin;
        string _hashedPass;
        string _urlWebDav;
        public void Init(string userGuid, string login, string hashedPass, string urlWebDav)
        {
            _userGuid = userGuid;
            _genericLogin = login;
            _hashedPass = hashedPass;
            _urlWebDav = urlWebDav;
        }

        public async void Upload(string filename)
        {
            // Basic authentication required
            var client = new WebDAVClient.Client(new NetworkCredential { UserName = GenericLogin(), Password = GenericPassword() });
            // Set basic information for WebDAV provider
            client.Server = "http://" + _urlWebDav;
            client.BasePath = "/";

            try
            {
                string workingCopy = Path.GetTempFileName();
                File.Copy(filename, workingCopy, true);
                await client.Upload("/", File.OpenRead(workingCopy), _userGuid + ".xml");
                File.Delete(workingCopy);
            }
            catch (Exception e)
            {
                logger.Error(e);
                logger.Error(e.Message);
            }

            //             await client.DeleteFile(filename);
            //             await client.Upload("/", File.OpenRead("TimeMerge_Data.css"), "TimeMerge_Data.css");
            //             await client.Upload("/", File.OpenRead("TimeMerge_Data.xsl"), "TimeMerge_Data.xsl");

            //             Stream x = await client.Download("toDelete.xml");
            //             var reader = new StreamReader(x);
            //             string contents = reader.ReadToEnd();
        }

        public string XmlAddress()
        {
            return string.Format("http://{0}:{1}@{2}/{3}.xml", GenericLogin(), GenericPassword(), _urlWebDav, _userGuid);
        }

        public string XmlAddressNoPassword()
        {
            return string.Format("http://{0}/{1}.xml", _urlWebDav, _userGuid);
        }

        public string GenericLogin()
        {
            return _genericLogin;
        }

        public virtual string GenericPassword()
        {
            var sb = new StringBuilder();
            int hashIndex = 0;
            int keyIndex = 0;
            while (hashIndex < _hashedPass.Length)
            {
                keyIndex %= WebDavConnection._key.Length;

                int hashedChar = int.Parse(_hashedPass.Substring(hashIndex, 2), System.Globalization.NumberStyles.HexNumber);
                char unhashedChar = Convert.ToChar(hashedChar ^ WebDavConnection._key[keyIndex]);
                sb.Append(unhashedChar);

                ++keyIndex;
                hashIndex += 2;
            }

            return sb.ToString();
        }

        public static string EncodeSecret(string plainText)
        {
            var sb = new StringBuilder();
            int keyIndex = 0;
            foreach (int c in plainText)
            {
                keyIndex %= WebDavConnection._key.Length;
                string hashedChar = string.Format("{0:X2}", c ^ WebDavConnection._key[keyIndex]);
                sb.Append(hashedChar);
                ++keyIndex;
            }
            return sb.ToString();
        }
    }
}
