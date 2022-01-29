﻿using System.Collections.Generic;

namespace CMS.Model.Entity
{
    public class BlogCategory : BaseModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public bool IsShowHome { get; set; }

        public bool IsActive { get; set; }

        public bool Deleted { get; set; }
    }
}
