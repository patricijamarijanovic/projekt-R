from flask import Flask, request, jsonify
import json
import socket
from langchain_ollama import OllamaLLM
from langchain_core.prompts import ChatPromptTemplate
from TTS.api import TTS
import re

app = Flask(__name__)

def extract_response_and_emotion_scores(response_string):
    # Regular expression to capture the response
    response_pattern = r"Answer:\s*(.*?)\n\n"  # Allow space between Answer and emotion analysis

    # Regular expression to capture emotion scores with optional spaces between values
    emotion_pattern = r"Emotion analysis:\s*sadness:\s*(\d+),\s*joy:\s*(\d+),\s*love:\s*(\d+),\s*anger:\s*(\d+),\s*fear:\s*(\d+),\s*surprise:\s*(\d+)"

    # Extract the response
    response_match = re.search(response_pattern, response_string, re.DOTALL)
    response = response_match.group(1).strip() if response_match else ""

    # Extract the emotion scores
    emotion_match = re.search(emotion_pattern, response_string)
    if emotion_match:
        emotion_scores = {
            "sadness": int(emotion_match.group(1)),
            "joy": int(emotion_match.group(2)),
            "love": int(emotion_match.group(3)),
            "anger": int(emotion_match.group(4)),
            "fear": int(emotion_match.group(5)),
            "surprise": int(emotion_match.group(6)),
        }
    else:
        emotion_scores = {}

    # Return the result as a dictionary
    return {
        "response": response,
        "emotion_scores": emotion_scores
    }
    
# naš prompt
template = """
You are a virtual avatar designed to interact with users, providing short, clear, and natural responses optimized for text-to-speech (TTS). 
Your replies should sound conversational, engaging, and easy to understand. 
Avoid using parentheses, symbols, or any formatting that could make the response hard to speak aloud. 
Focus on a friendly, approachable tone while keeping the response direct and clear.

Emotion analysis should reflect the sentiment of the user's message and your reply. 
Analyze the emotional tone of the user's query in addition to your response, 
and rate emotions (sadness, joy, love, anger, fear) as a percentage from 0 to 100. 
Surprise should be kept to a low or moderate level unless it's clearly a key emotion in the context. 
Always include all emotions, even if their score is 0. If no emotion is strongly detected, set the score to 0 for that emotion.

Here is the conversation history: {context}

The user says: {question}  

Respond in the following format:

Answer:
response

(Leave a blank line here)

Emotion analysis:
sadness: sadness_score (0-100), joy: joy_score (0-100), love: love_score (0-100), anger: anger_score (0-100), fear: score (0-100), surprise: surprise_score (0-100)

"""

model = OllamaLLM(model = "llama3.2") # odabrani model

prompt = ChatPromptTemplate.from_template(template)

chain = prompt | model

def handle_conversation(context, user_input):
    result = chain.invoke({"context": context, "question": user_input})
    return result

@app.route("/")
def index():
    return jsonify(200)

@app.route("/chat", methods=["POST"])
def chat():
    data = request.get_json()

    # Extract context and user input from the incoming request
    context = data.get("context", "")
    user_input = data.get("user_input", "")

    # Process the conversation
    result = chain.invoke({"context": context, "question": user_input})

    # Extract response and emotion scores
    result2 = extract_response_and_emotion_scores(result)

    if len(result2["response"]) == 0:
        # Process the conversation again
        result = chain.invoke({"context": context, "question": user_input})
        # Extract response and emotion scores
        result2 = extract_response_and_emotion_scores(result)
        

    print(result2)

    # Update the context with the new conversation
    context += f"\nUser: {user_input}\nAI: {result}"

    # Return the result as JSON
    return jsonify({
        "response": result2["response"],
        "emotion_scores": result2["emotion_scores"],
        "context": context
    })

@app.route("/tts", methods=["POST"])
def speech():
    print("Got request")
    data = request.get_json()
    print("Got data")
    print(data)
    text = data.get("text", "")
    tts = TTS(model_name="tts_models/en/ljspeech/glow-tts")
    tts.tts_to_file(text=text, file_path="../Resources/output.wav", emotion="neutral")
    return jsonify(201)


if __name__ == "__main__":
    app.run(debug=True, host="0.0.0.0", port=5005)