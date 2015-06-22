using System.ComponentModel;
using System.IO;

namespace Proxy
{
    public class Utils
    {
        public static string GetEnumDescription(object enumValue, string defDesc)
        {
            var fi = enumValue.GetType().GetField(enumValue.ToString());

            if (null != fi)
            {
                var attrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }

            return defDesc;
        }

        public static string GetRandomString()
        {
            var path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path;
        }
    }
}
