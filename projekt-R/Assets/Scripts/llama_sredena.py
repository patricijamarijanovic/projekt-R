from flask import Flask, request, jsonify
import json
import socket
from langchain_ollama import OllamaLLM
from langchain_core.prompts import ChatPromptTemplate

app = Flask(__name__)

def fix_incomplete_json(result):
    # Provjeravamo je li odgovor valjan JSON, ako nije, dodajemo zatvorenu zagradu.
    if result.count("{") != result.count("}") or not result.endswith("}"):
        return result + "\n}"  # Dodajemo zatvorenu zagradu na kraj ako nedostaje
    return result

# Define the prompt template
template = """
You are a virtual avatar designed to interact with users, providing short, clear, and natural responses optimized for text-to-speech (TTS). 
Your replies should sound conversational, engaging, and easy to understand. 
Avoid using parentheses, symbols, or any formatting that could make the response hard to speak aloud. 
Focus on a friendly, approachable tone while keeping the response direct and clear.

Emotion analysis should reflect the sentiment of the user's message and your reply. 
Analyze the emotional tone of the user's query in addition to your response, 
and rate emotions (sadness, joy, love, anger, fear) as a percentage from 0 to 100. 
Surprise should be kept to a low or moderate level unless it's clearly a key emotion. 
Always include all emotions, even if their score is 0. If no emotion is strongly detected, set the score to 0 for that emotion.

Here is the conversation history: {context}

The user says: {question}  

Your response should **only** include the following JSON format. Always include the closing brace of the entire JSON object. Failure to follow this rule will result in incorrect output. You must strictly follow this format without any additional text:
{{
    "emotion_scores": {{
        "anger": <anger_score>,
        "fear": <fear_score>,
        "joy": <joy_score>,
        "love": <love_score>,
        "sadness": <sadness_score>,
        "surprise": <surprise_score>
    }},
    "response": "<response_text>"
}}


"""

# Initialize the model
model = OllamaLLM(model="llama3.2")

# Create the prompt template
prompt = ChatPromptTemplate.from_template(template)

# Create the chain
chain = prompt | model


@app.route("/")
def index():
    return jsonify(200)

@app.route("/chat", methods=["POST"])
def chat():
    data = request.get_json()

    # Extract context and user input from the incoming request
    context = data.get("context", "")
    user_input = data.get("user_input", "")
    
    # Initialize emotion_scores
    emotion_scores_default = {
        "anger": 0,
        "fear": 0,
        "joy": 60,
        "love": 0,
        "sadness": 0,
        "surprise": 0
    }
    emotion_scores = emotion_scores_default

    # Process the conversation
    result = chain.invoke({
        "context": context, 
        "question": user_input
    })

    print("ovo je rezultat: " + result)

    
    try:
        result_json = json.loads(result)
        response = result_json.get("response", "")
        emotion_scores = result_json.get("emotion_scores", emotion_scores)
    except json.JSONDecodeError:
        # Ako parsiranje ne uspije, popravimo odgovor
        result = fix_incomplete_json(result)
        print("popravljam odgovor, novi odgovor " + result)
        
        # Ponovno pokušaj parsirati popravljen odgovor
        try:
            result_json = json.loads(result)
            response = result_json.get("response", "")
            emotion_scores = result_json.get("emotion_scores", emotion_scores)
        except json.JSONDecodeError:
            # Ako još uvijek ne uspije, pošaljemo default odgovor
            response = "Sorry, I didn't catch that. Can you repeat?"
            

    # Update the context with the new conversation
    context += f"\nUser: {user_input}\nAI: {response}"

    # Return the result as JSON
    return jsonify({
        "response": response,
        "emotion_scores": emotion_scores,
        "context": context
    })


if __name__ == "__main__":
    app.run(debug=True, host="0.0.0.0", port=5006)