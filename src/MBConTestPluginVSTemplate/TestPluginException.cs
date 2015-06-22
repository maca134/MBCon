using Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace $safeprojectname$
{
    public class $safeprojectname$Exception : IPluginException
    {
        public $safeprojectname$Exception()
        {
        }

        public $safeprojectname$Exception(string message)
            : base(message)
        {
        }

        public $safeprojectname$Exception(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
