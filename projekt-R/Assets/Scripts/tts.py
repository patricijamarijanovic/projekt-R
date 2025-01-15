from flask import Flask, request, jsonify
from TTS.api import TTS
app = Flask(__name__)

@app.route('/')
def index():
    print("App started")
    return jsonify(200)

@app.route("/", methods=["POST"])
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
    app.run(debug=True, host="0.0.0.0", port=5006)