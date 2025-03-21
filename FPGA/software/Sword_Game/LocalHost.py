import socket
import time

HOST = '127.0.0.1'  # Localhost
PORT = 12345        # Port to listen on

# Create a TCP socket
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((HOST, PORT))  # Bind to the specified host and port
server_socket.listen(1)  # Listen for one connection at a time

print(f"Server listening on {HOST}:{PORT}...")

conn, addr = server_socket.accept()  # Accept an incoming connection
print(f"Connected by {addr}")

counter = 0  # Initialize counter

# Keep sending and receiving data
try:
    while True:
        # Send incrementing data to the client
        message = counter.to_bytes(1, byteorder = 'big')
        conn.sendall(message)  
        print(f"Sent: {message.strip()}")  # Print the sent message
        
        counter += 10  # Increment counter
        if counter > 90:
            counter = 90
        time.sleep(1)  # Wait for a second before sending the next message

        # Receive and print incoming data
        data = conn.recv(1024)  # Receive up to 15 bytes of data
        if not data:
            print("Done")
            break  # Stop if no data is received (client disconnected)
            
        
        received_data = data.decode().strip()
        print(f"Received: {received_data}")  # Print the received message

except KeyboardInterrupt:
    print("\nServer shutting down...")

finally:
    conn.close()  # Close the connection
    server_socket.close()  # Close the server socket
