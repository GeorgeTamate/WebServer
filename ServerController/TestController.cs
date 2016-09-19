using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerController
{
    public class TestController : IController
    {
        public TestController()
        {
        }

        [AttrGET]
        public void peopleGet()
        {
        }

        [AttrPOST]
        public void peoplePost()
        {
        }

        [AttrPUT]
        public void peoplePut()
        {
        }

        [AttrDELETE]
        public void peopleDelete()
        {
        }
    }
}
