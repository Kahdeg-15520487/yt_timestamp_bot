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
    public class TagController : ControllerBase
    {
        class BulkTagAction
        {
            public DateTime time { get; set; }
            public string content { get; set; }
        }
        private readonly ITagService tagService;

        public TagController(ITagService tagService)
        {
            this.tagService = tagService;
        }

        [HttpGet("{videoId}")]
        public IEnumerable<TimeStampDto> Get(string videoId)
        {
            return tagService.ListTag(videoId);
        }

        //[HttpPost("{videoId}")]
        //public string Post(string videoId)
        //{
        //    tagService.ad
        //}

        //[HttpPost("bulk")]
        //public async Task<string> Upload()
        //{
        //    using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
        //    {
        //        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(await reader.ReadLineAsync());
        //        string[] tagAction = (await reader.ReadToEndAsync()).Split(Environment.NewLine).ToArray();
        //        List<BulkTagAction> action = new List<BulkTagAction>();
        //        for (int i = 0; i < tagAction.Length; i += 2)
        //        {
        //            action.Add(new BulkTagAction()
        //            {
        //                time = TimeZoneInfo.ConvertTimeToUtc(,
        //            });
        //        }
        //    }
        //}
    }
}
