import zmq, datetime, time, json

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
    socket.send_multipart(['fake-topic'.encode(), json.dumps(payload).encode('utf-8')])

    #  Get the reply
    reply = socket.recv()
    print(f"Received reply: {reply}")

    time.sleep(2)
