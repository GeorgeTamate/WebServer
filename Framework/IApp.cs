using System.IO;

namespace Framework
{
    public interface IApp
    {
        string getResponse(string path, string httpVerb, string controllerName, string methodName);
    }
}
