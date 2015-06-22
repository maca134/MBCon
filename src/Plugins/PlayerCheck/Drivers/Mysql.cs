using System;
using Proxy.DB;

namespace PlayerCheck.Drivers
{
    public class Mysql : Base, IDriver
    {
        private const string _sql = "SELECT COUNT(*) as `Total` FROM {0} WHERE `{1}` = @data";

        public bool CheckPlayer(Player player)
        {
            var query = new Query()
            {
                Conn = new Connection()
                {
                    Host = _settings.Ini.GetSetting("Mysql", "Host"),
                    Port = _settings.Ini.GetSetting("Mysql", "Port"),
                    User = _settings.Ini.GetSetting("Mysql", "User"),
                    Password = _settings.Ini.GetSetting("Mysql", "Password"),
                    Database = _settings.Ini.GetSetting("Mysql", "Database")
                },
                QueryType = Query.Type.Select,
                Sql = String.Format(_sql, _settings.Ini.GetSetting("Mysql", "Table"), _settings.Ini.GetSetting("Mysql", "Field"))
            };
            query.Params.Add("@data", player.Data);
            var result = _settings.Api.Db.Execute(query);
            return result.Rows[0]["Total"] != "0";
        }
    }
}
