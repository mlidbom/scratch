#region usings

using System;
using System.Linq;
using System.Reflection;

#endregion

[Serializable]
internal class ModuleInitializer
{
    public static void Run()
    {
        Console.WriteLine("Running module initializer for {0} in appdomain {1}", Assembly.GetExecutingAssembly().FullName,
                          AppDomain.CurrentDomain.FriendlyName);

        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
    }

    private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
    {
        Console.WriteLine("Looking for assembly {0} \n\tin assembly {1}\n\tin Appdomain {2}\n", args.Name, Assembly.GetExecutingAssembly().FullName, AppDomain.CurrentDomain.FriendlyName);

        var loaded = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.FullName == args.Name);

        if(loaded == null)
        {
            var requestedAssemblyName = new AssemblyName(args.Name).Name;            
            var assemblyData = ReadMatchingResourceByteArray(requestedAssemblyName + ".dll");

            if(assemblyData != null)
            {
                if(AssemblyNameExtractor.ExtractAssemblyName(assemblyData) == args.Name)
                {
                    var symbolsData = ReadMatchingResourceByteArray(requestedAssemblyName + ".pdb");
                    loaded = Assembly.Load(assemblyData, symbolsData);

                    Console.WriteLine("\tFound assembly {0} \n\t\tin assembly {1}\n\t\tin Appdomain {2}\n", args.Name, Assembly.GetExecutingAssembly().FullName, AppDomain.CurrentDomain.FriendlyName);
                }
            }
        }else
        {
            Console.WriteLine("\tAssembly {0} was already loaded\n", args.Name);
        }
        return loaded;
    }

    private static byte[] ReadMatchingResourceByteArray(string resourceName)
    {
        var resourcePath = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(name => name.EndsWith(resourceName)).SingleOrDefault();
        if(resourcePath == null)
        {
            return null;
        }
        using(var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
        {
            var resourceData = new Byte[resourceStream.Length];
            resourceStream.Read(resourceData, 0, resourceData.Length);
            return resourceData;
        }
    }


    [Serializable]
    public class AssemblyNameExtractor
    {
        public static string ExtractAssemblyName(byte[] assemblyData)
        {
            var domain = AppDomain.CreateDomain("TempDomain");
            var nameExtractor = new AssemblyNameExtractor { _assemblyData = assemblyData };
            Console.WriteLine("\textracting name");
            domain.DoCallBack(nameExtractor.ExtractName);
            AppDomain.Unload(domain);
            return nameExtractor._nameHolder.Value;
        }

        private byte[] _assemblyData;
        private readonly NameHolder _nameHolder = new NameHolder();

        private void ExtractName()
        {
            _nameHolder.Value = Assembly.ReflectionOnlyLoad(_assemblyData).FullName;
        }

        private class NameHolder : MarshalByRefObject
        {
            public string Value;
        }
    }
}