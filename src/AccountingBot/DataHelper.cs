using AccountingBot.Models;
using Dapper;
using System.Data.SQLite;

namespace AccountingBot
{
    public static class DataHelper
    {
        public async static Task<bool> AddMoneyRecordAsync(long userId, string item, decimal amount, long msgId, long typeId, long createTime = 0)
        {
            if (createTime == 0)
            {
                createTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
            using var cnn = SimpleDbConnection();
            cnn.Open();
            return await cnn.ExecuteAsync("insert into AccountingRecord(CreateTime, CreateUser, Item, Amount, MsgId, TypeId) values (@CreateTime, @CreateUser, @Item, @Amount, @MsgId, @TypeId)", new
            {
                CreateTime = createTime,
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

        /// <summary>
        /// 通过记录ID删除账单记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async static Task<bool> RemoveAccountingRecordByIdAsync(long id)
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            return await cnn.ExecuteAsync("delete from AccountingRecord where Id = @Id", new
            {
                Id = id
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

        public async static Task<long> GetAccountingTypeAsync(long typeId)
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var type = await cnn.QueryFirstOrDefaultAsync<AccountingTypeRecord>("select * from AccountingType where TypeId = @TypeId", new
            {
                TypeId = typeId
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

        public async static Task<int> CorrectYesterdayDataAsync()
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var result = await cnn.ExecuteAsync("UPDATE AccountingRecord SET Item = replace(Item, '昨日', ''), CreateTime = CreateTime - 86400000 WHERE Item LIKE '昨日%'");
            return result;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public async static Task<UserRecord> GetUserAsync(string userName)
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var result = await cnn.QueryFirstOrDefaultAsync<UserRecord>("select * from User where UserName = @UserName", new
            {
                UserName = userName
            });

            return result;
        }

        public static SQLiteConnection SimpleDbConnection()
        {
            return new SQLiteConnection("Data Source=accounting.db");
        }

        public async static Task<List<string>> CorrectAllDataAsync()
        {
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var result = new List<string>();
            var records = await cnn.QueryAsync<AccountingRecord>("select * from AccountingRecord where Item like '%月%日%'");
            if (records != null)
            {
                foreach (var record in records)
                {
                    var positionToRemove = record.Item.IndexOf("日") + 1;

                    if (positionToRemove > 0 && (positionToRemove + 1) < record.Item.Length)
                    {
                        var timeString = record.Item.Remove(positionToRemove);
                        var timestamp = TimeHelper.GetLocalTimeFromTimeString(timeString);
                        var item = record.Item.Substring(positionToRemove);
                        var affectedRows = await cnn.ExecuteAsync("UPDATE AccountingRecord SET Item = @Item, CreateTime = @CreateTime WHERE Id = @Id", new
                        {
                            Item = item,
                            CreateTime = timestamp,
                            Id = record.Id,
                        });

                        if (affectedRows > 0)
                        {
                            result.Add(record.Item);
                        }
                    }
                }
            }

            return result;
        }
    }
}
