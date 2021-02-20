using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using discordbot.Services.DTOs;
using discordbot.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace discordbot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService videoService;

        public VideoController(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        [HttpGet]
        public IEnumerable<VideoDto> ListVideo()
        {
            return videoService.ListVideos();
        }

        [HttpGet("{videoId}")]
        public IActionResult GetVideo(string videoId)
        {
            VideoDto dto = videoService.GetVideo(videoId);
            if (dto == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(dto);
            }
        }
    }
}
