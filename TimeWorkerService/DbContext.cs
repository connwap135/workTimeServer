using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace TimeWorkerService
{
    public class DbContext
    {
        public DbContext()
        {
#if DEBUG
            SqlConnString = "Server=192.168.1.250;Database=longtech;Uid=root;Pwd=223081080";
#else
            SqlConnString = "Server=localhost;Database=longtech;Uid=root;Pwd=223081080";
#endif
            try
            {
                Client.CodeFirst.InitTables<DeviceTimes>();
                Client.CodeFirst.InitTables<employee>();
                Client.CodeFirst.InitTables<QTSJ>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
        private static readonly DbContext instance = new DbContext();
        private readonly string SqlConnString;

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
            DbType = DbType.MySql,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
            IsShardSameThread = false,
            ConfigureExternalServices = new ConfigureExternalServices()
            {
                DataInfoCacheService = new HttpRuntimeCache()
            }
        });

        /// <summary>
        /// SqlServer客户端
        /// </summary>
        public SqlSugarClient MsSQLClient=> new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = "Data Source=192.168.1.238;Initial Catalog=longtech;Persist Security Info=True;User ID=sa;Password=80370265xyf;MultipleActiveResultSets=True;Pooling=true;Max Pool Size=40000;Min Pool Size=0;Application Name=TimeWorkerService",
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
