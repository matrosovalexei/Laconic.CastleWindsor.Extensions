using System.IO;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;

namespace Laconic.CastleWindsor.Extensions
{
    public class FileWatchingLifestyle : SingletonLifestyleManager
    {
        protected virtual FileSystemWatcher Watcher { get; set; }

        public override void Init(IComponentActivator componentActivator, IKernel kernel, ComponentModel model)
        {
            base.Init(componentActivator, kernel, model);

            var path = GetPath(model);
            var directory = GetDirectory(path);
            var fileName = Path.GetFileName(path);

            Watcher = new FileSystemWatcher(directory) {Filter = fileName};
            Watcher.Changed += (sender, e) => Dispose();
            Watcher.Deleted += (sender, e) => Dispose();
            Watcher.EnableRaisingEvents = true;
        }

        private static string GetPath(ComponentModel model)
        {
            var path = model.CustomDependencies["path"] as string;

            if (path == null)
            {
                throw new ComponentRegistrationException("Custom dependency 'path' is required to set path to the file to watch");
            }

            return path;
        }

        private static string GetDirectory(string path)
        {
            return Path.GetDirectoryName(path);
        }
    }
}