import zmq, datetime, time, json, msgpack

base_time = datetime.datetime(1, 1, 1)
def send_payload(pub_sock, topic, message, originatingTime=None):
    payload = {}
    payload[u"message"] = message
    if originatingTime is None:
        originatingTime = int((datetime.datetime.utcnow() - base_time).total_seconds()) * 1e7
    payload[u"originatingTime"] = originatingTime
    pub_sock.send_multipart([topic.encode(), msgpack.dumps(payload)])

context = zmq.Context()
socket = context.socket(zmq.REQ)

print("Connecting to server...")
socket.connect("tcp://128.2.204.249:40001")
time.sleep(1)

request = ">>>>> Hello from afar! <<<<<<"

while True:

    # Send the request
    payload = {}
    payload['message'] = request
    payload['originatingTime'] = datetime.datetime.utcnow().isoformat()
    print(f"Sending request: {request}")
    # socket.send_multipart(['fake-topic'.encode(), json.dumps(payload).encode('utf-8')])
    send_payload(socket, "fake-topic", {"keypoints":[1, 2, 3], "poses": ['sleeping', 'standing']})
    #  Get the reply
    reply = socket.recv()
    print(f"Received reply: {reply}")

    time.sleep(2)
