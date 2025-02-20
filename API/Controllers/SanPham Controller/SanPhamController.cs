using API.DbConects.DTO.SanPham_DTO;
using API.Services;
using API.Services.SanPham_Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class SanPhamController : ControllerBase
{
    private readonly ISanPhamService _sanPhamService;

    public SanPhamController(ISanPhamService sanPhamService)
    {
        _sanPhamService = sanPhamService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _sanPhamService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _sanPhamService.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] SanPhamDTO sanPhamDto)
    {
        await _sanPhamService.AddAsync(sanPhamDto);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SanPhamDTO sanPhamDto)
    {
        await _sanPhamService.UpdateAsync(id, sanPhamDto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sanPhamService.DeleteAsync(id);
        return Ok();
    }
}
