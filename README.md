# projekt-R
unity avatar 👾

## Instalacija
Unity verzija 2022.3.50.f1
### Upute za instalaciju LLM
* preuzeti Ollama sa [poveznice](https://ollama.com)
* prezueti [llama3.2](https://ollama.com/library/llama3.2) unosom u konzolu
```
ollama run llama3.2
```

### Upute za instalaciju TTS
* za **Windows** operativni sustav na [poveznici](https://youtu.be/zRaDe08cUIk?si=m4RBhnSUEjLjH-c0)
* za **Linux** opreativni sustav na [poveznici](https://github.com/coqui-ai/TTS?tab=readme-ov-file#installation)

## Modeli
* **tts_models/en/ljspeech/glow-tts** --> verzija ženskastog/mješovitog TTS-a, najbolje zbog kombinacije kvalitete zvuka i brzine
* **tts_models/en/jenny/jenny** --> lijepi, ali spor engleski ženski glas
* **tts_models/hr/cv/vits** --> hrvatski tts koji je asistent Antun spominjao, baziran i na muškom i ženskom glasu, ima poteškoća dosta
* **tts_models/en/blizzard2013/capacitron-t2-c50** --> radi s emocijama, ali ne dobro baš

## Pokretanje
* prije pokretanja scene unutar Unity-a, obavezno pokrenuti Pyhton skripte "**llama_flasky.py**" i "**tts.py**" **UNUTAR** virtualnog okruzenja, zatim pokrenuti scenu
* odabrati objekt avatara i u LLMInteraction script komponenti unijeti tekst i pritisnuti Send
