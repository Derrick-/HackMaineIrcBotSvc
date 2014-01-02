using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackMaineIrcBot
{
    public static class ReflectionUtils
    {
        public static List<Type> FindInheritedTypes(Type parenttype)
        {
            List<Type> list = new List<Type>();
            foreach (var t in Program.Assembly.GetTypes())
            {
                if (t != parenttype && parenttype.IsAssignableFrom(t))
                    list.Add(t);
            }
            return list;
        }
    }
}
