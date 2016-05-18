using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Registration.Lifestyle;

namespace Laconic.CastleWindsor.Extensions
{
    public static class LifestyleGroupExtensions
    {
        public static ComponentRegistration<T> FileWatching<T>(this LifestyleGroup<T> lifestyleGroup)
            where T : class
        {
            return lifestyleGroup.Custom<FileWatchingLifestyle>();
        }
    }
}