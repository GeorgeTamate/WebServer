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
            if (File.Exists($"{path}peopleForm.html"))
            {
                //using (Stream filestream = File.Open($"{path}peopleGet.shtml", FileMode.Open))
                    return $"{path}peopleForm.html";
            }
            return null;
        }

        
    }
}
