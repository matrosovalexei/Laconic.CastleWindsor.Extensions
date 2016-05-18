using System;
using System.IO;
using System.Threading;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;

namespace Laconic.CastleWindsor.Extensions.Tests
{
    [TestFixture]
    public class FileWatchingLifestyleTests
    {
        private static FileSystemWatcher InterceptedWatcher { get; set; }

        private string _tempFilePath;

        [SetUp]
        public void SetUp()
        {
            _tempFilePath = Path.GetTempFileName();
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_tempFilePath);

            if (InterceptedWatcher == null) return;

            InterceptedWatcher.EnableRaisingEvents = false;
            InterceptedWatcher = null;
        }

        [Test]
        public void Resolve_NoPathDependency_ThrowsException()
        {
            var container = CreateContainer();

            Assert.That(() => container.Register(Component.For<TestComponent>().LifeStyle.Custom<FileWatchingLifestyleUnderTest>()),
                Throws.Exception.InstanceOf<ComponentRegistrationException>()
                    .With.Message.EqualTo("Custom dependency 'path' is required to set path to the file to watch"));
        }

        [Test]
        public void Resolve_CalledOnce_ReturnsInstance()
        {
            var container = CreateContainerWithTestComponent();

            var instance = container.Resolve<TestComponent>();

            Assert.That(instance, Is.Not.Null);
        }

        [Test]
        public void Resolve_CalledTwice_ReturnsSameInstance()
        {
            var container = CreateContainerWithTestComponent();

            var first = container.Resolve<TestComponent>();
            var second = container.Resolve<TestComponent>();

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void Resolve_FileUpdated_InstanceIsReset()
        {
            var container = CreateContainerWithTestComponent();

            var first = container.Resolve<TestComponent>();
            ChangeFileSynchronously(() => File.AppendAllText(_tempFilePath, " "));
            var second = container.Resolve<TestComponent>();

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Resolve_FileDeletedAndCreated_InstanceIsReset()
        {
            var container = CreateContainerWithTestComponent();

            var first = container.Resolve<TestComponent>();
            ChangeFileSynchronously(() =>
            {
                File.Delete(_tempFilePath);
                File.AppendAllText(_tempFilePath, " ");
            });
            var second = container.Resolve<TestComponent>();

            Assert.That(second, Is.Not.SameAs(first));
        }

        private static void ChangeFileSynchronously(Action action)
        {
            var manualResetEvent = new ManualResetEvent(false);
            FileSystemEventHandler setEvent = (sender, args) => manualResetEvent.Set();
            InterceptedWatcher.Changed += setEvent;

            action();

            manualResetEvent.WaitOne();
            InterceptedWatcher.Changed -= setEvent;
        }

        private static IWindsorContainer CreateContainer()
        {
            return new WindsorContainer();
        }

        private IWindsorContainer CreateContainerWithTestComponent()
        {
            return CreateContainer().Register(
                Component.For<TestComponent>()
                    .DependsOn(new {path = _tempFilePath})
                    .LifeStyle.Custom<FileWatchingLifestyleUnderTest>());
        }

        public class FileWatchingLifestyleUnderTest : FileWatchingLifestyle
        {
            protected override FileSystemWatcher Watcher
            {
                get { return base.Watcher; }
                set
                {
                    InterceptedWatcher = value;
                    base.Watcher = value;
                }
            }
        }

        public class TestComponent
        {
        }
    }
}