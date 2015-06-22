using System;
using System.Collections.Generic;

namespace Proxy.DB
{
    public class Query
    {
        public enum Type
        {
            Select,
            Insert,
            Update,
            Delete,
            NotSet
        }

        private Connection _conn;
        public Connection Conn
        {
            get
            {
                return _conn;
            }
            set
            {
                if (_conn == null)
                    _conn = value;
            }
        }

        private Type _queryType = Type.NotSet;
        public Type QueryType
        {
            get
            {
                return _queryType;
            }
            set
            {
                if (_queryType == Type.NotSet)
                    _queryType = value;
            }

        }

        private String _sql;
        public String Sql
        {
            get
            {
                return _sql;
            }
            set
            {
                if (_sql == null)
                    _sql = value;
            }
        }

        private Dictionary<string, string> _params = new Dictionary<string, string>();
        public Dictionary<string, string> Params
        {
            get
            {
                return _params;
            }
            set
            {
                _params = value;
            }
        }

        private List<Dictionary<string, string>> _results;
        public List<Dictionary<string, string>> Results
        {
            get
            {
                return _results;
            }
            set
            {
                if (_results == null)
                    _results = value;
            }
        }
    }
}
