using System;
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
    
}
