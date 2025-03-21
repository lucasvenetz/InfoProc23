import intel_jtag_uart
import sys
import time
import matplotlib.pyplot as plt
import numpy as np
from collections import deque
import socket
import threading

received_data = ""
received_data_lock = threading.Lock()

HOST = '127.0.0.1'  # Change this to the server’s IP if running on a different machine
PORT = 12345

try:
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client_socket.connect((HOST, PORT))  # Connect to the server
    print(f"Connected to TCP server at {HOST}:{PORT}")
except Exception as e:
    print(f"Failed to connect to server: {e}")
    sys.exit(1)

NIOS_CMD_SHELL_BAT = "C:/intelFPGA_lite/18.1/nios2eds/Nios II Command Shell.bat"
BUFFER_SIZE = 4000  # Set rolling buffer size
temp_led = 57
def receive_data(): # Server receiver
    global temp_led
    while True:
        try:
            received_data = client_socket.recv(1024)  # Receive up to 1024 bytes
            if not received_data:
                print("Server disconnected.")
                break  # Exit loop if connection is lost
            print(f"Received from server: {received_data}")
            received_data.strip()
            print(f"direct {received_data}")
            temp_led = int(int(received_data)/10) - 1
            print(f"idk {temp_led}")
            if temp_led < 0:
                temp_led = 0 
            temp_led = temp_led + 48
            temp_led = temp_led & 255
            

     
        except Exception as e:
            print(f"Error receiving data: {e}")
            break  # Exit loop on error
        

# Start the receiving thread
receive_thread = threading.Thread(target=receive_data, daemon=True)
receive_thread.start()

def read_jtag():
    global temp_led
    try:
        ju = intel_jtag_uart.intel_jtag_uart()
    except Exception as e:
        print(e)
        sys.exit(1)

    print("Listening for incoming JTAG UART data... (Press Ctrl+C to stop)")

    # Rolling buffers for storing the data
    data_buffer_1 = deque(maxlen=BUFFER_SIZE)
    data_buffer_2 = deque(maxlen=BUFFER_SIZE)
    data_buffer_3 = deque(maxlen=BUFFER_SIZE)

    # Set up the figure with three subplots
    fig, axes = plt.subplots(3, 1, figsize=(8, 10))
    fig.suptitle('Accelerometer Data (Three Channels)')

    labels = ['X_Value', 'Y_Value', 'Z_Value']
    lines = []

    for i, ax in enumerate(axes):
        ax.set_xlabel('Sample Index')
        ax.set_ylabel(labels[i])
        ax.set_ylim(-300, 300)  # Adjust as needed
        line, = ax.plot([], [], label=labels[i])
        ax.legend()
        lines.append(line)

    plt.ion()
    plt.show()

    line_buffer = ""  # Buffer for handling partial reads
    previous_data = " "
    previousMoveTime = 0
    previousTimeJab = 0
    move_blocker = False
    impulse_slash = 0
    impulse_jump = 0

    sampling_count = 0  
    pr = False       
    start_time = time.time()
    while True:
        data = ju.read()
        # print(f"WTF {received_data}")
        # received_data_int = int(int(received_data)/10)  # Converts string "42" to integer 42
        # received_data_byte = received_data_int.to_bytes(1, byteorder='little')  # Convert to 1-byte (big-endian)
        ju.write(temp_led.to_bytes(1,byteorder = "little"))
        print(temp_led.to_bytes(1,byteorder = "little"))   
        if data:
            decoded_data = data.decode(errors="ignore")
            line_buffer += decoded_data

            while "\n" in line_buffer:
                line_str, line_buffer = line_buffer.split("\n", 1)
                line_str = line_str.strip()

                if line_str:
                    try:
                        values = line_str.split()
                        if len(values) != 4:
                            print(f"Invalid data format: {line_str}")
                            continue

                        signed_ints = []
                        for value in values:
                            signed_int = int(value, 16)
                            if signed_int > 0x7FFFFFFF:
                                signed_int -= 0x100000000
                            signed_ints.append(signed_int)
                            
                        buttonBit = int(values[3], 16)
                            
                        # sampling test #
                        current_time = time.time()
                        if current_time - start_time >= 10 and pr == False: 
                            print(sampling_count)
                            pr = True
                        else:
                            sampling_count = sampling_count + 1
                        
                        # Add the new data to the rolling buffers
                        data_buffer_1.append(signed_ints[0])
                        data_buffer_2.append(signed_ints[1])
                        data_buffer_3.append(signed_ints[2])
                        
                        # Impulse detection logic
                        window_size = 100
                        slash_threshold = 100
                        jump_threshold = 100
                        timeout = 0.3
                        
                        move_blocker = (time.time()- previousMoveTime) < 0.3
                        if len(data_buffer_3) >= window_size:  # Ensure enough data
                            window_jump = list(data_buffer_3)[-window_size:]
                            avg_jump = sum(window_jump) / len(window_jump)
                            if (signed_ints[2] - avg_jump) > jump_threshold and not move_blocker:
                                impulse_jump = 1
                                previousMoveTime = time.time()

                            elif not move_blocker:
                                impulse_jump = 0

                        if len(data_buffer_3) >= window_size:  # Ensure enough data
                            window_slash = list(data_buffer_3)[-window_size:]
                            avg_slash = sum(window_slash) / len(window_slash)
                            if (avg_slash - signed_ints[2]) > slash_threshold and not move_blocker:
                                impulse_slash = 1
                                previousMoveTime = time.time()

                            elif not move_blocker:
                                impulse_slash = 0
                        
                        jab_threshold = 100
                        impulse_jab = 0

                        if len(data_buffer_2) >= window_size:  # Ensure enough data
                            window_jab = list(data_buffer_2)[-window_size:]
                            avg_jab = sum(window_jab) / len(window_jab)

                            if (signed_ints[1] - avg_jab > jab_threshold) and (time.time() - previousTimeJab > timeout):
                                impulse_jab = 1
                                previousTimeJab = time.time()
                            else:
                                impulse_jab = 0
                                
                        if signed_ints[0] > 85:
                            A = 1
                            D = 0
                        elif signed_ints[0] < -85:
                            A = 0
                            D = 1
                        else: 
                            A = 0
                            D = 0
                            
                        R = 0
                        
                        if buttonBit == 3:
                            impulse_jab = 1
                            S = 1
                        elif buttonBit == 2:
                            impulse_jab = 0
                            S = 1
                        elif buttonBit == 1:
                            impulse_jab = 1
                            S = 0
                        else:
                            impulse_jab = 0
                            S = 0
                        
                        # Convert data to CSV format and send it to the TCP server
                        data_to_send = f"<Key>{impulse_jump}/{A}/{S}/{D}/{impulse_slash}/{R}/{impulse_jab}/</Key>\n"

                        try:
                            if data_to_send != previous_data:
                                client_socket.sendall(data_to_send.encode())
                                print(f"Sent data to server: {data_to_send}")
                                previous_data = data_to_send
                        except Exception as e:
                            print(f"Failed to send data to server: {e}")
                            client_socket.close()
                            sys.exit(1)

                    except ValueError:
                        print(f"Invalid data received (not hex?): {line_str}")
                        continue  # Skip this iteration

            # Update plots with rolling buffer data
            for i, (buffer, line) in enumerate(zip([data_buffer_1, data_buffer_2, data_buffer_3], lines)):
                line.set_xdata(np.arange(len(buffer)))
                line.set_ydata(list(buffer))
                axes[i].relim()
                axes[i].autoscale_view(True, True, True)

            plt.pause(0.05)

        

def main():
    read_jtag()

if __name__ == "__main__":
    main()
