﻿using System.Collections.Generic;

namespace CMS.Model.Entity
{
    public class TodoCategory : BaseModel
    {
        public string Name { get; set; }

        public bool IsActive { get; set; }

        public bool Deleted { get; set; }

        public virtual ICollection<Todo> Todos { get; set; }
    }
}
