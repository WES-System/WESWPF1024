using Common;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WES.Models;

namespace WES.Helpers
{
    public class PGSQLHelper : IDisposable
    {
        private static PGSQLHelper _instance;// = GetInstance();
        public static PGSQLHelper Instance => _instance ?? GetInstance();
        private static readonly object _lock = new object();
        private readonly IFreeSql _freeSql;
        private bool _disposed = false;
        // 私有构造函数，防止在类外被实例化
        private PGSQLHelper(string connectionString)
        {
            // 初始化 FreeSql 的 PostgreSQL 数据库连接
            _freeSql = new FreeSqlBuilder()
                .UseConnectionString(DataType.PostgreSQL, connectionString)
                .UseAutoSyncStructure(true) // 自动同步实体结构到数据库
                .UseNoneCommandParameter(true)
                .Build();
            _freeSql.CodeFirst.SyncStructure<FloorCode>();
            _freeSql.CodeFirst.SyncStructure<ShelfLocation>();
            _freeSql.CodeFirst.SyncStructure<RoboticArm>();
            _freeSql.CodeFirst.SyncStructure<Material>();
            _freeSql.CodeFirst.SyncStructure<OrderTable>();
            _freeSql.CodeFirst.SyncStructure<RuninShelf>();
            _freeSql.CodeFirst.SyncStructure<InboundRecord>();
            _freeSql.CodeFirst.SyncStructure<OutboundRecord>();
            // 全局过滤配置（可根据需要开启）
            //_freeSql.GlobalFilter.Apply<ISoftDelete>("IsDeleted", a => a.IsDeleted == false);
        }

        static PGSQLHelper()
        {
            _instance = GetInstance();
        }

        // 单例实例获取方法
        public static PGSQLHelper GetInstance(string connectionString = @"Host=localhost;Port=5432;Username=postgres;Password=QWER0987654321;Database=Runin;Pooling=true;Minimum Pool Size=1")
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PGSQLHelper(connectionString);
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

