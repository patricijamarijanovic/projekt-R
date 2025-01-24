# projekt-R
unity avatar 👾

## Instalacija

### Unity Game Engine
Srce ovog projekta je Unity, višeplatformski softver za izradu igara. Unity se koristi za pokretanje avatara u ispitnom i razvojnom okruženju. 

Koraci za instalaciju Unity okruženja:
1. Kreirati [Unity ID](https://id.unity.com/en/conversations/4e95f832-fd5f-418b-a268-a309a113aae6005f) račun
2. Instalirati [Unity Hub](https://unity.com/download)
3. Instalirati [Unity Editor 2022.3.50.f1](unityhub://2022.3.50f1/c3db7f8bf9b1)

### Visual Studio (Windows)
Visual Studio je integrirano razvojno okruženje (IDE). Koristi se za razvoj i pisanje koda kompatibilnog s Unity okruženjem.

Koraci za instalaciju Visual Studio okruženja:
1. Instalirati [Visual Studio Installer](https://visualstudio.microsoft.com/downloads/)
2.  Odabrati jednu od verzija po želji
3. Prije same instalacije, odabrati iduće pakete:
	- .NET desktop development
	- Dekstop development with C++
	- Game development with Unity
4. Pokrenuti instalaciju

### Ollama
* preuzeti Ollama sa [poveznice](https://ollama.com)
* prezueti [llama3.2](https://ollama.com/library/llama3.2) unosom u konzolu
```
ollama run llama3.2
```

### TTS
TTS je biblioteka za generiranje teksta u govor koja se ispituje na Ubuntu operacijskom sustavu.

Upute za instalaciju TTS biblioteke na Windows operacijskom sustavu:
1. Instalirati [Python](https://www.python.org/downloads/) verziju **>= 3.9, < 3.12**, tijekom instalacije odabrati:
	- Install launcher for all users
	- Add Python _X.xx_ to PATH
2. Otvoriti naredbeni redak te se pozicionirati u direktorij po želji
3. Kreirati Python virtualno okruženje nardebom
``` 
python.exe -m venv. 
```
4. Pozicionirat se u direktorij ``Scripts``
```
cd .\Scripts\
```
5. Pokrenuti skriptu nardebom
```
.\activate
```
_U slučaju da vas zatkne greška s tekstom:_
```
...cannot be loaded because the execution of scripts is disabled on this system
```
_Pokrenite novi naredbeni redak uz opciju **Run as Administrator** te unesite_
```
set-executionpolicy remotesigned
```
6. Instalirati paket _Wheel_ nardebom
```
pip install wheel
```
7. Instalirati paket _TTS_ naredbom _(veliki paket je u pitanju, očekivano je da potraje)_
```
pip install TTS
```

### STT
* skinuti željeni model weight s [poveznice](https://huggingface.co/ggerganov/whisper.cpp/tree/main) (preporuka koristiti ggml-base.bin)
* staviti .bin datoteku u Assets/StreamingAssets/Whisper 
* u Whisper GameObject-u promijeniti Model Path

* više o veličinama modela na [OpenAi readme](https://github.com/openai/whisper#available-models-and-languages)

## Modeli
* **tts_models/en/ljspeech/glow-tts** --> verzija ženskastog/mješovitog TTS-a, najbolje zbog kombinacije kvalitete zvuka i brzine
* **tts_models/en/jenny/jenny** --> lijepi, ali spor engleski ženski glas
* **tts_models/hr/cv/vits** --> hrvatski tts koji je asistent Antun spominjao, baziran i na muškom i ženskom glasu, ima poteškoća dosta
* **tts_models/en/blizzard2013/capacitron-t2-c50** --> radi s emocijama, ali ne dobro baš

## Pokretanje
* prije pokretanja scene unutar Unity-a, obavezno pokrenuti **Ollama** aplikaciju (misli se na Ollama executable, ne na pokretanje modela unutar konzole), te Pyhton skriptu "**server.py**" **UNUTAR** virtualnog okruzenja, zatim pokrenuti scenu
