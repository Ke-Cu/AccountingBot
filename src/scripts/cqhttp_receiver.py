
import http.server
import socketserver
import json
import requests

remote_url = ""
local_url = ""
group_id = None

def send_message(message):
    url = local_url
    payload = json.dumps({
        "group_id": group_id,
        "message": message,
        "auto_escape": True
    })
    headers = {
        'Content-Type': 'application/json'
    }
    requests.request("POST", url, headers=headers, data=payload)


def make_remote_call(input):
    url = remote_url
    headers = {
        'Content-Type': 'application/json',
        'Authorization': ''}
    response = requests.request(
        "POST", url, headers=headers, data=json.dumps(input))
    if response.status_code == 200 and response.text:
        data = json.loads(response.text)
        text = data['text']
        send_message(text)


def handle_group_message(json_data):
    remote_call = {}
    remote_call["MessageId"] = json_data["message_id"]
    remote_call["Type"] = "GroupMessage"
    remote_call["MessageChain"] = []
    remote_call["MessageChain"].append(
        {"Type": "Source", "Id": json_data["message_id"]})
    remote_call["MessageChain"].append(
        {"Type": "Plain", "Text": json_data["raw_message"]})
    remote_call["Sender"] = {"Id": json_data["user_id"]}
    make_remote_call(remote_call)

def handle_recall_message(json_data):
    remote_call = {}
    remote_call["MessageId"] = json_data["message_id"]
    remote_call["Type"] = "GroupRecallEvent"
    make_remote_call(remote_call)

class MyHandler(http.server.SimpleHTTPRequestHandler):
    def do_POST(self):
        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        text = post_data.decode('utf-8')
        json_data = json.loads(text)
        post_type = json_data["post_type"]
        if post_type == "message" and json_data["message_type"] == "group":
            response = handle_group_message(json_data)
        elif post_type == "notice" and json_data["notice_type"] == "group_recall":
            response = handle_recall_message(json_data)
        self.send_response(200)
        self.end_headers()

    def log_message(self, format, *args):
        return


PORT = 5701
Handler = MyHandler

with socketserver.TCPServer(("", PORT), Handler) as httpd:
    print("serving at port", PORT)
    httpd.serve_forever()
