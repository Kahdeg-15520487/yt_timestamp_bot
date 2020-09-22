using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace discordbot
{
    internal static class Utility
    {
        internal static string GetYoutubeUrl(string videoId)
        {
            return "https://youtube.com/watch?v=" + videoId;
        }

        internal static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        internal static string ToTimeStamp(double time)
        {
            return TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss");
        }

        internal static object GetYoutubeUrlWithTime(string videoId, double time)
        {
            return "https://youtube.com/watch?v=" + videoId + "&t=" + (int)time;
        }

        internal static T ToEnum<T>(this string s) where T : struct, Enum
        {
            if (Enum.TryParse(s, out T result))
            {
                return result;
            }
            else throw new InvalidDataException("s");
        }
    }
}
