﻿using CMS.Model.Enum;
using System.Collections.Generic;

namespace CMS.Model.Entity
{
    public class AccessRight : BaseModel
    {
        public int? ParentId { get; set; }

        public string Name { get; set; }

        public string Endpoint { get; set; }

        public string Method { get; set; }

        public bool IsShowMenu { get; set; }

        public AccessRightType Type { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public bool Deleted { get; set; }

        public ICollection<UserAccessRight> UserAccessRights { get; set; }
    }
}
