# This is a proof of concept simulation script that should use WiFi (UDP protocol)
# to a Unity application
# Unity is acting as a server, python script is acting as client (should be replaced with an Arduino client)

import socket

def map_input(typed):
    if "u" in typed:
        return "1.0, 0.0, 0.0"
    elif "d" in typed:
        return "-1.0, 0.0, 0.0"
    elif "r" in typed:
        return "0.0, 0.0, 1.0"
    elif "l" in typed:
        return "0.0, 0.0, -1.0"

    elif "e" in typed:
        global boolStop
        boolStop = True
        return "0.0, 0.0, 0.0"
    else:
        return None



if __name__ == "__main__":
    boolStop = False

    server_IP = "127.0.0.1" # home ip for any device
    PORT = 6969
    soc = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    message = "0.0, 0.0, 0.0"

    while not boolStop:
        print("Press u for up, d for down, r for right, l for left")
        inp = input()
        msg = map_input(inp)
        if msg != None:
            msg = msg.encode("utf-8")
            soc.sendto(msg, (server_IP, PORT))

