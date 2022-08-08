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
            SqlConnString = "Data Source=192.168.1.240;Initial Catalog=longtech;Persist Security Info=True;User ID=sa;Password=80370265xyf;MultipleActiveResultSets=True;Pooling=true;Max Pool Size=40000;Min Pool Size=0;Application Name=worktime";
#else
            SqlConnString = "Data Source=192.168.1.238;Initial Catalog=longtech;Persist Security Info=True;User ID=sa;Password=80370265xyf;MultipleActiveResultSets=True;Pooling=true;Max Pool Size=40000;Min Pool Size=0;Application Name=worktime";
#endif
            //try
            //{
            //    Client.CodeFirst.InitTables<DeviceTimes>();
            //    Client.CodeFirst.InitTables<employee>();
            //    Client.CodeFirst.InitTables<QTSJ>();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
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
            DbType = DbType.SqlServer,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.SystemTable,
            ConfigureExternalServices = new ConfigureExternalServices()
            {
                DataInfoCacheService = new HttpRuntimeCache()
            }
        });

        public SqlSugarClient MySqlClinet => new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = "Server=192.168.1.250;Database=longtech;Uid=root;Pwd=223081080",
            DbType = DbType.MySql,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
            ConfigureExternalServices = new ConfigureExternalServices()
            {
                DataInfoCacheService = new HttpRuntimeCache()
            }
        });

    }
}
