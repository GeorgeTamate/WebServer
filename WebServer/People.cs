using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebServer
{
    public class People
    {
        public string id = Guid.NewGuid().ToString();
        public string firstname;
        public string lastname;
        public string age;

        public People(string firstname, string lastname, string age)
        {
            this.firstname = firstname;
            this.lastname = lastname;
            this.age = age;
        }
    }
}
