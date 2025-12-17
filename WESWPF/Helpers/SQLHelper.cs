using FreeSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Helpers
{
    public class SQLHelper
    {
        private static SQLHelper _instance;// = GetInstance();
        public static SQLHelper Instance => _instance;
        private static readonly object _lock = new object();
        private readonly IFreeSql _freeSql;

        // 私有构造函数，防止在类外被实例化
        private SQLHelper(string connectionString)
        {
            // 初始化 FreeSql 的 PostgreSQL 数据库连接
            _freeSql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, connectionString)
                .UseAutoSyncStructure(true) // 自动同步实体结构到数据库
                .Build();
        }

        static SQLHelper()
        {
            _instance = GetInstance();
        }

        // 单例实例获取方法
        public static SQLHelper GetInstance(string connectionString = @"Data Source = MyDataBase.db")
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SQLHelper(connectionString);
                    }
                }
            }
            return _instance;
        }

        // 创建实体
        public async Task<int> InsertAsync<T>(T entity) where T : class
        {
            return await _freeSql.Insert(entity).ExecuteAffrowsAsync();
        }

        // 批量创建实体
        public async Task<int> InsertRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            return await _freeSql.Insert(entities).ExecuteAffrowsAsync();
        }

        // 查询实体
        public async Task<List<T>> SelectAsync<T>() where T : class
        {
            return await _freeSql.Select<T>().ToListAsync();
        }

        // 根据条件查询实体
        public async Task<List<T>> SelectAsync<T>(Func<ISelect<T>, ISelect<T>> conditions) where T : class
        {
            return await conditions(_freeSql.Select<T>()).ToListAsync();
        }

        // 更新实体
        public async Task<int> UpdateAsync<T>(T entity) where T : class
        {
            return await _freeSql.Update<T>().SetSource(entity).ExecuteAffrowsAsync();
        }

        // 根据条件更新实体
        public async Task<int> UpdateAsync<T>(Func<IUpdate<T>, IUpdate<T>> updateAction) where T : class
        {
            return await updateAction(_freeSql.Update<T>()).ExecuteAffrowsAsync();
        }

        // 删除实体
        public async Task<int> DeleteAsync<T>(T entity) where T : class
        {
            return await _freeSql.Delete<T>().Where(entity).ExecuteAffrowsAsync();
        }

        // 根据条件删除实体
        public async Task<int> DeleteAsync<T>(Func<IDelete<T>, IDelete<T>> whereConditions) where T : class
        {
            return await whereConditions(_freeSql.Delete<T>()).ExecuteAffrowsAsync();
        }
    }
}
