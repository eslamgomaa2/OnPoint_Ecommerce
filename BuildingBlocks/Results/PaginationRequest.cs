<<<<<<< HEAD
﻿namespace BuildingBlocks.Results
{
    public class PaginationRequest
    {

        public int PageNumber { get; set; } = 1;


=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Results
{
    public class PaginationRequest
    {
        
        public int PageNumber { get; set; } = 1;

        
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > 100 || value <= 0) ? 10 : value;
        }

        public string? SearchTerm { get; set; }
    }
}
