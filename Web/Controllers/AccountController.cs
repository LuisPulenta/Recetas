﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Web.Helpers;
using Web.Models;
using Web.Data.Entities;
using Common.Enums;
using Microsoft.AspNetCore.Identity;
using Common.Models;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(IUserHelper userHelper, IImageHelper imageHelper, IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _imageHelper = imageHelper;
            _mailHelper = mailHelper;
        }

        //----------------------------------------------------------------------------------------
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            return View(new LoginViewModel());
        }

        //----------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(Request.Query["ReturnUrl"].First());
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            }

            return View(model);
        }

        //----------------------------------------------------------------------------------------
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        //----------------------------------------------------------------------------------------
        public IActionResult NotAuthorized()
        {
            return View();
        }

        //----------------------------------------------------------------------------------------
        public IActionResult Register()
        {
            AddUserViewModel model = new AddUserViewModel
            {
            };

            return View(model);
        }

        //----------------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imageId = string.Empty;

                if (model.ImageFile != null)
                {
                    imageId = await _imageHelper.UploadImageAsync(model.ImageFile, "users");
                }

                model.Modulo="Avon";

                User user = await _userHelper.AddUserAsync(model, imageId, UserType.User);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Este correo ya está siendo usado por otro usuario.");
                    return View(model);
                }

                
                //LoginViewModel loginViewModel = new LoginViewModel
                //{
                //    Password = model.Password,
                //    RememberMe = false,
                //    Username = model.Username
                //};

                //var result2 = await _userHelper.LoginAsync(loginViewModel);

                //if (result2.Succeeded)
                //{
                //    return RedirectToAction("Index", "Home");
                //}
                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                string tokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userid = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendMail(model.Username, "Confirmación de cuenta", $"<h1>Confirmación de cuenta</h1>" +
                    $"Para habilitar el usuario, " +
                    $"por favor hacer clic en el siguiente enlace: </br></br><a href = \"{tokenLink}\">Confirmar Email</a>");
                if (response.IsSuccess)
                {
                    ViewBag.Message = "Las instrucciones para habilitar su cuenta han sido enviadas al correo.";
                    return View(model);
                }
                ModelState.AddModelError(string.Empty, response.Message);

            }
            return View(model);
        }

        //----------------------------------------------------------------------------------------
        public async Task<IActionResult> ChangeUser()
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            EditUserViewModel model = new()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Photo = user.Photo,
                Id = user.Id,
                Document = user.Document,
            };

            return View(model);
        }

        //----------------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imageId = model.Photo;

                if (model.ImageFile != null)
                {
                    imageId = await _imageHelper.UploadImageAsync(model.ImageFile, "users");
                }

                User user = await _userHelper.GetUserAsync(User.Identity.Name);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Photo = imageId;
                user.Document = model.Document;
                await _userHelper.UpdateUserAsync(user);
                return RedirectToAction("Index", "Home");
            }

                    return View(model);
        }

        //----------------------------------------------------------------------------------------
        public IActionResult ChangePassword()
        {
            return View();
        }

        //----------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(User.Identity.Name);
                if (user != null)
                {
                    IdentityResult result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(ChangeUser));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                }
            }

            return View(model);
        }

        //----------------------------------------------------------------------------------------
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            User user = await _userHelper.GetUserAsync(new Guid(userId));
            if (user == null)
            {
                return NotFound();
            }

            IdentityResult result = await _userHelper.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return NotFound();
            }
            return View();
        }

        //----------------------------------------------------------------------------------------
        public IActionResult RecoverPassword()
        {
            return View();
        }

        //----------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "El correo ingresado no corresponde a ningún usuario.");
                    return View(model);
                }

                string myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                string link = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = myToken }, protocol: HttpContext.Request.Scheme);
                _mailHelper.SendMail(model.Email, "Reseteo de contraseña", $"<h1>Reseteo de contraseña</h1>" +
                    $"Para establecer una nueva contraseña haga clic en el siguiente enlace:</br></br>" +
                    $"<a href = \"{link}\">Cambio de Contraseña</a>");
                ViewBag.Message = "Las instrucciones para el cambio de contraseña han sido enviadas a su email.";
                return View();

            }

            return View(model);
        }

        //----------------------------------------------------------------------------------------
        public IActionResult ResetPassword(string token)
        {
            return View();
        }

        //----------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            User user = await _userHelper.GetUserAsync(model.UserName);
            if (user != null)
            {
                IdentityResult result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    ViewBag.Message = "Contraseña cambiada.";
                    return View();
                }

                ViewBag.Message = "Error cambiando la contraseña.";
                return View(model);
            }

            ViewBag.Message = "Usuario no encontrado.";
            return View(model);
        }
    }
}