        #region 插入操作
        /// <summary>
        /// 插入单条数据
        /// </summary>
        public async Task<OpResult<long>> InsertWithResultAsync<T>(T entity) where T : class
        {
            var result = new OpResult<long> { IsSuccess = true };
            if (entity == null)
            {
                result.IsSuccess = false;
                result.Message = "插入实体不能为空";
                return result;
            }

            try
            {
                var id = await _freeSql.Insert(entity).ExecuteIdentityAsync();
                result.Any1 = id;
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error("InsertWithResultAsync失败", ex);
                return HandleException<long>(ex, "插入数据失败");
            }
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        public async Task<OpResult<int>> InsertRangeWithResultAsync<T>(IEnumerable<T> entities) where T : class
        {
            var result = new OpResult<int> { IsSuccess = true };
            if (entities == null || !entities.Any())
            {
                result.IsSuccess = false;
                result.Message = "插入集合不能为空";
                return result;
            }

            try
            {
                var count = await _freeSql.Insert(entities).ExecuteAffrowsAsync();
                result.Any1 = count;
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error("InsertRangeWithResultAsync失败", ex);
                return HandleException<int>(ex, "批量插入失败");
            }
        }
        #endregion

        #region 查询操作

        /// <summary>
        /// 查询所有数据
        /// </summary>
        public async Task<OpResult<List<T>>> SelectWithResultAsync<T>() where T : class
        {
            var result = new OpResult<List<T>> { IsSuccess = true };
            try
            {
                result.Any1 = await _freeSql.Select<T>().ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error("SelectWithResultAsync失败", ex);
                return HandleException<List<T>>(ex, "查询数据失败");
            }
        }

        /// <summary>
        /// 条件查询
        /// </summary>
        public async Task<OpResult<List<T>>> SelectWithResultAsync<T>(Expression<Func<T, bool>> where = null) where T : class
        {
            var result = new OpResult<List<T>> { IsSuccess = true };
            try
            {
                var query = _freeSql.Select<T>();
                if (where != null) query = query.Where(where);

                var data = await query.ToListAsync();
                result.Any1 = data;
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error("SelectWithResultAsync失败", ex);
                return HandleException<List<T>>(ex, "查询数据失败");
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public async Task<OpResult<PageResult<T>>> SelectPageWithResultAsync<T>(
            Expression<Func<T, bool>> where = null,
            int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<T, object>> orderBy = null) where T : class
        {
            var result = new OpResult<PageResult<T>> { IsSuccess = true };
            try
            {
                var query = _freeSql.Select<T>();
                if (where != null) query = query.Where(where);
                var totalCount = await query.CountAsync();

                if (orderBy != null) query = query.OrderBy(orderBy);
                var data = await query.Page(pageIndex, pageSize).ToListAsync();

                return new OpResult<PageResult<T>>
                {
                    IsSuccess = true,
                    Any1 = new PageResult<T>
                    {
                        List = data,
                        TotalCount = totalCount,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        PageCount = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };
            }
            catch (Exception ex)
            {
                LogHelper.Error("SelectPageWithResultAsync失败", ex);
                return HandleException<PageResult<T>>(ex, "分页查询失败");
            }
        }


        /// <summary>
        /// 检查是否存在
        /// </summary>
        public async Task<OpResult<bool>> ExistsWithResultAsync<T>(Expression<Func<T, bool>> where) where T : class
        {

            var result = new OpResult<bool> { IsSuccess = true };
            try
            {
                var exists = await _freeSql.Select<T>().Where(where).AnyAsync();
                result.Any1 = exists;
                return result;
                }
            catch (Exception ex)
            {
                return HandleException<bool>(ex, "检查存在性失败");
            }
        }
        #endregion

        #region 更新操作
        /// <summary>
        /// 更新单条数据
        /// </summary>
        public async Task<OpResult<int>> UpdateWithResultAsync<T>(T entity, Expression<Func<T, object>> ignoreColumns = null) where T : class
        {
            var result = new OpResult<int> { IsSuccess = true };
            if (entity == null)
            {
                result.IsSuccess = false;
                result.Message = "更新实体不能为空";
                return result;
            }

            try
            {
                var update = _freeSql.Update<T>().SetSource(entity);
                if (ignoreColumns != null) update = update.IgnoreColumns(ignoreColumns);

                var count = await update.ExecuteAffrowsAsync();
                result.Any1 = count;
                return result;
            }
            catch (Exception ex)
            {
                return HandleException<int>(ex, "更新数据失败");
            }
        }

        /// <summary>
        /// 条件更新
        /// </summary>
        public async Task<OpResult<int>> UpdateWithResultAsync<T>(
            Expression<Func<T, T>> set,
            Expression<Func<T, bool>> where) where T : class
        {
            try
            {
                var count = await _freeSql.Update<T>()
                    .Set(set)
                    .Where(where)
                    .ExecuteAffrowsAsync();
                return new OpResult<int> { IsSuccess = true, Any1 = count };
            }
            catch (Exception ex)
            {
                return HandleException<int>(ex, "条件更新失败");
            }
        }
        #endregion

        #region 删除操作
        /// <summary>
        /// 删除单条数据
        /// </summary>
        public async Task<OpResult<int>> DeleteWithResultAsync<T>(T entity) where T : class
        {
            var result = new OpResult<int> { IsSuccess = true };
            if (entity == null)
            {
                result.IsSuccess = false;
                result.Message = "删除实体不能为空";
                return result;
            }

            try
            {
                var count = await _freeSql.Delete<T>().Where(entity).ExecuteAffrowsAsync();
                result.Any1 = count;
                return result;
            }
            catch (Exception ex)
            {
                return HandleException<int>(ex, "删除数据失败");
            }
        }

        /// <summary>
        /// 条件删除
        /// </summary>
        public async Task<OpResult<int>> DeleteWithResultAsync<T>(Expression<Func<T, bool>> where) where T : class
        {
            var result = new OpResult<int> { IsSuccess = true };
            try
            {
                result.Any1 = await _freeSql.Delete<T>().Where(where).ExecuteAffrowsAsync();
                return result;
            }
            catch (Exception ex)
            {
                return HandleException<int>(ex, "条件删除失败");
            }
        }
        #endregion

        #region 事务操作
        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="action">事务内执行的操作</param>
        /// <returns>操作结果</returns>
        public async Task ExecuteTransactionAsync(Func<IFreeSql, Task> transactionAction, CancellationToken cancellationToken = default)
        {
            if (transactionAction == null)
                throw new ArgumentNullException(nameof(transactionAction));

            using (var uow = _freeSql.CreateUnitOfWork())
            {
                try
                {
                    await transactionAction(uow.Orm);
                }
                catch
                {
                    throw;
                }
            }
        }
        #endregion

        #region 辅助方法

        public Expression<Func<T, bool>> AddWhere<T>(Expression<Func<T, bool>> existingWhere, Expression<Func<T, bool>> newCondition)
        {
            if (existingWhere == null)
                return newCondition;

            // 组合两个条件（与运算）
            var parameter = Expression.Parameter(typeof(T));
            var visitor = new ParameterRebinder(parameter);
            var left = visitor.Visit(existingWhere.Body);
            var right = visitor.Visit(newCondition.Body);
            var combined = Expression.AndAlso(left, right);
            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }


        /// <summary>
        /// 异常处理通用方法
        /// </summary>
        private OpResult<T> HandleException<T>(Exception ex, string message)
        {
            return new OpResult<T>
            {
                IsSuccess = false,
                Message = $"{message}: {ex.Message}",
                Any1 = default
            };
        }


        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 释放托管资源
                (_freeSql as IDisposable)?.Dispose();
            }

            _disposed = true;
        }

        ~PGSQLHelper()
        {
            Dispose(false);
        }
    }


}
