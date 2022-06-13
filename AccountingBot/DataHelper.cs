using AccountingBot.Models;
using Dapper;
using System.Data.SQLite;

namespace AccountingBot
{
    public static class DataHelper
    {
        public async static Task<bool> AddMoneyRecordAsync(long userId, string item, decimal amount, long msgId, long typeId)
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            return await cnn.ExecuteAsync("insert into AccountingRecord(CreateTime, CreateUser, Item, Amount, MsgId, TypeId) values (@CreateTime, @CreateUser, @Item, @Amount, @MsgId, @TypeId)", new
            {
                CreateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                CreateUser = userId,
                Item = item,
                Amount = amount,
                MsgId = msgId,
                TypeId = typeId
            }) > 0;
        }

        public async static Task<IEnumerable<MoneyRecord>> GetMoneyRecordsAsync(long fromTime, long endTime = long.MaxValue)
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            return await cnn.QueryAsync<MoneyRecord>("select AccountingRecord.*,AccountingType.TypeName from AccountingRecord left join AccountingType on AccountingRecord.TypeId = AccountingType.TypeId where CreateTime between @fromTime and @endTime order by CreateTime", new
            {
                fromTime,
                endTime
            });
        }

        public async static Task<decimal> GetTodayAmountAsync()
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var result = await cnn.QueryAsync<MoneyRecord>("select * from AccountingRecord where CreateTime > @Timestamp", new
            {
                Timestamp = TimeHelper.GetTimestampOfToday()
            });
            return result.Sum(e => e.Amount);
        }

        public async static Task<bool> RemoveAccountingRecordAsync(long msgId)
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            return await cnn.ExecuteAsync("delete from AccountingRecord where MsgId = @MsgId", new
            {
                MsgId = msgId
            }) > 0;
        }

        public async static Task<long> GetAccountingTypeAsync(string typeName)
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var type = await cnn.QueryFirstOrDefaultAsync<AccountingTypeRecord>("select * from AccountingType where TypeName = @TypeName", new
            {
                TypeName = typeName
            });

            if (type != null)
            {
                return type.TypeId;
            }

            return -1;
        }

        public async static Task<AccountingTypeRecord> GetDefaultAccountingTypeAsync()
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var type = await cnn.QueryFirstOrDefaultAsync<AccountingTypeRecord>("select * from AccountingType where `Default` = TRUE");
            return type;
        }

        public async static Task<bool> AddAccountingTypeAsync(string typeName, bool IsDefault, long msgId)
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            return await cnn.ExecuteAsync("insert into AccountingType(TypeName, `Default`, MsgId) values (@TypeName, @Default, @MsgId)", new
            {
                TypeName = typeName,
                Default = IsDefault,
                MsgId = msgId
            }) > 0;
        }

        public async static Task<bool> RemoveAccountingTypeAsync(long msgId)
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            return await cnn.ExecuteAsync("delete from AccountingType where MsgId = @MsgId", new
            {
                MsgId = msgId
            }) > 0;
        }

        public async static Task<IEnumerable<AccountingTypeRecord>> GetAccountingTypeListAsync()
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var result = await cnn.QueryAsync<AccountingTypeRecord>("select * from AccountingType");
            return result;
        }

        public static SQLiteConnection SimpleDbConnection()
        {
            return new SQLiteConnection("Data Source=accounting.db");
        }
    }
}
