#region usings

using EmbeddedAssembly;

#endregion

namespace UsesEmbeddedAssemblyV2
{
    public class Main
    {
        public static string SayHello()
        {
            return EmbeddedClass.SayHi();
        }
    }
}