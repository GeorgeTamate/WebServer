using System.IO;
using Framework;
using app.Attributes;

namespace app
{
    public class homeController : Controller
    {
        public homeController() { }

        [AttrGET]
        public string Index(string path)
        {
            if (File.Exists($"{path}peopleGet.shtml"))
            {
                using (Stream filestream = File.Open($"{path}peopleGet.shtml", FileMode.Open))
                    return $"{path}peopleGet.shtml";
            }
            return null;
        }

        
    }
}
