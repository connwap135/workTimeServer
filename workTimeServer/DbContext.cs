using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace workTimeServer
{
    public class DbContext
    {
        public DbContext()
        {
#if DEBUG
            SqlConnString = "Data Source=192.168.1.240;Initial Catalog=longtech;Persist Security Info=True;User ID=sa;Password=80370265xyf;MultipleActiveResultSets=True;Pooling=true;Max Pool Size=40000;Min Pool Size=0;Application Name=webApp2";
#else
            SqlConnString = "Data Source=192.168.1.238;Initial Catalog=longtech;Persist Security Info=True;User ID=sa;Password=80370265xyf;MultipleActiveResultSets=True;Pooling=true;Max Pool Size=40000;Min Pool Size=0;Application Name=webApp2";
#endif

        }
        private static readonly DbContext instance = new DbContext();
        private string SqlConnString;

        public static DbContext Instance
        {
            get
            {
                return instance;
            }
        }
        public SqlSugarClient Client => new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = SqlConnString,
            DbType = DbType.SqlServer,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.SystemTable,
            IsShardSameThread = false,
            ConfigureExternalServices = new ConfigureExternalServices()
            {
                DataInfoCacheService = new HttpRuntimeCache()
            }
        });

       
    }
}
