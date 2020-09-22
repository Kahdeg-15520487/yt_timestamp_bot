from youtube_dl import YoutubeDL
import json

video = "https://www.youtube.com/watch?v=ZGpRM1Cjsjs"
youtube_dl_opts = {}
with YoutubeDL(youtube_dl_opts) as ydl:
      info_dict = ydl.extract_info(video, download=False)

with open('vid_info.json','w') as file:
    file.write(json.dumps(info_dict))
