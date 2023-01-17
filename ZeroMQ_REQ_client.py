import zmq, datetime, time, json, msgpack
from datetime import timedelta
base_time = datetime.datetime(1, 1, 1)
delta = timedelta(microseconds=1)
def send_payload(pub_sock, topic, message, originatingTime=None):
    payload = {}
    payload[u"message"] = message
    if originatingTime is None:
        originatingTime = (datetime.datetime.utcnow() - base_time)/delta * 1e1
    payload[u"originatingTime"] = originatingTime
    print(payload)
    pub_sock.send_multipart([topic.encode(), msgpack.dumps(payload)])
    return originatingTime

context = zmq.Context()
socket = context.socket(zmq.REQ)

print("Connecting to server...")
socket.connect("tcp://128.2.204.249:40001")
time.sleep(1)

request = ">>>>> Hello from afar! <<<<<<"
old = 0
new = 0
flag = 0
while True:

    # Send the request
    payload = {}
    payload['message'] = request
    payload['originatingTime'] = datetime.datetime.utcnow().isoformat()
    print(f"Sending request: {request}")
    # socket.send_multipart(['fake-topic'.encode(), json.dumps(payload).encode('utf-8')])
    new = send_payload(socket, "fake-topic", [1, 2, 3])
    if((new - old)!=0):
        print(new - old)
        if flag == 1:
            break
        flag = 1
    old = new
    #  Get the reply
    reply = socket.recv()
    print(f"Received reply: {reply}")

    # time.sleep(2)
