﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sikia.Sys
{
    public abstract class ModulePlugIn
    {
        public abstract void Register();
        private static bool isSystem(string name)
        {
            return name.StartsWith("System.") || name.StartsWith("Microsoft.")
                || name.StartsWith("System,") || name.StartsWith("mscorlib,");
        }

        private static void RegisterModule(Type type)
        {
            ModulePlugIn plugIn = (ModulePlugIn)Activator.CreateInstance(type);
            plugIn.Register();
        }
        private static string AssemblyDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }
        public static void Initialize()
        {

            Type basePlugin = typeof(ModulePlugIn);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (isSystem(assembly.FullName))
                    continue;
                Type[] types = assembly.GetExportedTypes();
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(basePlugin))
                    {
                        ModulePlugIn.RegisterModule(type);
                    }
                }
            }

        }

        public static void Load(string module)
        {
            try
            {
                var fileName = Path.Combine(ModulePlugIn.AssemblyDirectory, module + ".dll");
                if (File.Exists(fileName))
                {
                    var basePlugin = typeof(ModulePlugIn);
                    var pluginDll = Assembly.LoadFile(fileName);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Logger.PLUGIN, ex);
            }
        }


    }

}
