﻿using CMS.Model.Model;
using CMS.Service;
using CMS.Service.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var result = await _userService.Authenticate(login);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var result = await _userService.ForgotPassword(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("ResetPassword/{code}")]
        public IActionResult ResetPassword(string code)
        {
            var result = _userService.GetUserByCode(code);
            return Ok(result);
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
        {
            var result = _userService.ResetPassword(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("Profile")]
        [CMSAuthorize(CheckAccessRight = false)]
        public IActionResult GetProfile()
        {
            var result = _userService.GetProfile();
            return Ok(result);
        }

        [HttpGet("MemberComments")]
        [CMSAuthorize(CheckAccessRight = false)]
        public IActionResult MemberComments()
        {
            return Ok();
        }

        [HttpPut("Profile")]
        [CMSAuthorize(CheckAccessRight = false)]
        public IActionResult UpdateProfile([FromBody] UserProfileModel model)
        {
            var result = _userService.UpdateProfile(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("AddMember")]
        public async Task<IActionResult> AddMember([FromBody] AddMemberModel model)
        {
            var result = await _userService.AddMember(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("ChangePassword")]
        [CMSAuthorize(CheckAccessRight = false)]
        public IActionResult ChangePassword([FromBody] ChangePasswordModel model)
        {
            var result = _userService.ChangePassword(model);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("EmailVerified/{code}")]
        public IActionResult EmailVerified(string code)
        {
            var result = _userService.EmailVerified(code);
            return StatusCode(result.StatusCode, result);
        }
    }
}