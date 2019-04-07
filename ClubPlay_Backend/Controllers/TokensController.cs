using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClubPlay_Backend.Models;

namespace ClubPlay_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private readonly clubplayContext _context;

        public TokensController(clubplayContext context)
        {
            _context = context;
        }

        // GET: api/Tokens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Token>>> GetToken()
        {
            return await _context.Token.ToListAsync();
        }

        // GET: api/Tokens/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Token>> GetToken(int id)
        {
            var token = await _context.Token.FindAsync(id);

            if (token == null)
            {
                return NotFound();
            }

            return token;
        }

        // PUT: api/Tokens/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutToken(int id, Token token)
        {
            if (id != token.Id)
            {
                return BadRequest();
            }

            _context.Entry(token).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TokenExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tokens
        [HttpPost]
        public async Task<ActionResult<Token>> PostToken(Token token)
        {
            _context.Token.Add(token);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetToken", new { id = token.Id }, token);
        }

        // DELETE: api/Tokens/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Token>> DeleteToken(int id)
        {
            var token = await _context.Token.FindAsync(id);
            if (token == null)
            {
                return NotFound();
            }

            _context.Token.Remove(token);
            await _context.SaveChangesAsync();

            return token;
        }

        private bool TokenExists(int id)
        {
            return _context.Token.Any(e => e.Id == id);
        }
    }
}
