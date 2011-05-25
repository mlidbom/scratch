#region usings

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

#endregion

namespace Scratch
{
    public class Application
    {
        public static void Main()
        {
            //TestEvent(Enumerable.Range(0, 20).Select( _ => new Events.RegisteredEvent()).ToArray());
            TestEvent(Enumerable.Range(0, 20).Select(_ => new Events.AddSkillEvent()).ToArray());
        }

        private static void TestEvent(object @event)
        {
            var stringData = JsonConvert.SerializeObject(@event);
            Console.WriteLine("Jsonstring: {0}", stringData.Length);
            Console.WriteLine();
            Console.WriteLine(stringData);


            var jsonSerilizer = new JsonSerializer
                                    {
                                        TypeNameHandling = TypeNameHandling.Auto,
                                        DefaultValueHandling = DefaultValueHandling.Ignore,
                                        NullValueHandling = NullValueHandling.Ignore
                                    };

            using (var stream = new MemoryStream())
            {
                using(var streamWriter = new StreamWriter(stream, Encoding.UTF8))
                {
                    using (var jwriter = new JsonTextWriter(streamWriter))
                    {
                        jsonSerilizer.Serialize(jwriter, @event);
                    }
                    var byteData = stream.ToArray();
                    Console.WriteLine("JsonSerializedToByteArray: {0}", byteData.Length);
                    Console.WriteLine("Compressed JsonSerializedToByteArray: {0}", Compression.Compress(byteData).Length);
                    Console.WriteLine("Ratio: {0}", (Double)byteData.Length / Compression.Compress(byteData).Length);
                    Console.WriteLine("GZip Compressed JsonSerializedToByteArray: {0}", Compressor.Compress(byteData).Length);
                    Console.WriteLine("GZip ratio: {0}", (Double)byteData.Length / Compressor.Compress(byteData).Length);

                }
            }

            var writer = new StringWriter();

            var serializer = new XmlSerializer(@event.GetType());
            serializer.Serialize(writer, @event);
            stringData = writer.ToString();
            Console.WriteLine("Xml string: {0}", stringData.Length);
            Console.WriteLine();
            Console.WriteLine(stringData);


            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, @event);
                var byteData = stream.ToArray();
                Console.WriteLine("XMLSerializedToByteArray: {0}", byteData.Length);
                Console.WriteLine("Compressed XMLSerializedToByteArray: {0}", Compression.Compress(byteData).Length);
                Console.WriteLine("Ratio: {0}", (Double)byteData.Length / Compression.Compress(byteData).Length);

                Console.WriteLine("GZIp Compressed XMLSerializedToByteArray: {0}", Compressor.Compress(byteData).Length);
                Console.WriteLine("GZip Ratio: {0}", (Double)byteData.Length / Compressor.Compress(byteData).Length);
            }
        }
    }
}