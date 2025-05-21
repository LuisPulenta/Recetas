using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Web.Data;
using Web.Data.Entities;
using Web.Models.Request;
using Web.Helpers;
using Web.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Recetas.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RecipesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IImageHelper _imageHelper;
        

        public RecipesController(DataContext context, IImageHelper imageHelper)
        {
            _context = context;
            _imageHelper = imageHelper;
        }

        //-----------------------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipes()
        {
            List<Recipe> recipes = await _context.Recipes
                .Include(x => x.Ingredients)
                .Include(x => x.Steps)
              .OrderBy(x => x.Name)
              .ToListAsync();            

            return Ok(recipes);
        }

        
        //-----------------------------------------------------------------------------------
        [HttpPost]
        [Route("CreateRecipe")]
        public async Task<ActionResult<RecipeViewModel>> CreateRecipe(RecipeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = await _context.Users
               .FirstOrDefaultAsync(x => x.Id == request.UserId);

            if (user == null)
            {
                return NotFound();
            }

            DateTime ahora = DateTime.Now;

            string imageId = string.Empty;

            if (request.Image != null && request.Image.Length > 0)
            {
                imageId = _imageHelper.UploadImage(request.Image, "recipes");
            }

            Recipe newRecipe = new Recipe
            {
                Id = 0,
                Name = request.Name,
                Description =request.Description,
                User = user,
                Photo=imageId,
                Ingredients = request.Ingredients?.Select(ingredient => new Ingredient
                {
                    Id = 0,                    
                    Description = ingredient,
                }).ToList(),
                Steps= request.Steps?.Select(step => new Step
                {
                    Id=0,
                    number=step.Number,
                    Description=step.Description,
                }).ToList(),
            };

            _context.Recipes.Add(newRecipe);



            try
            {
                await _context.SaveChangesAsync();
                return new RecipeViewModel
                {
                    Id = newRecipe.Id,
                    Name = newRecipe.Name,
                    UserId=newRecipe.User.Id,
                    UserName=newRecipe.User.FullName,
                    Description = newRecipe.Description,
                    Ingredients = newRecipe.Ingredients?.Select(ingredient => ingredient.Description).ToList(),
                    Steps = newRecipe.Steps?.Select(step => new StepRequest
                    {
                        Number = step.number,
                        Description = step.Description,
                    }).ToList(),
                };
            }
            catch (DbUpdateException dbUpdateException)
            {
                return BadRequest(dbUpdateException.InnerException.Message);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        //-----------------------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            Recipe recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}