from transformers import pipeline
import json
import socket

# Initialize emotion model
classifier = pipeline("text-classification", model="bhadresh-savani/distilbert-base-uncased-emotion", return_all_scores=True)

def analyze_emotion(text):
    # Analyze emotions
    result = classifier(text)
    # Transform results to a simpler format
    emotion_scores = {entry['label']: entry['score'] for entry in result[0]}
    return emotion_scores

def start_server(host="127.0.0.1", port=5005):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.bind((host, port))
        server_socket.listen()
        print("Python server listening for Unity connection...")

        while True:
            client_socket, addr = server_socket.accept()
            with client_socket:
                print(f"Connected by {addr}")
                
                # Receive text from Unity
                data = client_socket.recv(1024).decode('utf-8')
                if not data:
                    break

                print(f"Received text from Unity: {data}")

                # Run emotion analysis
                emotion_scores = analyze_emotion(data)
                
                # Send emotion data back to Unity
                emotion_json = json.dumps(emotion_scores)
                client_socket.sendall(emotion_json.encode('utf-8'))
                print(f"Sent emotion data: {emotion_json}")

if __name__ == "__main__":
    start_server()