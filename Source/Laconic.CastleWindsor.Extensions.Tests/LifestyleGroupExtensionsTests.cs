using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;

namespace Laconic.CastleWindsor.Extensions.Tests
{
    [TestFixture]
    public class LifestyleGroupExtensionsTests
    {
        [Test]
        public void UnitOfWork_Scenario_public()
        {
            var container = new WindsorContainer();

            container.Register(
                Component.For<TestComponent>()
                    .DependsOn(new {path = AppDomain.CurrentDomain.BaseDirectory + "config.xml"})
                    .LifeStyle.FileWatching());

            Assert.That(container.Kernel.GetHandler(typeof (TestComponent)).ComponentModel.CustomLifestyle, Is.EqualTo(typeof (FileWatchingLifestyle)));
        }

        public class TestComponent
        {
        }
    }
}