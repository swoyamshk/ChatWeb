using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Controllers
{

        [ApiController]
        [Route("api/[controller]")]
        public class MessagesController : ControllerBase
        {
            private readonly AuthDbContext _context;

            public MessagesController(AuthDbContext context)
            {
                _context = context;
            }

            // GET: api/Messages
            [HttpGet]
            public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
            {
                return await _context.Messages.OrderBy(m => m.Timestamp).ToListAsync();
            }

            // GET: api/Messages/5
            [HttpGet("{id}")]
            public async Task<ActionResult<Message>> GetMessage(int id)
            {
                var message = await _context.Messages.FindAsync(id);

                if (message == null)
                {
                    return NotFound();
                }

                return message;
            }

            // POST: api/Messages
            [HttpPost]
            public async Task<ActionResult<Message>> PostMessage(Message message)
            {
                message.Timestamp = DateTime.UtcNow;
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
            }

            // PUT: api/Messages/5
            [HttpPut("{id}")]
            public async Task<IActionResult> PutMessage(int id, Message message)
            {
                if (id != message.Id)
                {
                    return BadRequest();
                }

                _context.Entry(message).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(id))
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

            // DELETE: api/Messages/5
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteMessage(int id)
            {
                var message = await _context.Messages.FindAsync(id);
                if (message == null)
                {
                    return NotFound();
                }

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            private bool MessageExists(int id)
            {
                return _context.Messages.Any(e => e.Id == id);
            }
        }
    }
