using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ApiFoundation.PluginFramework
{
    internal static class MvcBuilderExtensions
    {
        public static IList<PluginInfo> AddPlugins(this IMvcBuilder mvc)
        {
            // Plugin model
            // In the real world this would be controlled with a manifest file.
            // note that this implementation would be insecure in the real world.
            var myPath = Assembly.GetExecutingAssembly().Location;
            string prefix = "", postfix = myPath;
            var binDirStart = myPath.LastIndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar);
            if (binDirStart > 0)
            {
                var projectDirStart = myPath.LastIndexOf(Path.DirectorySeparatorChar, binDirStart - 1);
                if (projectDirStart > 0)
                {
                    prefix = myPath.Substring(0, projectDirStart);
                    postfix = myPath.Substring(projectDirStart);
                }
            }
            var myName = Path.GetFileNameWithoutExtension(myPath);

            var dirs = Directory.GetDirectories(prefix);
            var plugins = new List<PluginInfo>();
            
            foreach (var dir in dirs)
            {
                var plugin = Path.GetFileName(dir);
                var pluginPath = prefix + postfix.Replace(myName, Path.GetFileName(dir));
                if (File.Exists(pluginPath))
                {
                    if (plugin != myName)
                    {
                        mvc.AddApplicationPart(Assembly.LoadFile(pluginPath));
                    }
                    var xmlDocFile = Path.ChangeExtension(pluginPath, "xml");
                    var info = new PluginInfo {
                        Name = Path.GetFileNameWithoutExtension(pluginPath),
                        FullPath = pluginPath,
                        XmlDocFile = File.Exists(xmlDocFile) ? xmlDocFile : null,
                    };
                    plugins.Add(info);
                }
            }

            return plugins;
        }
    }
}