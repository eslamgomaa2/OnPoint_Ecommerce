<<<<<<< HEAD
﻿
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
namespace Onpoint.Store.Domin.Common
{
    public abstract class BaseEntity
    {
<<<<<<< HEAD
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
=======
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
    }
}
