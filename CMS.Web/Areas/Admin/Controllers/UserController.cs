﻿using CMS.Service;
using CMS.Service.Attributes;
using CMS.Service.Infrastructure;
using CMS.Storage.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Route("admin/user")]
        [CMSAuthorize(IsView = true)]
        public IActionResult Index() => View();

        [HttpPost("admin/user/list")]
        [CMSAuthorize(RouteLevel = 3)]
        public IActionResult Get(UserFilterModel filter)
        {
            var response = PaginationHelper.CreatePagedReponse(_userService.Get(filter), filter);
            return Ok(response);
        }        

        [HttpGet("admin/user/{id}")]
        [CMSAuthorize(RouteLevel = 2)]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _userService.GetById(id);
            return Ok(result);
        }

        [HttpPost("admin/user")]
        [CMSAuthorize(RouteLevel = 2)]
        public async Task<IActionResult> Post(UserModel model)
        {
            var result = await _userService.Post(model);
            return Ok(result);
        }

        [HttpPut("admin/user")]
        [CMSAuthorize(RouteLevel = 2)]
        public async Task<IActionResult> Put(UserModel model)
        {
            var result = await _userService.Put(model);
            return Ok(result);
        }

        [HttpDelete("admin/user/{id}")]
        [CMSAuthorize(RouteLevel = 2)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.Delete(id);
            return Ok(result);
        }

        [HttpGet("admin/user/jobs/{id}")]
        [CMSAuthorize(RouteLevel = 3)]
        public async Task<IActionResult> GetUserJobs(int id)
        {
            var result = await _userService.GetUserJobs(id);
            return Ok(result);
        }

        [HttpGet("admin/user/files/{id}")]
        [CMSAuthorize(RouteLevel = 3)]
        public async Task<IActionResult> GetUserFiles(int id)
        {
            var result = await _userService.GetUserFiles(id);
            return Ok(result);
        }
    }
}
