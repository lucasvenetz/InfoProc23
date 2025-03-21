import intel_jtag_uart
import sys
import time
import matplotlib.pyplot as plt
import numpy as np
from collections import deque
import socket
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

def read_jtag():
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

    # Initialize text annotations once outside the loop
    text_annotations = [ax.text(0.8, 0.9, "", transform=ax.transAxes, fontsize=12) for ax in axes]

    # Initialize the impulse annotation (for impulse detection text)
    impulse_jab_annotation = axes[2].annotate(
        "", xy=(0.5, 0.75), xycoords="axes fraction", ha="center", va="center", fontsize=14, color="red", fontweight="bold"
    )
    impulse_slash_annotation = axes[2].annotate(
        "", xy=(0.5, 0.95), xycoords="axes fraction", ha="center", va="center", fontsize=14, color="blue", fontweight="bold"
    )

    plt.ion()
    plt.show()

    line_buffer = ""  # Buffer for handling partial reads
    previous_data = " "
    previousMoveTime = 0
    previousTimeJab = 0
    move_blocker = False
    # sampling_count = 0  
    # pr = False                      
    impulse_slash = 0
    impulse_jump = 0
    start_time = time.time()
    while True:
        data = ju.read()
        if data:
            decoded_data = data.decode(errors="ignore")
            line_buffer += decoded_data

            while "\n" in line_buffer:
                line_str, line_buffer = line_buffer.split("\n", 1)
                line_str = line_str.strip()

                if line_str:
                    try:
                        values = line_str.split()
                        if len(values) != 3:
                            print(f"Invalid data format: {line_str}")
                            continue

                        signed_ints = []
                        for value in values:
                            signed_int = int(value, 16)
                            if signed_int > 0x7FFFFFFF:
                                signed_int -= 0x100000000
                            signed_ints.append(signed_int)

                        # sampling test #
                        # current_time = time.time()
                        # if current_time - start_time >= 10 and pr == False: 
                        #     print(sampling_count)
                        #     pr = True
                        # else:
                        #     sampling_count = sampling_count + 1

                        # Add the new data to the rolling buffers
                        data_buffer_1.append(signed_ints[0])
                        data_buffer_2.append(signed_ints[1])
                        data_buffer_3.append(signed_ints[2])
                        
                        # Impulse detection logic
                        window_size = 100
                        slash_threshold = 80
                        jump_threshold = 80

                        timeout = 0.3
                        
                        move_blocker = (time.time()- previousMoveTime) < 0.4
                        if len(data_buffer_3) >= window_size:  # Ensure enough data
                            window_jump = list(data_buffer_3)[-window_size:]  # Convert deque to list for slicing
                            avg_jump = sum(window_jump) / len(window_jump)
                            if (signed_ints[2] - avg_jump) > jump_threshold and not move_blocker:
                                impulse_jump = 1
                                previousMoveTime = time.time()
                                #ju.write(b'ID\n')  # Send the impulse message back through UART

                            elif not move_blocker:
                                impulse_jump = 0
                                #ju.write(b'NID')  # Send the impulse message back through UART

                        if len(data_buffer_3) >= window_size:  # Ensure enough data
                            window_slash = list(data_buffer_3)[-window_size:]  # Convert deque to list for slicing
                            avg_slash = sum(window_slash) / len(window_slash)
                            if (avg_slash - signed_ints[2]) > slash_threshold and not move_blocker:
                                impulse_slash_annotation.set_text("Slash Impulse Detected!")  # Show text on the plot
                                impulse_slash = 1
                                previousMoveTime = time.time()
                                #ju.write(b'ID\n')  # Send the impulse message back through UART

                            elif not move_blocker:
                                impulse_slash_annotation.set_text("")  # Clear text when no impulse detected
                                impulse_slash = 0
                                #ju.write(b'NID')  # Send the impulse message back through UART
                        
                        jab_threshold = 100
                        impulse_jab = 0

                        if len(data_buffer_2) >= window_size:  # Ensure enough data
                            window_jab = list(data_buffer_2)[-window_size:]  # Convert deque to list for slicing
                            avg_jab = sum(window_jab) / len(window_jab)

                            if (signed_ints[1] - avg_jab > jab_threshold) and (time.time() - previousTimeJab > timeout):
                                impulse_jab_annotation.set_text("Jab Impulse Detected!")  # Show text on the plot
                                impulse_jab = 1
                                previousTimeJab = time.time()
                                #ju.write(b'ID\n')  # Send the impulse message back through UART

                            else:
                                impulse_jab_annotation.set_text("")  # Clear text when no impulse detected
                                impulse_jab = 0
                                
                        if signed_ints[0] > 75:
                            A = 1
                            D = 0
                        elif signed_ints[0] < -75:
                            A = 0
                            D = 1
                        else: 
                            A = 0
                            D = 0

                        # if signed_ints[1] > 100 and not jumpBlocker:
                        #     W = 1
                        #     S = 0
                        # elif signed_ints[1] < -100:
                        #     W = 0
                        #     S = 1
                        # else: 
                        #     W = 0
                        #     S = 0
                        S = 0
                        E = 0
                        Q = 0
                        
                        # Convert data to CSV format and send it to the TCP server
                        data_to_send = f"<Key>{impulse_jump}/{A}/{S}/{D}/{impulse_slash}/{impulse_jab}/{Q}/</Key>\n"

                        
                        try:
                            if data_to_send != previous_data:
                                client_socket.sendall(data_to_send.encode())
                                print(f"Sent data to server: {data_to_send}")
                                previous_data = data_to_send
                        except Exception as e:
                            print(f"Failed to send data to server: {e}")
                            client_socket.close()
                            sys.exit(1)


                        # Update text annotations with the latest values
                        for i, buffer in enumerate([data_buffer_1, data_buffer_2, data_buffer_3]):
                            if buffer:
                                text_annotations[i].set_text(f"{labels[i]}: {buffer[-1]}")

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
