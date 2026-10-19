using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RA_MuniAnnouncements.Models;
using RA_MuniAnnouncements.Services;

namespace RA_MuniAnnouncements.Controllers;

[ApiController]
[Route("api/my-requests")]
[Authorize]
public sealed class MyRequestsController(MyRequestsService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<MyRequestsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MyRequestsResponse>> Get(CancellationToken cancellationToken)
    {
        var idNumber = User.FindFirstValue("idNumber");
        if (string.IsNullOrWhiteSpace(idNumber))
        {
            return Unauthorized();
        }

        return Ok(await service.GetRequestsAsync(idNumber, cancellationToken));
    }
}