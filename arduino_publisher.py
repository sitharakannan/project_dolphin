import serial
import time
from helper_functions import *
import zmq

ports = list(serial.tools.list_ports.comports())
for p in ports:
    print (p)

sPort = '/dev/cu.usbmodem14301'
ser =  serial.Serial(sPort,9600) 

port = "12346"
TIMEOUT = 10000

context = zmq.Context()
print ("Connecting to server...")
socket = context.socket(zmq.PUB)
socket.bind("tcp://*:%s" % port)


def read_and_publish_loop():
    while True:
        res = ser.readline()
        print (res.decode("utf-8"))
        socket.send_string(res.decode("utf-8"))
        # socket.send_string("hi")

try:
    read_and_publish_loop()
except KeyboardInterrupt:
    socket.close()
finally:
    print("Socket closed")
    print(str(socket.closed))


