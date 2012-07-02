using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;
using Castle.MicroKernel.Lifestyle;
using FluentAssertions;

namespace Scratch.Tests
{
    [TestFixture]
    public class WhenUsingScopes
    {
        [Test]
        public void NestedScopesGiveSeparateInstancesForScopedRegisteredComponents()
        {
            var container = new WindsorContainer();

            container.Register(Component.For<IStringFactory>().ImplementedBy<StringFactory>().LifeStyle.Scoped());

            using (container.BeginScope())
            {
                var outer = container.Resolve<IStringFactory>();
                using (container.BeginScope())
                {
                    var inner = container.Resolve<IStringFactory>();
                    inner.Should().NotBeSameAs(outer);

                    var inner2 = container.Resolve<IStringFactory>();
                    inner2.Should().BeSameAs(inner);
                }                
            }
        } 
    }

    public class StringFactory : IStringFactory, IDisposable
    {
        private static int instances = 0;
        public StringFactory()
        {
            Console.WriteLine("Constructing" + ++instances);
        }


        public void Dispose()
        {
            Console.WriteLine("Destructing" + instances--);
        }
    }

    public interface IStringFactory
    {

    }
}