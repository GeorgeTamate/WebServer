using System.IO;
using Framework;
using app.Attributes;

namespace app
{
    public class peopleController : Controller
    {
        public peopleController() { }

        [AttrGET]
        public string peopleGet(string path)
        {
            if (File.Exists($"{path}peopleGet.shtml"))
            {
                using (Stream filestream = File.Open($"{path}peopleGet.shtml", FileMode.Open))
                    return $"{path}peopleGet.shtml";
            }
            return null;
        }

        [AttrPOST]
        public string peoplePost(string path)
        {
            if (File.Exists($"{path}peoplePost.shtml"))
            {
                using (Stream filestream = File.Open($"{path}peoplePost.shtml", FileMode.Open))
                    return $"{path}peoplePost.shtml";
            }
            return null;
        }

        [AttrPUT]
        public string peoplePut(string path)
        {
            if (File.Exists($"{path}peoplePut.shtml"))
            {
                using (Stream filestream = File.Open($"{path}peoplePut.shtml", FileMode.Open))
                    return $"{path}peoplePut.shtml";
            }
            return null;
        }

        [AttrDELETE]
        public string peopleDelete(string path)
        {
            if (File.Exists($"{path}peopleDelete.shtml"))
            {
                using (Stream filestream = File.Open($"{path}peopleDelete.shtml", FileMode.Open))
                    return $"{path}peopleDelete.shtml";
            }
            return null;
        }
    }


}
