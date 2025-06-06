﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Entities;
using Web.Helpers;
using Web.Models;
using Web.Data;
using Web.Models.Request;
using Common.Enums;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Common.Models;

namespace RecetasApi.Àpi.Controllers.Àpi
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IMailHelper _mailHelper;
        private readonly IImageHelper _imageHelper;


        public AccountController(IUserHelper userHelper, IConfiguration configuration, DataContext context, IMailHelper mailHelper, IImageHelper imageHelper)
        {
            _userHelper = userHelper;
            _configuration = configuration;
            _context = context;
            _mailHelper = mailHelper;
            _imageHelper = imageHelper;

        }

        //---------------------------------------------------------------------------------
        [HttpPost]
        [Route("CreateToken")]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(model.Username);
                if (user != null)
                {
                    var result = await _userHelper.ValidatePasswordAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        Claim[] claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        JwtSecurityToken token = new JwtSecurityToken(
                            _configuration["Tokens:Issuer"],
                            _configuration["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(99),
                            signingCredentials: credentials);
                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                            user
                        };

                        return Created(string.Empty, results);
                    }
                }
            }

            return BadRequest();
        }

        //---------------------------------------------------------------------------------
        [HttpPost]
        [Route("GetUserByEmail")]
        public async Task<IActionResult> GetUser(EmailRequest email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _context.Users.FirstOrDefaultAsync(o => o.Email.ToLower() == email.Email.ToLower());

            if (user == null)
            {
                return BadRequest("El Usuario no existe.");
            }

            return Ok(user);
        }
        
        //---------------------------------------------------------------------------------
        [HttpPost]
        [Route("CreateUser")]
        public async Task<ActionResult<User>> PostUser(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = await _userHelper.GetUserAsync(request.Email);
            if (user != null)
            {
                return BadRequest("Ya existe un usuario registrado con  ese email.");
            }

            string imageId = string.Empty;

            if (request.Image != null && request.Image.Length > 0)
            {
                imageId = _imageHelper.UploadImage(request.Image, "users");
            }

            user = new User
            {
                Document = request.Document,
                Email = request.Email,
                FirstName = request.FirstName,
                Photo = imageId,
                LastName = request.LastName,
                UserName = request.Email,
                UserType = UserType.User,
            };

            try
            {
                await _userHelper.AddUserAsync(user, request.Password);
                await _userHelper.AddUserToRoleAsync(user, user.UserType.ToString());

                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                string tokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userid = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                _mailHelper.SendMail(user.Email, "Confirmación de Email",
                   $"<table style = 'max-width: 800px; padding: 10px; margin:0 auto; border-collapse: collapse;'>" +
                   $"  <tr>" +
                   $"    <td style = 'background-color: #3658a8; text-align: center; padding: 0'>" +
                                      $"  <td style = 'padding: 0'>" +
                   $"<tr>" +
                   $" <td style = 'background-color: #ecf0f1'>" +
                   $"      <div style = 'color: #3658a8; margin: 4% 10% 2%; text-align: justify;font-family: sans-serif'>" +
                   $"            <h1 style = 'color: #e67e22; margin: 0 0 7px' > Cookbook </h1>" +
                   $"                    <p style = 'margin: 2px; font-size: 15px'>" +
                   $"    <h2 style = 'color: #e67e22; margin: 0 0 5px' >Confirmación de Email</h2>" +
                   $"    Para habilitar el usuario, por favor hacer clic en el siguiente enlace: </ br ></ br > " +
                   $"     </ br ></ br > " +
                   $"    <a style ='text-decoration: none; border-radius: 5px; padding: 5px 5px; color: white; background-color: #3658a8' href = \"{tokenLink}\">Confirmar Email</a>" +
                   $"    <p style = 'color: #b3b3b3; font-size: 12px; text-align: center;margin: 10px 0 0' > Keypress Software</p>" +
                   $"  </div>" +
                   $" </td >" +
                   $"</tr>" +
                   $"</table>");
         
                await _context.SaveChangesAsync();

                return Ok(user);
            }
            catch (Exception)
            {
                return BadRequest("Ya existe un usuario registrado con  ese DNI.");
            }
        }

        //---------------------------------------------------------------------------------
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response
                {
                    IsSuccess = false,
                    Message = "Bad request",
                    Result = ModelState
                });
            }


            User user = await _userHelper.GetUserAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new Response
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado"
                });
            }

            IdentityResult result = await _userHelper.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new Response
                {
                    IsSuccess = false,
                    Message = result.Errors.FirstOrDefault().Description
                });
            }

            return Ok(new Response
            {
                IsSuccess = true,
                Message = "El password fue cambiado con éxito."
            });
        }

        //---------------------------------------------------------------------------------
        [HttpPost]
        [Route("RecoverPassword")]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(model.Email);
                if (user == null)
                {
                    return BadRequest("El correo ingresado no corresponde a ningún usuario.");
                }

                await _context.SaveChangesAsync();

                string myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                string link = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = myToken }, protocol: HttpContext.Request.Scheme);
                _mailHelper.SendMail(model.Email, "Reseteo de contraseña", $"<h1>Reseteo de contraseña</h1>" +
                    $"Para establecer una nueva contraseña haga clic en el siguiente enlace:</br></br>" +
                    $"<a href = \"{link}\">Cambio de Contraseña</a>");
                return Ok("Las instrucciones para el cambio de contraseña han sido enviadas a su email.");
            }

            return BadRequest(model);
        }

        //-------------------------------------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, UserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id.ToString() != request.Id)
            {
                return BadRequest();
            }

            User user = await _userHelper.GetUserAsync(request.Email);
            if (user == null)
            {
                return BadRequest("No existe el usuario.");
            }

            string imageId = user.Photo;

            if (request.Image != null && request.Image.Length > 0)
            {
                imageId = _imageHelper.UploadImage(request.Image, "users");
            }

            user.Document = request.Document;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Photo = imageId;

            await _userHelper.UpdateUserAsync(user);
            return NoContent();
        }
    }
}