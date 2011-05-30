#region usings

using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

#endregion

namespace Scratch
{
    public class Application
    {
        public static void Main()
        {
            var container = new WindsorContainer();
            container.Register(AllTypes.FromThisAssembly().BasedOn(typeof(IHandle<>)).WithService.Base());

            var handlers = container.ResolveAll<IHandle<IB>>();
            foreach(var handler in handlers)
            {
                handler.Handle(new B() { });
            }
        }
    }

    public interface IHandle<in T>
    {
        void Handle(T message);
    }

    public interface IA{}

    public interface IB : IA{}

    public class A : IA{}
    public class B : IB {}


    public class AHandler : IHandle<IA>
    {
        public void Handle(IA message)
        {
            Console.WriteLine("AHandler");
        }
    }

    public class IBHandler : IHandle<IB>
    {
        public void Handle(IB message)
        {
            Console.WriteLine("IBHandler");
        }
    }

}