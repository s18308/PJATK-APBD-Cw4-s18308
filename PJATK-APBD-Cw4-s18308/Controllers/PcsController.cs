using PJATK_APBD_Cw4_s18308.DTOs;
using PJATK_APBD_Cw4_s18308.Exceptions;
using PJATK_APBD_Cw4_s18308.Services;
using Microsoft.AspNetCore.Mvc;

namespace PJATK_APBD_Cw4_s18308.Controllers;

[ApiController]
[Route("api/pcs")]
public class PcsController(IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllPcs(CancellationToken cancellationToken)
    {
        var result = await service.GetAllPcsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/components")]
    public async Task<IActionResult> GetPcWithComponents([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetPcWithComponentsAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddPc([FromBody] PcRequest request, CancellationToken cancellationToken)
    {
        var result = await service.AddPcAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPcWithComponents), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePc([FromRoute] int id, [FromBody] PcRequest request, CancellationToken cancellationToken)
    {
        await service.UpdatePcAsync(id, request, cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePc([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            await service.DeletePcAsync(id, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
