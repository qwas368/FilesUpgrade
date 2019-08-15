#nullable enable
using FilesUpgrade.Monad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LanguageExt;
using Autofac;
using System.Reflection;
using FilesUpgrade.Controller;
using FilesUpgrade.Service;
using FilesUpgrade.IO;

namespace FilesUpgrade
{
    class Program
    {
        private static IContainer container;

        static void Main(string[] args)
        {
            container = InitialAutoFac();

            using (var scope = container.BeginLifetimeScope())
            {
                var entry = container.Resolve<Entry>();
                entry.Execute(args);
            }

            Console.WriteLine("Press any key to continue..");
            Console.ReadKey();
        }

        public static IContainer InitialAutoFac()
        {
            var builder = new ContainerBuilder();

            Assembly assembly = Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("Service"));

            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("Controller"));

            builder.RegisterType<FileSystem>();
            builder.RegisterType<Diff>();
            builder.RegisterType<Entry>();

            return builder.Build();
        }
    }
}
