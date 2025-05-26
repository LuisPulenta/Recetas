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
using Org.BouncyCastle.Asn1.Ocsp;

namespace Recetas.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RecipesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IImageHelper _imageHelper;
        private readonly IUserHelper _userHelper;

        public RecipesController(DataContext context, IImageHelper imageHelper, IUserHelper userHelper)
        {
            _context = context;
            _imageHelper = imageHelper;
            _userHelper = userHelper;
        }


        //-----------------------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipes()
        {
            List<Recipe> recipes = await _context.Recipes
                .Include(x => x.Ingredients)
                .Include(x => x.Steps)
                .Include(x => x.User)
              .OrderBy(x => x.Name)
              .ToListAsync();

            List<RecipeViewModel> recipesViewModel = new List<RecipeViewModel>();

            foreach (Recipe recipe in recipes)
            {
                RecipeViewModel recipeViewModel = new RecipeViewModel
                {
                    Id = recipe.Id,
                    Name = recipe.Name,
                    UserId = recipe.User.Id,
                    UserName = recipe.User.FullName,
                    Description = recipe.Description,
                    Photo=recipe.Photo,
                    Ingredients = recipe.Ingredients?.Select(ingredient => ingredient.Description).ToList(),
                    Steps = recipe.Steps?.Select(step => new StepRequest
                    {
                        Number = step.number,
                        Description = step.Description,
                    }).ToList(),
                };
                recipesViewModel.Add(recipeViewModel);
            }

            return Ok(recipesViewModel);
        }

        //-----------------------------------------------------------------------------------
        [HttpGet]
        [Route("GetRecipesByUserId/{id}")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipesByUserId(string id)
        {
            List<Recipe> recipes = await _context.Recipes
                .Include(x => x.Ingredients)
                .Include(x => x.Steps)
                .Include(x => x.User)
                .Where(x => x.User.Id==id)
              .OrderBy(x => x.Name)
              .ToListAsync();

            List<RecipeViewModel> recipesViewModel = new List<RecipeViewModel>();

            foreach (Recipe recipe in recipes)
            {
                RecipeViewModel recipeViewModel = new RecipeViewModel
                {
                    Id = recipe.Id,
                    Name = recipe.Name,
                    UserId = recipe.User.Id,
                    UserName = recipe.User.FullName,
                    Description = recipe.Description,
                    Photo = recipe.Photo,
                    Ingredients = recipe.Ingredients?.Select(ingredient => ingredient.Description).ToList(),
                    Steps = recipe.Steps?.Select(step => new StepRequest
                    {
                        Number = step.number,
                        Description = step.Description,
                    }).ToList(),
                };
                recipesViewModel.Add(recipeViewModel);
            }

            return Ok(recipesViewModel);
        }

        //-----------------------------------------------------------------------------------
        [HttpPost]
        [Route("GetRecipeById/{id}")]
        public async Task<IActionResult> GetRecipeById(int id)
        {
            Recipe recipe = await _context.Recipes
               .Include(x => x.Ingredients)
               .Include(x => x.Steps)
               .Include(x => x.User)
               .FirstOrDefaultAsync(x => x.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            RecipeViewModel recipeViewModel = new RecipeViewModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                UserId = recipe.User.Id,
                UserName = recipe.User.FullName,
                Description = recipe.Description,
                Photo = recipe.Photo,
                Ingredients = recipe.Ingredients?.Select(ingredient => ingredient.Description).ToList(),
                Steps = recipe.Steps?.Select(step => new StepRequest
                {
                    Number = step.number,
                    Description = step.Description,
                }).ToList(),
            };

            return Ok(recipeViewModel);
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
            Recipe recipe = await _context.Recipes
               .Include(x => x.Ingredients)
               .Include(x => x.Steps)
               .FirstOrDefaultAsync(x => x.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();
            }

            foreach (Step step in recipe.Steps)
            {
                _context.Steps.Remove(step);
                await _context.SaveChangesAsync();
            }            

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        //-------------------------------------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipe(int id, RecipeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != request.Id)
            {
                return BadRequest();
            }

            Recipe recipe = await _context.Recipes
              .Include(x => x.Ingredients)
              .Include(x => x.Steps)
              .FirstOrDefaultAsync(x => x.Id == id);

            string imageId = recipe.Photo;

            if (request.Image != null && request.Image.Length > 0)
            {
                imageId = _imageHelper.UploadImage(request.Image, "recipes");
            }

            recipe.Name = request.Name;
            recipe.Description = request.Description;
            recipe.Photo = imageId;

            //Borramos los ingredientes que había en la BD
            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();
            }

            //Borramos los pasos que había en la BD
            foreach (Step step in recipe.Steps)
            {
                _context.Steps.Remove(step);
                await _context.SaveChangesAsync();
            }

            //Grabamos nuevos Ingredientes en recipe
            foreach (string ingredient in request.Ingredients)
            {
                recipe.Ingredients.Add(new Ingredient {
                    Description = ingredient,
                    Recipe = recipe,
                });
            }

            //Grabamos nuevos Steps en recipe
            foreach (StepRequest step in request.Steps)
            {
                recipe.Steps.Add(new Step
                {
                    number= step.Number,
                    Description = step.Description,
                    Recipe = recipe,
                });
            }

            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //-----------------------------------------------------------------------------------
        [HttpPost]
        [Route("MarkAsFavorite/{userId}/recipeId")]
        public async Task<IActionResult> MarkAsFavorite(string userId,int recipeId)
        {
            FavoriteRecipe favoriteRecipe = await _context.FavoriteRecipes
                .FirstOrDefaultAsync(x => x.User.Id == userId && x.Recipe.Id == recipeId);

            if (favoriteRecipe != null) {
                _context.FavoriteRecipes.Remove(favoriteRecipe);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            else
            {
                User user = await _context.Users
               .FirstOrDefaultAsync(x => x.Id == userId);

                Recipe recipe = await _context.Recipes
               .Include(x => x.Ingredients)
               .Include(x => x.Steps)
               .Include(x => x.User)
               .FirstOrDefaultAsync(x => x.Id == recipeId);
                 
                FavoriteRecipe newFavoriteRecipe = new FavoriteRecipe { Id=0, User=user ,Recipe=recipe};
                _context.FavoriteRecipes.Add(newFavoriteRecipe);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

        //-----------------------------------------------------------------------------------
        [HttpPost]
        [Route("FavoritesByUser/{userId}")]
        public async Task<IActionResult> FavoritesByUser(string userId)
        {
            List<FavoriteRecipe> favoriteRecipes = await _context.FavoriteRecipes
                 
                 .Where(x => x.User.Id == userId)
               .ToListAsync();

            List<FavoriteRecipeViewModel> favoriteRecipesViewModel = new List<FavoriteRecipeViewModel>();

            foreach (FavoriteRecipe favoriteRecipe in favoriteRecipes)
            {
                FavoriteRecipeViewModel favoriteRecipeViewModel = new FavoriteRecipeViewModel
                {
                    Id = favoriteRecipe.Id,
                    RecipeId = favoriteRecipe.Recipe.Id,
                    UserId = favoriteRecipe.User.Id
                };
                favoriteRecipesViewModel.Add(favoriteRecipeViewModel);
            }

            return Ok(favoriteRecipesViewModel);

        }
    }
}