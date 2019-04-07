using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClubPlay_Backend.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ClubPlay_Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly clubplayContext _context;

        public GamesController(clubplayContext context)
        {
            _context = context;
        }

        // GET: api/Games
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetGame()
        {
            return await _context.Game.ToListAsync();
        }

        // GET: api/Games/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            var game = await _context.Game.FindAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            return game;
        }

        // GET: api/Games/ready
        [HttpGet("ready")]
        public async Task<ActionResult<Game>> GetWaitingGame([FromHeader]string token)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var playerId = Convert.ToInt32(claimsIdentity.FindFirst(ClaimTypes.Name)?.Value);

            var player = await _context.Player.FirstOrDefaultAsync(c => c.Id == playerId);
            if (player == null)
            {
                return BadRequest("No such player with attach JWT");
            }


            var now = DateTime.UtcNow;
            var tokenDB = await _context.Token.FirstOrDefaultAsync(t => t.Value==token && DateTime.Compare(t.ExpireAt, now) > 0);
            if (tokenDB == null)
            {
                return BadRequest("Invalid game token, please contact your token provider");
            }

            var game = await _context.Game.FirstOrDefaultAsync(g => ((g.Player1Id == playerId) || (g.Player2Id == playerId)) && (g.ScorePlayer1==null) && (g.ScorePlayer2==null));
            if (game == null)
            {
                return NotFound();
            }

            return game;
        }

        // PUT: api/Games/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGame(int id, Game game)
        {
            if (id != game.Id)
            {
                return BadRequest();
            }

            var player1 = await _context.Player.FindAsync(game.Player1Id);
            var player2 = await _context.Player.FindAsync(game.Player2Id);
            if(player1 == null)
            {
                return BadRequest("Player1 with id = " + game.Player1Id + " does not exist");
            }
            if (player2 == null)
            {
                return BadRequest("Player2 with id = " + game.Player2Id + " does not exist");
            }

            _context.Entry(game).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(game);
        }

        // POST: api/Games
        [HttpPost]
        public async Task<ActionResult<Game>> PostGame([FromHeader]string token, [FromBody]Game game)
        {
            // Verify JWT
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var playerId = Convert.ToInt32(claimsIdentity.FindFirst(ClaimTypes.Name)?.Value);

            var player = await _context.Player.FirstOrDefaultAsync(p => p.Id == playerId);
            if (player == null)
            {
                return BadRequest("No such player with attached JWT");
            }

            if (game.Player1Id != playerId)
            {
                return BadRequest("Player1Id must match JWT player id = " + playerId);
            }

            // Verify token
            var tokenDB = await _context.Token.FirstOrDefaultAsync(t => t.Value == token);
            if (tokenDB == null)
            {
                return BadRequest("Invalid game token");
            }

            DateTime now = DateTime.UtcNow;
            if (DateTime.Compare(tokenDB.ExpireAt, now) <= 0)
            {
                return BadRequest("token " + token + " expired");
            }

            // Check if a game with player id already exisits
            var gameDb = await _context.Game.FirstOrDefaultAsync(g => g.Player1Id == game.Player1Id && g.Player2Id == null);
            if (gameDb != null)
            {
                return Ok(gameDb);
            }

            _context.Game.Add(game);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGame", new { id = game.Id }, game);
        }

        // DELETE: api/Games/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Game>> DeleteGame(int id)
        {
            var game = await _context.Game.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            _context.Game.Remove(game);
            await _context.SaveChangesAsync();

            return game;
        }

        private bool GameExists(int id)
        {
            return _context.Game.Any(e => e.Id == id);
        }
    }
}
