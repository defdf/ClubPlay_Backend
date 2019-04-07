using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClubPlay_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using ClubPlay_Backend.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace ClubPlay_Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClubsController : ControllerBase
    {
        private readonly clubplayContext _context;
        private readonly AppSettings _appSettings;

        public ClubsController(clubplayContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        // GET: api/Clubs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Club>>> GetClub()
        {
            return await _context.Club.ToListAsync();
        }

        // GET: api/Clubs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Club>> GetClub(int id)
        {
            var club = await _context.Club.FindAsync(id);

            if (club == null)
            {
                return NotFound();
            }

            return club;
        }

        // PUT: api/Clubs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClub(int id, Club club)
        {
            if (id != club.Id)
            {
                return BadRequest();
            }

            _context.Entry(club).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClubExists(id))
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

        // POST: api/Clubs
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Club>> PostClub([FromBody]Club club)
        {
            _context.Club.Add(club);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClub", new { id = club.Id }, club);
        }

        // DELETE: api/Clubs/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Club>> DeleteClub(int id)
        {
            var club = await _context.Club.FindAsync(id);
            if (club == null)
            {
                return NotFound();
            }

            _context.Club.Remove(club);
            await _context.SaveChangesAsync();

            return club;
        }

        // POST: api/clubs/id/authenticate
        [AllowAnonymous]
        [HttpPut("authenticate")]
        public IActionResult Authenticate([FromBody]Credential cred)
        {
            var club = _context.Club.SingleOrDefault(p => p.Email == cred.Email && p.Password == cred.Password);
            if (club == null)
            {
                return BadRequest("Email or password is incorrect.");
            }

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, club.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // remove password before returning
            club.Password = null;

            HttpContext.Response.Headers.Add("jwt", tokenHandler.WriteToken(token));

            return Ok(club);
        }

        [HttpGet("{id}/token")]
        public async Task<IActionResult> GetToken([FromRoute]int id)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var clubId = Convert.ToInt32(claimsIdentity.FindFirst(ClaimTypes.Name)?.Value);

            if (id != clubId)
            {
                return BadRequest("You are not allowed to access this club with id  = " + id);
            }

            var club = await _context.Club.FirstOrDefaultAsync(c => c.Id == clubId);
            if (club == null)
            {
                return BadRequest("No such club with attach JWT");
            }

            var now = DateTime.UtcNow;

            var token = await _context.Token.FirstOrDefaultAsync(p => p.ClubId == clubId && DateTime.Compare(p.ExpireAt, now) > 0);
            if (token != null)
            {
                return Ok(token.Value);
            }

            // Create new token
            var value = GenerateToken(clubId, now);
            var expireAt = now.AddDays(1);
            var newToken = new Token
            {
                Value = value,
                ExpireAt = expireAt,
                ClubId = clubId
            };

            _context.Token.Add(newToken);
            await _context.SaveChangesAsync();
            return Ok(newToken.Value);
        }

        // GET: api/clubs/id/subscriptions
        [HttpGet("{id}/subscriptions")]
        public async Task<ActionResult<IEnumerable<Subscription>>> GetSubscriptions([FromRoute]int id)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var clubId = Convert.ToInt32(claimsIdentity.FindFirst(ClaimTypes.Name)?.Value);

            if (id != clubId)
            {
                return Forbid("You are not allowed to access this club with id " + id);
            }

            var club = await _context.Club.FirstOrDefaultAsync(c => c.Id == clubId);
            if (club == null)
            {
                return BadRequest("No such club with attach JWT");
            }

            return await _context.Subscription.Where(s => s.ClubId == clubId).ToListAsync();
        }

        // GET: api/clubs/id_club/subscriptions/current
        [AllowAnonymous]
        [HttpGet("{id_club}/subscriptions/current")]
        public async Task<ActionResult<bool>> GetCurrentSubscription([FromRoute]int id_club)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var clubId = Convert.ToInt32(claimsIdentity.FindFirst(ClaimTypes.Name)?.Value);

            if (id_club != clubId)
            {
                return BadRequest("You are not allowed to access this club with id = " + id_club);
            }

            var club = await _context.Club.FirstOrDefaultAsync(c => c.Id == clubId);
            if (club == null)
            {
                return BadRequest("No such club with attach JWT");
            }

            DateTime now = DateTime.UtcNow;
            var current_sub =await _context.Subscription.FirstOrDefaultAsync(s => s.ClubId == id_club && DateTime.Compare(s.ExpireAt, now) > 0);
            if (current_sub != null)
            {
                return Ok(current_sub);
            }
            return NotFound();
        }

        // POST: api/Clubs/subscriptions
        [HttpPost("{id_club}/subscriptions")]
        public async Task<ActionResult<Club>> Post([FromRoute]int id_club, [FromBody]SubscriptionParam subscriptionParam)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var clubId = Convert.ToInt32(claimsIdentity.FindFirst(ClaimTypes.Name)?.Value);

            if (id_club != clubId)
            {
                return BadRequest("You are not allowed to access this club with id = " + id_club);
            }

            var club = await _context.Club.FirstOrDefaultAsync(c => c.Id == clubId);
            if (club == null)
            {
              return BadRequest("No such club with attach JWT");
            }

            if (clubId != subscriptionParam.ClubId)
            {
            return BadRequest("Subscription's and current JWT'S ClubId do not match");
            }

            DateTime now = DateTime.UtcNow;
            DateTime expireAt = now;
            float payment = 0F;
            var month = subscriptionParam.Month;
            if (month == 1)
            {
                expireAt = now.AddMonths(1);
                payment = 9.9F;
            }
            if (month == 12)
            {
                payment = 90F;
                expireAt = now.AddYears(1);
            }

            var subscription = new Subscription
            {
                ClubId = subscriptionParam.ClubId,
                Payment = payment,
                ExpireAt = expireAt
            };

            _context.Subscription.Add(subscription);
            await _context.SaveChangesAsync();

            return Ok(subscription);
        }

        private bool ClubExists(int id)
        {
            return _context.Club.Any(e => e.Id == id);
        }

        private string GenerateToken(int id, DateTime todayDate)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var randomString= new string(Enumerable.Repeat(chars, 9)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var date = todayDate.ToString("dd/MM/yyyy");
            return id + randomString + date;
        }
    }
}
