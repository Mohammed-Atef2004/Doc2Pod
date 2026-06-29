using Application.Features.Podcasts.Commands.GeneratePodcast;
using Application.Features.Podcasts.Query.GetAllPodcasts;
using Application.Features.Podcasts.Query.GetPodcast;
using Application.Features.Podcasts.Query.GetPodcastDetails;
using Application.Features.Podcasts.Query.GetPodcastStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/podcast")]
    public class PodcastController : ApiController
    {

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GeneratePodcastCommand command)
        {
            var result = await Sender.Send(command);

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok(result.Value);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetAudio(Guid id)
        {
            var result = await Sender.Send(new GetPodcastQuery(id));

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }
            return Ok(new { audioUrl = result.Value });
        }



        [HttpGet("my-podcasts")]
        [Authorize]
        public async Task<IActionResult> GetUserPodcasts([FromQuery] PodcastQueryParameters parameters)
        {
            var query = new GetUserPodcastsQuery
            {
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                SortBy = parameters.SortBy,
                SortDirection = parameters.SortDirection,
                SearchTerm = parameters.SearchTerm
            };

            var result = await Sender.Send(query);
            return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
        }

        [HttpGet("current-status")]
        public async Task<IActionResult> GetCurrentStatus()
        {
            var result = await Sender.Send(new GetPodcastStatusQuery());

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok(result.Value);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetPodcastDetails(Guid id)
        {
            var result = await Sender.Send(new GetPodcastDetailsQuery(id));

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok(result.Value);
        }
    }
}