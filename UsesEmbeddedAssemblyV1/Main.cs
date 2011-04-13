using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UsesEmbeddedAssemblyV1
{
    public class Main
    {
        public static string SayHello()
        {
            return EmbeddedAssembly.EmbeddedClass.SayHi();
        }
    }
}
