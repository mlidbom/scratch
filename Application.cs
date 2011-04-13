#region usings

using System;
using System.Collections.Generic;
using Composable.CQRS;

#endregion

namespace Scratch
{
    public class Application
    {
        public static void Main()
        {
            //Console.WriteLine("Calling: UsesEmbeddedAssemblyV1:");
            Console.WriteLine("UsesEmbeddedAssemblyV1 says: {0}\n\n", UsesEmbeddedAssemblyV1.Main.SayHello());

            //Console.WriteLine("Calling: UsesEmbeddedAssemblyV2:");
            //Console.WriteLine("UsesEmbeddedAssemblyV2 says: {0}\n\n", UsesEmbeddedAssemblyV2.Main.SayHello());
        }
    }
}