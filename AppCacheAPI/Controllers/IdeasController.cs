﻿using AppCacheAPI.Data;
using AppCacheAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppCacheAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class IdeasController : ControllerBase
    {
        private readonly AppCacheDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IdeasController(AppCacheDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Ideas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Idea>>> GetIdeas()
        {
            var userId = _userManager.GetUserId(User);

            return await _context.Ideas.Where(i => i.UserId == userId).ToListAsync();
        }

        // GET: api/Ideas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Idea>> GetIdea(int id)
        {
            var idea = await _context.Ideas.FindAsync(id);

            if (idea == null)
            {
                return NotFound();
            }

            return idea;
        }

        // PUT: api/Ideas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIdea(int id, Idea idea)
        {
            if (id != idea.IdeaId)
            {
                return BadRequest();
            }

            _context.Entry(idea).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IdeaExists(id))
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

      // POST: api/Ideas
        [HttpPost]
        public async Task<ActionResult<Idea>> PostIdea(IdeaDTO ideaDTO)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized("User is not authenticated");
            }

            var idea = new Idea
            {
                Title = ideaDTO.Title,
                Description = ideaDTO.Description,
                UserId = userId
            };

            try
            {
                _context.Ideas.Add(idea);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving the idea");
            }

            return CreatedAtAction("GetIdea", new { id = idea.IdeaId }, idea);
        }

        // DELETE: api/Ideas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIdea(int id)
        {
            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null)
            {
                return NotFound();
            }

            _context.Ideas.Remove(idea);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IdeaExists(int id)
        {
            return _context.Ideas.Any(e => e.IdeaId == id);
        }
    }
}
