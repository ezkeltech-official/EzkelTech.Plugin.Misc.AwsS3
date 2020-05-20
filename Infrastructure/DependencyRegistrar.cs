using Autofac;
using Autofac.Builder;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using EzkelTech.Plugin.Misc.AwsS3.Services;
using Nop.Services.Media;
using Nop.Services.Plugins;
using System.Linq;
using System;

namespace EzkelTech.Plugin.Misc.AwsS3.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private bool PluginIsEnable(ITypeFinder typeFinder)
        {
            var types = typeFinder.FindClassesOfType<IPluginService>(true);
            //https://www.nopcommerce.com/en/boards/topic/32899/uninstalled-plugin-still-being-used/page/2
            if (types.Count() == 1)
            {
                var plugins = Activator.CreateInstance(types.First()) as IPluginService;
                var descr = plugins.GetPluginDescriptorBySystemName<IPlugin>("EzkelTech.AwsS3");

                return descr == null || descr.Installed == false;
            }

            return true;
        }
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //if(PluginIsEnable(typeFinder))
                builder.RegisterDecorator<Aws3PictureService, IPictureService>();
        }

        public int Order => 1;
    }
}
