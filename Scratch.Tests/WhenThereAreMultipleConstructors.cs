using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;
using Castle.MicroKernel.Lifestyle;
using FluentAssertions;

namespace Scratch.Tests
{
    [TestFixture]
    public class WhenThereAreMultipleConstructors
    {
        [Test]
        public void TheConstructorWithTheMostParametersWin()
        {

            var container = new WindsorContainer();

            container.Register(
                Component.For<IRegisterWhichConstructorWasCalled>().ImplementedBy<RegisterWhichConstructorWasCalled>().
                    LifeStyle.Scoped());

            using(container.BeginScope())
            {
                container.Resolve<IRegisterWhichConstructorWasCalled>().Constructor.Should().Be(Constructor.Empty);
            }

            container.Register(Component.For<IDep1>().ImplementedBy<Dep1>().LifeStyle.Scoped());
            using (container.BeginScope())
            {
                container.Resolve<IRegisterWhichConstructorWasCalled>().Constructor.Should().Be(Constructor.Dep1);
            }

            container.Register(Component.For<IDep2>().ImplementedBy<Dep2>().LifeStyle.Scoped());
            using (container.BeginScope())
            {
                container.Resolve<IRegisterWhichConstructorWasCalled>().Constructor.Should().Be(Constructor.Dep1AndDep2);
            }
        } 
    }

    public enum Constructor
    {
        Empty, Dep1, Dep1AndDep2
    }

    public class RegisterWhichConstructorWasCalled : IRegisterWhichConstructorWasCalled
    {
        public Constructor Constructor { get; private set; }

        public RegisterWhichConstructorWasCalled()
        {
            Constructor = Constructor.Empty;
        }

        public RegisterWhichConstructorWasCalled(IDep1 dep1)
        {
            Constructor = Constructor.Dep1;
        }

        public RegisterWhichConstructorWasCalled(IDep1 dep1, IDep2 dep2)
        {
            Constructor=Constructor.Dep1AndDep2;
        }
    }

    public interface IRegisterWhichConstructorWasCalled
    {
        Constructor Constructor { get; }
    }

    public class Dep2 : IDep2
    {
    }

    public interface IDep2
    {
    }

    public class Dep1 : IDep1
    {
    }

    public interface IDep1
    {
    }
}