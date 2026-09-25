using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiCentralDocsWeb.Data;

namespace ApiCentralDocsWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoDocumentoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TipoDocumentoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tipos = await _context.TiposDocumento.ToListAsync();
            return Ok(tipos);
        }
    }
}