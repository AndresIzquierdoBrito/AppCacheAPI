using AppCacheAPI.Data;
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
        public async Task<ActionResult<IEnumerable<IdeaDTO>>> GetIdeas()
        {
            var userId = _userManager.GetUserId(User);

            return await _context.CategoryIdeas
                .Where(ci => ci.Category.UserId == userId && ci.Category.Title == "ALLIDEAS")
                .OrderBy(ci => ci.Order)
                .Select(ci => new IdeaDTO
                {
                    IdeaId = ci.Idea.IdeaId,
                    Title = ci.Idea.Title,
                    Description = ci.Idea.Description,
                    CategoryId = ci.CategoryId,
                    Order = ci.Order
                })
                .ToListAsync();
        }

        [HttpGet("/fromCategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<IdeaDTO>>> GetIdeasFromCategory(int categoryId)
        {
            var userId = _userManager.GetUserId(User);

            return await _context.CategoryIdeas
                .Where(ci => ci.Category.UserId == userId && ci.CategoryId == categoryId)
                .OrderBy(ci => ci.Order)
                .Select(ci => new IdeaDTO
                {
                    IdeaId = ci.Idea.IdeaId,
                    Title = ci.Idea.Title,
                    Description = ci.Idea.Description,
                    CategoryId = ci.CategoryId,
                    Order = ci.Order
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IdeaDTO>> GetIdea(int id)
        {
            var userId = _userManager.GetUserId(User);

            var categoryIdea = await _context.CategoryIdeas
                .Where(ci => ci.Idea.IdeaId == id && ci.Category.UserId == userId)
                .Include(categoryIdea => categoryIdea.Idea)
                .FirstOrDefaultAsync();

            if (categoryIdea == null)
            {
                return NotFound();
            }

            var ideaDTO = new IdeaDTO
            {
                IdeaId = categoryIdea.Idea.IdeaId,
                Title = categoryIdea.Idea.Title,
                Description = categoryIdea.Idea.Description,
                CategoryId = categoryIdea.CategoryId,
                Order = categoryIdea.Order
            };

            return ideaDTO;
        }

        // PUT: api/Ideas/5
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
        public async Task<ActionResult<IdeaDTO>> PostIdea(IdeaDTO ideaDTO)
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

            _context.Ideas.Add(idea);

            //var category = ideaDTO.CategoryId == null
            //    ? await _context.Categories.FirstOrDefaultAsync(c => c.UserId == userId && c.Title == "ALLIDEAS")
            //    : await _context.Categories.FirstOrDefaultAsync(c => c.UserId == userId && c.CategoryId == ideaDTO.CategoryId);


            var category = await _context.Categories
                .Include(c => c.CategoryIdeas)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Title == "ALLIDEAS");

            if (category == null)
            {
                return BadRequest("Category not found");
            }

            var categoryIdea = new CategoryIdea
            {
                CategoryId = category.CategoryId,
                Idea = idea,
                Order = category.CategoryIdeas.Count
            };

            _context.CategoryIdeas.Add(categoryIdea);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving the idea");
            }

            var ideaDTOToReturn = new IdeaDTO
            {
                IdeaId = idea.IdeaId,
                Title = idea.Title,
                Description = idea.Description,
                CategoryId = category.CategoryId,
                Order = categoryIdea.Order
            };

            return CreatedAtAction("GetIdea", new { id = idea.IdeaId }, ideaDTOToReturn);
        }

        [HttpPut("reorder")]
        public async Task<IActionResult> ReorderIdeas([FromBody] List<IdeaDTO> ideas)
        {
            var userId = _userManager.GetUserId(User);

            foreach (var ideaDTO in ideas)
            {
                var categoryIdea = await _context.CategoryIdeas
                    .FirstOrDefaultAsync(ci => ci.IdeaId == ideaDTO.IdeaId && ci.Category.UserId == userId);

                if (categoryIdea == null)
                {
                    return BadRequest();
                }

                categoryIdea.Order = ideaDTO.Order;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }

            return NoContent();
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

        [HttpPut("Order/{categoryId}")]
        public async Task<IActionResult> UpdateOrder(int categoryId, [FromBody] List<Idea> ideas)
        {
            var userId = _userManager.GetUserId(User);

            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null || category.UserId != userId)
            {
                return BadRequest();
            }

            for (int i = 0; i < ideas.Count; i++)
            {
                var categoryIdea = await _context.CategoryIdeas
                    .FirstOrDefaultAsync(ci => ci.CategoryId == categoryId && ci.IdeaId == ideas[i].IdeaId);
                if (categoryIdea == null)
                {
                    return BadRequest();
                }

                categoryIdea.Order = i;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }

            return NoContent();
        }

        private bool IdeaExists(int id)
        {
            return _context.Ideas.Any(e => e.IdeaId == id);
        }
    }
}
