using Application.Features.Documents.Commands.UploadDocument;
using Application.Features.Documents.Query.GetUserDocuments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/documents")]
    public class DocumentController : ApiController
    {
        [RequestSizeLimit(50 * 1024 * 1024)]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentCommand command)
        {
            var result = await Sender.Send(command);

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok(result.Value);
        }


        [HttpGet("my-document")]
        public async Task<IActionResult> GetUserDocuments([FromQuery] DocumentQueryParameters parameters)
        {
            var query = new GetUserDocumentsQuery
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
    }
}
