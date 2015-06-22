using System.Collections.Generic;

namespace Proxy.DB
{
    public class Result
    {
        public bool Error = false;
        public bool NoResult = false;
        public List<Dictionary<string, string>> Rows = new List<Dictionary<string, string>>();
    }
}
