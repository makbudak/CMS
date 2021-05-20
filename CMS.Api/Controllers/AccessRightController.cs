﻿using CMS.Model.Enum;
using CMS.Service;
using CMS.Service.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CMS.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessRightController : ControllerBase
    {
        private readonly IAccessRightService accessRightService;
        public AccessRightController(IAccessRightService accessRightService)
        {
            this.accessRightService = accessRightService;
        }

        [HttpGet]
        [CMSAuthorize]
        public IActionResult Get()
        {
            var accessRights = accessRightService.Get();
            return Ok(accessRights);
        }        
    }
}