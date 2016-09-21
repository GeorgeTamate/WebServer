using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ServerController
{
    
    public class TestController : IController
    {
        public TestController()
        {
        }

        [AttrGET]
        public void peopleGet(StreamWriter ostream, string path)
        {
            using (Stream filestream = File.Open(path + @"\peopleGet.shtml", FileMode.Open))
                filestream.CopyTo(ostream.BaseStream);
        }

        [AttrPOST]
        public void peoplePost(StreamWriter ostream, string path)
        {
            using (Stream filestream = File.Open(path + @"\peoplePost.shtml", FileMode.Open))
                filestream.CopyTo(ostream.BaseStream);
        }

        [AttrPUT]
        public void peoplePut(StreamWriter ostream, string path)
        {
            using (Stream filestream = File.Open(path + @"\peoplePut.shtml", FileMode.Open))
                filestream.CopyTo(ostream.BaseStream);
        }

        [AttrDELETE]
        public void peopleDelete(StreamWriter ostream, string path)
        {
            using (Stream filestream = File.Open(path + @"\peopleDelete.shtml", FileMode.Open))
                filestream.CopyTo(ostream.BaseStream);
        }
    }
}
