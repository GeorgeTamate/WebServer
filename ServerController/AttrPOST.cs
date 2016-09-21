using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerController
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    class AttrPOST : System.Attribute
    {
        private string verb;
        
        public AttrPOST()
        {
            verb = "POST";
        }
    }
}
