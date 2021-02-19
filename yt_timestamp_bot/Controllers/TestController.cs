using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using discordbot.Services.DTOs;
using discordbot.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace discordbot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ITagService tagService;

        public TestController(ITagService tagService)
        {
            this.tagService = tagService;
        }

        [HttpGet("{videoId}")]
        public IEnumerable<TimeStampDto> Get(string videoId)
        {
            return tagService.ListTag(videoId);
        }
    }
}
