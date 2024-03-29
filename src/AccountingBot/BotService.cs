﻿using AccountingBot.HttpApi;
using AccountingBot.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AccountingBot
{
    /// <summary>
    /// Bot后台服务类
    /// </summary>
    public class BotService : BackgroundService
    {
        private readonly IChatApi _chatApi;
        private readonly ILoginApi _loginApi;
        private string _session = string.Empty;
        private readonly BotConfig _config;

        public BotService(IChatApi chatApi, ILoginApi loginApi)
        {
            _chatApi = chatApi;
            _loginApi = loginApi;
            _config = JsonSerializer.Deserialize<BotConfig>(File.ReadAllText("config.json"));
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            while (await timer.WaitForNextTickAsync())
            {
                try
                {
                    var message = await _chatApi.FetchMessageAsync(_session, 10);
                    foreach (var item in message.Data)
                    {
                        var groupId = item?.Group?.Id ??
                            item?.Sender?.Group?.Id ??
                            0;

                        if (_config.Groups.Contains(groupId))
                        {
                            TextMessage messageContent = await GetMessageResponseAsync(item);

                            if (messageContent != null)
                            {
                                var content = JsonSerializer.Serialize(new
                                {
                                    target = groupId,
                                    messageChain = new List<TextMessage> { messageContent }
                                });

                                var sendResult = await _chatApi.SendGroupMessageAsync(_session, content);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    RefreshSession();
                }
            }
        }

        public static async Task<TextMessage> GetMessageResponseAsync(MessageOutput item)
        {
            TextMessage messageContent = null;
            switch (item.Type)
            {
                case "GroupRecallEvent":
                    if (await DataHelper.RemoveAccountingRecordAsync(item.MessageId))
                    {
                        messageContent = new TextMessage
                        {
                            Type = "Plain",
                            Text = $"撤销记账成功，今日总消费额：{await DataHelper.GetTodayAmountAsync()}"
                        };
                    }

                    if (await DataHelper.RemoveAccountingTypeAsync(item.MessageId))
                    {
                        messageContent = new TextMessage
                        {
                            Type = "Plain",
                            Text = @"撤销增加类别成功，查看""类别列表""试试"
                        };
                    }

                    break;
                case "GroupMessage":
                    long currentMsgId = 0;
                    foreach (var msg in item.MessageChain)
                    {
                        if (msg.Type == "Source")
                        {
                            currentMsgId = msg.Id;
                        }
                        if (msg.Type == "Plain")
                        {
                            Match match = Regex.Match(msg.Text, @"(.+)(\d+\.?\d*)", RegexOptions.RightToLeft);
                            if (match.Success)
                            {
                                string node = match.Groups[1].Value.Trim();
                                var price = decimal.Parse(match.Groups[2].Value);
                                var type = msg.Text[(match.Groups[2].Index + match.Groups[2].Value.Length)..].Trim();

                                if (type.StartsWith("元"))
                                {
                                    type = type[1..];
                                }

                                var hint = string.Empty;
                                var typeId = await DataHelper.GetAccountingTypeAsync(type);
                                if (typeId == -1)
                                {
                                    var defaultType = await DataHelper.GetDefaultAccountingTypeAsync();
                                    typeId = defaultType.TypeId;
                                    if (type != string.Empty)
                                    {
                                        hint = $"（提示：{type}不是有效的记账类型，已使用默认类型：{defaultType.TypeName}）";
                                    }
                                }

                                await DataHelper.AddMoneyRecordAsync(item.Sender.Id, node, price, currentMsgId, typeId);

                                messageContent = new TextMessage
                                {
                                    Type = "Plain",
                                    Text = $"记账成功，购入{node}，今日总消费额：{await DataHelper.GetTodayAmountAsync()}{hint}"
                                };
                            }
                            else if (msg.Text == "今日明细")
                            {
                                var details = await DataHelper.GetMoneyRecordsAsync(TimeHelper.GetTimestampOfToday());
                                if (details?.Count() > 0)
                                {
                                    messageContent = new TextMessage
                                    {
                                        Type = "Plain",
                                        Text = $"今日消费明细：\r\n{string.Join("\r\n", details.Select(e => $"【{e.TypeName}】{e.Item}：{e.Amount}元").ToList())}"
                                    };
                                }
                                else
                                {
                                    messageContent = new TextMessage
                                    {
                                        Type = "Plain",
                                        Text = $"今日暂无消费明细"
                                    };
                                }
                            }
                            else if ((msg.Text.StartsWith("新增类别") || msg.Text.StartsWith("增加类别")) && msg.Text.Length > 4)
                            {
                                var type = msg.Text[4..].Trim();
                                if (type.Length > 30)
                                {
                                    messageContent = new TextMessage
                                    {
                                        Type = "Plain",
                                        Text = "类别名称太长了"
                                    };
                                    break;
                                }

                                var typeId = await DataHelper.GetAccountingTypeAsync(type);
                                if (typeId > 0)
                                {
                                    messageContent = new TextMessage
                                    {
                                        Type = "Plain",
                                        Text = @"类别已存在，查看""类别列表""试试"
                                    };
                                    break;
                                }

                                await DataHelper.AddAccountingTypeAsync(type, false, currentMsgId);

                                messageContent = new TextMessage
                                {
                                    Type = "Plain",
                                    Text = "增加类别成功"
                                };
                            }
                            else if (msg.Text == "类别列表")
                            {
                                var typeList = await DataHelper.GetAccountingTypeListAsync();
                                messageContent = new TextMessage
                                {
                                    Type = "Plain",
                                    Text = $"类别列表：\r\n{string.Join("\r\n", typeList.Select(e => e.TypeName).ToList())}"
                                };
                            }
                            else if (msg.Text == "更正所有昨日数据" || msg.Text == "更正昨日数据")
                            {
                                var affectedDataCount = await DataHelper.CorrectYesterdayDataAsync();
                                if (affectedDataCount == 0)
                                {
                                    messageContent = new TextMessage
                                    {
                                        Type = "Plain",
                                        Text = "当前没有需要更正的数据"
                                    };
                                }
                                else
                                {
                                    messageContent = new TextMessage
                                    {
                                        Type = "Plain",
                                        Text = $"更正了{affectedDataCount}条昨日数据"
                                    };
                                }
                            }
                            else if (msg.Text == "更正所有历史数据" || msg.Text == "更正历史数据")
                            {
                                var affectedData = await DataHelper.CorrectAllDataAsync();
                                if (!affectedData.Any())
                                {
                                    messageContent = new TextMessage
                                    {
                                        Type = "Plain",
                                        Text = "当前没有需要更正的数据"
                                    };
                                }
                                else
                                {
                                    messageContent = new TextMessage
                                    {
                                        Type = "Plain",
                                        Text = $"更正了{affectedData.Count()}条历史数据：\r\n{string.Join("\r\n", affectedData)}"
                                    };
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return messageContent;
        }

        async void RefreshSession()
        {
            try
            {
                var sessionInfo = await _loginApi.VerifyAsync(JsonSerializer.Serialize(new
                {
                    verifyKey = _config.VerifyKey
                }));

                Console.WriteLine($"获取到Session: {sessionInfo.Session}");

                var bindResult = await _loginApi.BindAsync(JsonSerializer.Serialize(new
                {
                    sessionKey = sessionInfo.Session,
                    qq = _config.BotId
                }));

                if (bindResult.Code == 0)
                {
                    _session = sessionInfo.Session;
                    Console.WriteLine("绑定Bot成功，开始获取消息");
                }
                else
                {
                    Console.WriteLine("绑定Bot失败，退出程序");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
