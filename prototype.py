import requests

def get_vID(API_KEY, channelID):
    '''
     Use "search" function to derive upcomming/live livestream
    '''
    params = {
            'part': 'id',
            'key': API_KEY,
            'channelId': channelID,
            'eventType': 'upcoming',
            'type': 'video',
            'order': 'viewCount',
            'fields': 'items(id(videoId))'
            }
    
    url = 'https://www.googleapis.com/youtube/v3/search'
    req = requests.get(url, headers=None, params=params).json()
    req_res = req.get('items')
    # if cannot find upcomming streams, search for on-going (live) stream 
    if len(req_res) == 0:
        params.update({'eventType':'live'})
        req = requests.get(url, headers=None, params=params).json()
        req_res = req.get('items')
    
    vID = req_res[0].get('id').get('videoId')
    
    return vID
    
    
def get_livestream_info(API_KEY, channelID, vID):
    '''
     Use "videos" function to derive info of livestream 
    '''
    params = {
              'part': 'liveStreamingDetails,statistics,snippet',
              'key': API_KEY,
              'id': vID,
              'fields': 'items(id,liveStreamingDetails(activeLiveChatId,concurrentViewers,scheduledStartTime,actualStartTime),' + 
              'snippet(channelId,channelTitle,description,liveBroadcastContent,publishedAt,thumbnails,title),statistics)'
              }
    
    url = 'https://www.googleapis.com/youtube/v3/videos'
    req = requests.get(url, headers=None, params=params).json()
    
    streamData = dict(req.get('items')[0])
    
    # print output

    
    return streamData

# === Test ===
# supply API key and Channel ID
API_KEY = 'XXX'
channelID = 'UC-lHJZR3Gqxm24_Vd_AJ5Yw'

# get vid
vid = get_vID(API_KEY,channelID)
# get stream info
sinfo = get_livestream_info(API_KEY,channelID, vid)

# print output
print('Channel: ', sinfo['snippet']['channelTitle'])
print('Title: ', sinfo['snippet']['title'])
print('Video ID: ', sinfo['id'])
print('Stream Created: ', sinfo['snippet']['publishedAt'])
print('Start Time (Plan): ', sinfo['liveStreamingDetails']['scheduledStartTime'])
if "actualStartTime" in sinfo['liveStreamingDetails']:
    print('Start Time (Actual): ', sinfo['liveStreamingDetails']['actualStartTime'])