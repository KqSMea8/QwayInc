using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class SQLResultStatusInfo
    {
        public String Code { get; set; } = String.Empty;
        public Int32 Id { get; set; } = -1;
        public String Status { get; set; } = String.Empty;
    }
    public class WebsiteInfo
    {
        private static List<String> _IgnoredList = getIgnoredList();

        public String Code { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public String ParentUrl { get; set; } = String.Empty;
        public String Url { get; set; } = String.Empty;
        public String Metadata { get; set; } = String.Empty;
        public String WebsiteUrl { get { return getWebsiteUrl(); } }
        public Boolean Active { get; set; } = true;
        public Boolean IsIgnored { get { return getIsIgnored(); } }
        public Int32 Id { get; set; } = -1;
        public Int32 DumpLevel { get; set; } = -1;
        public Int32 ParentId { get; set; } = -1;
        public DateTime AddedDate { get; set; } = new DateTime(1800, 1, 1);
        public DateTime UpdatedDate { get; set; } = new DateTime(1800, 1, 1);
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        public WebsiteInfo() { }
        public WebsiteInfo(String[] args)
        {
            if (args != null && args.Length >= 3)
            {
                this.Code = args[0];
                this.Url = args[1];
                this.DumpLevel = Convert.ToInt32(args[2]);
                this.Name = "Root";
            }
        }
        public WebsiteInfo(WebsiteInfo website)
        {
            if (website != null)
            {
                this.Code = website.Code;
                this.ParentUrl = website.WebsiteUrl;
                this.DumpLevel = website.DumpLevel - 1;
            }
        }

        private static List<string> getIgnoredList()
        {
            return new List<String>() {
                ".google.com",
                ".googleapis.com",
                ".googleblog.com",
                ".googletagmanager.com",
                ".googleblog.com"
            };
        }

        private bool getIsIgnored()
        {
            Boolean success = true;
            if (!String.IsNullOrEmpty(this.Url) && !this.Url.StartsWith("/"))
            {
                if (!this.Url.Contains(".google"))
                    success = _IgnoredList.Where(item => this.Url.Contains(item)).Count() > 0;
            }
            return success;
        }

        private String getWebsiteUrl()
        {
            String url = String.Empty;
            try
            {
                String orgUrl = this.Url.Replace(@"\", "").Replace("\"", "").Replace("'", "");
                if (!orgUrl.ToLower().StartsWith("http://")
                    && !orgUrl.ToLower().StartsWith("https://")
                    && !String.IsNullOrEmpty(this.ParentUrl))
                {
                    String[] list = this.ParentUrl.Split(':');
                    if (list.Length > 1)
                        orgUrl = String.Format("{0}://{1}", list[0], orgUrl.Replace("//", ""));
                }
                System.Uri uri = new Uri(orgUrl);
                url = uri.AbsoluteUri;
            }
            catch (System.Exception)
            {
            }

            return url;
        }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Code",this.Code),
                new KeyValuePair<String, Object>("@Active",this.Active),
                new KeyValuePair<String, Object>("@Name",this.Name),
                new KeyValuePair<String, Object>("@Url",this.Url),
                new KeyValuePair<String, Object>("@WebsiteUrl",this.WebsiteUrl),
                new KeyValuePair<String, Object>("@ParentUrl",this.ParentUrl),
                new KeyValuePair<String, Object>("@ParentId",this.ParentId),
                new KeyValuePair<String, Object>("@Metadata",this.Metadata)
            };
            return parameters;
        }
    }
    public class WebDriverSettingInfo
    {
        public Boolean HideError { get; set; } = true;
        public Boolean HideBrowser { get; set; } = false;
        public Boolean HideCommand { get; set; } = false;
        public WebDriverSettingInfo() { }
        public WebDriverSettingInfo(Boolean hideCommand, Boolean hideBrowser, Boolean hideError = true) : this()
        {
            this.HideBrowser = hideBrowser;
            this.HideCommand = hideCommand;
            this.HideError = hideError;
        }
    }
}
