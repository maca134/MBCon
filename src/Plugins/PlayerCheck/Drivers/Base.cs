using System;
using Proxy;

namespace PlayerCheck.Drivers
{
    abstract public class Base
    {
        public static IDriver GetDriver(IniParser ini)
        {
            var driverName = ini.GetSetting("PlayerCheck", "Driver");
            if (driverName == "")
            {
                throw new DriverException("Driver not set");
            }

            try
            {
                var iDriverType = typeof(IDriver);
                var driverType = Type.GetType(String.Format("PlayerCheck.Drivers.{0}", driverName));

                if (driverType.IsInterface || driverType.IsAbstract)
                    throw new DriverException("Driver is interface or abstract");

                if (driverType.GetInterface(iDriverType.FullName) == null)
                    throw new DriverException("Driver doesnt implement IDriver");

                return (IDriver)Activator.CreateInstance(driverType);
            }
            catch (Exception ex)
            {
                throw new DriverException(String.Format("Error loading driver {0}: {1}", driverName, ex.Message));
            }
        }

        protected DriverSettings _settings;
        public void SetConfig(DriverSettings settings)
        {
            _settings = settings;
        }
    }
}
