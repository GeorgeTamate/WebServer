using System;
using System.Linq;
using System.IO;
using System.Reflection;
using Framework;

namespace App1
{
    public class App : IApp
    {
        public string getResponse(string path, string httpVerb, string controllerName, string methodName)
        {
            var assembly = Assembly.LoadFile($"{path}App1.dll");

            Type tController = assembly.GetTypes().FirstOrDefault(
                t => t.Name == $"{controllerName}Controller");

            Type tVerb = assembly.GetType($"App1.Attributes.Attr{httpVerb}");

            var method = tController.GetMethods()
                .FirstOrDefault(m => m.GetCustomAttributes(tVerb, false).Length > 0 &&
                m.Name.Contains(methodName));

            object classInstance = Activator.CreateInstance(tController, null);
            ParameterInfo[] parameters = method.GetParameters();
            object[] parametersArray = { path };
            return (string)method.Invoke(classInstance, parametersArray);
        }
    }
}
