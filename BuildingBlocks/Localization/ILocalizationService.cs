<<<<<<< HEAD
﻿namespace BuildingBlocks.Localization
{

    public interface ILocalizationService
    {

        string Get(string key);


        string Get(string key, params object?[] arguments);
    }

=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Localization
{
   
        public interface ILocalizationService
        {
            
            string Get(string key);

         
            string Get(string key, params object?[] arguments);
        }
    
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
}
