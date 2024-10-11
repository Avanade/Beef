#nullable enable

using CoreEx.Results;

namespace Beef.Demo.Api.Controllers
{
    public partial class PersonController
    {
        /// <summary>
        /// Extend Response.
        /// </summary>
        /// <returns>A resultant <c>string</c>.</returns>
        [HttpPost("api/v1/persons/extend-response", Name = "Person_ExtendResponse")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public Task<IActionResult> ExtendResponse(string? name) => _webApi.PostWithResultAsync(Request, p => 
            Result.GoAsync(() => _manager.ExtendResponseAsync(name))         // Execute the business logic.
                .ThenAs(r =>                                                 // Then handle response where Result.IsSuccess; otherwise, allow any error/exceptions to bubble out.
                {
                    var ia = p.CreateActionResult(r, HttpStatusCode.OK);     // Use standard CoreEx to create a ValueContentResult with an OK status to handle the response.
                    if (ia is ValueContentResult vcr)                        // Ensure is a ValueContentResult, and if so, then manipulate.
                    {
                        vcr.BeforeExtension = hr =>                          // Extend the reponse by adding a new header.
                        {
                            hr.Headers.Add("X-Beef-Test", "123");
                            return Task.CompletedTask;
                        };
                    }

                    return ia;
                }), alternateStatusCode: HttpStatusCode.NoContent, operationType: CoreEx.OperationType.Unspecified);
    }
}

#nullable restore