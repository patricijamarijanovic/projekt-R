from TTS.api import TTS

# MODELS:
# "tts_models/en/ljspeech/glow-tts" --> fastest/best version of English female/mixed tts
# "tts_models/en/jenny/jenny" --> nice but slow English female voice
# "tts_models/hr/cv/vits" --> Croatian tts which assistant mentioned, it is not very clean, but honest work
# "tts_models/en/blizzard2013/capacitron-t2-c50" --> it has some emotion mapping, but not very good quality

# returns string which represents tts model which user wants to use
def choose_tts_model():
    model_names = [
        "tts_models/en/ljspeech/glow-tts",
        "tts_models/en/jenny/jenny",
        "tts_models/hr/cv/vits"]
    return model_names[int(input("Press 0: english male/female voice\nPress 1: english female voice\nPress 2: croatian voice\nYour choice: "))]

# returns audiofile for given TTS model and text with optional emotion
def load_tts_and_save_to_file(model_name, text, output_filename = "output", emotion = "neutral"):
    tts = TTS(model_name = model_name)
    tts.tts_to_file(text = text, file_path = "../Assets/Resources/" + output_filename + ".wav", emotion = emotion)
    return



my_model_name = choose_tts_model()

while True:
    my_text = input("Text for C# and audiofile: ")
    load_tts_and_save_to_file(
        model_name = my_model_name,
        text = my_text)
    with open("../Assets/Resources/output.txt", "w") as file:
        file.write(my_text)