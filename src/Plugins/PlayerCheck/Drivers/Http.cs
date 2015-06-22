using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace PlayerCheck.Drivers
{
    public class Http : Base, IDriver
    {
        private string _url;
        new public void SetConfig(DriverSettings settings)
        {
            base.SetConfig(settings);
            _url = settings.Ini.GetSetting("Http", "Url").Trim();
            if (String.IsNullOrWhiteSpace(_url))
                throw new DriverException("URL is empty");
        }

        public bool CheckPlayer(Player player)
        {
            byte[] res;

            using (var client = new WebClient())
            {
                var reqparm = new NameValueCollection {{"name", player.Name}, {"data", player.Data}};
                res = client.UploadValues(_url, "POST", reqparm);
            }
            
            var str = Encoding.Default.GetString(res);
            return (str == "1");
        }
    }
}
