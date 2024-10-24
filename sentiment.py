from transformers import pipeline
from flask import Flask, request, jsonify

app = Flask(__name__)

#sentiment_pipeline = pipeline("sentiment-analysis", model="cardiffnlp/twitter-roberta-base-sentiment-latest")
sentiment_pipeline = pipeline("text-classification", model='bhadresh-savani/distilbert-base-uncased-emotion')

@app.route("/analyze-sentiment", methods = ["POST"])
def analyze_sentiment():
    data = request.get_json()
    text = data.get("text")
    result = sentiment_pipeline(text)[0]
    return jsonify(result) # {"label":   , "score":    }


if __name__ == "__main__":
    app.run(debug = True) 