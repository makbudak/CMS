﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Model.Entity
{
    public class BlogCategory : BaseModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsShowHomePage { get; set; }
        public bool IsActive { get; set; }
        public bool Deleted { get; set; }
        public virtual ICollection<Blog> Blogs { get; set; }
    }
}