using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;
using Amazon.AspNetCore.Identity.Cognito;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;
        public AccountsController(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }

        public async Task<IActionResult> Signup()
        {
            var model = new SignupModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExist", "User already exist");
                    return View(model);
                }
                user.Attributes.Add(CognitoAttribute.Name.AttributeName, model.Email);
                try
                {
                    var createdUser = await _userManager.CreateAsync(user, model.Password);
                    if (createdUser.Succeeded)
                        RedirectToAction("Confirm");
                }
                catch (Exception ex)
                {
                    _ = ex.Message;
                }
                
                
            }
            return View(model);
        }
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            return View(model);
        }
        [HttpPost]
        [ActionName("Confirm")]
        public async Task<IActionResult> Confirm_Post(ConfirmModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user == null)
                {
                    ModelState.AddModelError("Not Found", "A user with given email adress is not found");
                    return View(model);
                }

                var result = await _userManager.ConfirmEmailAsync(user, model.Code);
                if(result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                }
            }
            return View(model);
        }
    }
}