using System.Threading;
using System.Threading.Tasks;
using LineItem.Exceptions;
using LineItem.Models;
using LineItem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LineItem.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] UserModel user, CancellationToken cancellationToken)
    {
        try
        {
            var createdUser = await _userService.CreateAsync(user, cancellationToken);
            return CreatedAtAction(
                nameof(GetByIdAsync),
                new { version = "1", id = createdUser.Id },
                createdUser
            );
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await _userService.RetrieveByIdAsync(id, cancellationToken);

        return result is not null
            ? Ok(result)
            : NotFound($"User with Id = {id} not found!");
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] UserModel user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var updatedUser = await _userService.UpdateAsync(id, user, cancellationToken);

            return updatedUser is not null
                ? Ok(updatedUser)
                : NotFound($"User with Id = {id} not found!");
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}