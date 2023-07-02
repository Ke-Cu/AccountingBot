import time
import re
import sqlite3


def removeprefix(text, prefix):
    if not prefix:
        return text
    if text.startswith(prefix):
        return text[len(prefix):]
    else:
        return text


def removesuffix(text, suffix):
    if not suffix:
        return text
    if text.endswith(suffix):
        return text[:-len(suffix)]
    else:
        return text


user_info_dict = {
}

# 1	餐饮
# 2	交通
# 3	日用品
# 9	服饰
# 10	美妆
# 11	宠物
# 12	电子产品
# 13	住房
# 14	家居
# 15	学习
# 16	汽车
# 17	其他
# 18	医疗
# 19	旅游

type_dict = {
    "餐饮": 1,
    "交通": 2,
    "日用品": 3,
    "服饰": 9,
    "美妆": 10,
    "宠物": 11,
    "电子产品": 12,
    "住房": 13,
    "家居": 14,
    "学习": 15,
    "汽车": 16,
    "其他": 17,
    "医疗": 18,
    "旅游": 19
}


class Record:
    def __init__(self, create_time=None, create_user=None, item=None, amount=None, type_id=None):
        self.create_time = create_time
        self.create_user = create_user
        self.item = item
        self.amount = amount
        self.type_id = type_id


with open('record.txt', 'r', encoding='utf-8') as f:
    record_row = 0
    record_list = []
    for line in f:
        record_row += 1

        if line.strip() == '':
            if record_row == 1:
                exit('Error: record is not complete')
            record_row = 0

        if record_row == 1:
            record_list.append(Record())
            # get the record in user_info_dict where the key is in the line
            record = [user for user in user_info_dict if user in line]
            if not record:
                exit('Error: user not found: ' + line)
            else:
                user_name = record[0]
                current_record = record_list[-1]
                current_record.create_user = user_info_dict[user_name]
                date = line.split(user_name)[1].strip()
                # convert date to timestamp, utc time
                timestamp = int(time.mktime(time.strptime(
                    date, '%Y/%m/%d %H:%M:%S'))) * 1000
                current_record.create_time = timestamp

        if record_row == 2:
            current_record = record_list[-1]
            item = line.strip()
            # use regex find the last decimal number in the line
            amount = re.findall(r'\d+\.?\d*', line)[-1]
            current_record.amount = float(amount)
            if item.startswith('昨日'):
                item = removeprefix(item, '昨日')
                current_record.create_time -= 86400000
            # type is the content after the last decimal number
            type_name = item.split(amount)[-1].strip()
            current_record.type_name = type_name if type_name else '餐饮'
            current_record.type_id = type_dict[current_record.type_name]
            # the current_item item is the rest of the line
            current_record.item = removesuffix(
                removesuffix(item, type_name), amount).strip()


conn = sqlite3.connect('accounting.db')
c = conn.cursor()

for record in record_list:
    c.execute('''SELECT * FROM AccountingRecord WHERE CreateTime = ? AND CreateUser = ? AND Item = ? AND Amount = ? AND TypeId = ?''',
              (record.create_time, record.create_user, record.item, record.amount, record.type_id))
    if c.fetchone() is not None:
        print("Data already exists, skipping insertion: " + str(record.__dict__))
    else:
        c.execute('''INSERT INTO AccountingRecord (CreateTime, CreateUser, Item, Amount, MsgId, TypeId) 
                     VALUES (?, ?, ?, ?, ?, ?)''',
                  (record.create_time, record.create_user, record.item, record.amount, -1, record.type_id))
        conn.commit()

print("账单记录完成，点击查看：http://kecu.life/")

c.close()
conn.close()
