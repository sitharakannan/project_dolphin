import sys
import zmq

port = "12346"

# Socket to talk to server
context = zmq.Context()
socket = context.socket(zmq.SUB)
socket.setsockopt_string(zmq.SUBSCRIBE, '')

print ("Collecting updates ")
socket.connect ("tcp://10.25.174.235:%s" % port)

while True:
    data = socket.recv()
    print (data)
