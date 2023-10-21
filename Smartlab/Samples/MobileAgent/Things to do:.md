Things to do:
1. How to invoke on the basis of Hey Rachel.
2. How to invoke on the basis of vote from cv system
3. test CV pipeline


Port Configuration
BREE
# main.py (python backend)
ChatGPT Response PUB Socket     50001
# Program.cs (psi)
Inital NanoIP Respnse Socket    40001
Send Messages to Agent          40002
Send Audio to Python BE         40003
Send Images to Python BE        40004
Send cvpreds to Python BE       40005
Send tts inv to PythonBE        40006
# Frontend 
Send tts_invocations to PSI     41000


JETSON
# record_audio.py 
remoteIP                        60000
Send audio                      60001
Send doa                        60002
Send vad                        60003
# send_video_dict_witg_embed.py
Send images                     60004



Jetson