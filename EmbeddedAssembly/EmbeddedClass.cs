#region usings

using System.Reflection;

#endregion

namespace EmbeddedAssembly
{
    public class EmbeddedClass
    {
        public static string SayHi()
        {
            return Assembly.GetExecutingAssembly().FullName;
        }
    }
}