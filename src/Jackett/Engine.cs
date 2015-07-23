﻿using Autofac;
using Jackett.Services;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackett
{
    public class Engine
    {
        private static IContainer container = null;

        static Engine()
        {
            BuildContainer();
           
        }

        public static void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<JackettModule>();
            container = builder.Build();

            // Register the container in itself to allow for late resolves
            var secondaryBuilder = new ContainerBuilder();
            secondaryBuilder.RegisterInstance<IContainer>(container).SingleInstance();
            SetupLogging(secondaryBuilder);
            secondaryBuilder.Update(container);
        }

        public static IContainer GetContainer()
        {
            return container;
        }

        public static bool IsWindows
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT;
            }
        }

        public static IConfigurationService ConfigService
        {
            get
            {
                return container.Resolve<IConfigurationService>();
            }
        }

        public static IProcessService ProcessService
        {
            get
            {
                return container.Resolve<IProcessService>();
            }
        }

        public static IServiceConfigService ServiceConfig
        {
            get
            {
                return container.Resolve<IServiceConfigService>();
            }
        }

        public static IServerService Server
        {
            get
            {
                return container.Resolve<IServerService>();
            }
        }

        public static IRunTimeService RunTime
        {
            get
            {
                return container.Resolve<IRunTimeService>();
            }
        }

        public static Logger Logger
        {
            get
            {
                return container.Resolve<Logger>();
            }
        }

        public static ISecuityService SecurityService
        {
            get
            {
                return container.Resolve<ISecuityService>();
            }
        }


        private static void SetupLogging(ContainerBuilder builder)
        {
            var logConfig = new LoggingConfiguration();

            var logFile = new FileTarget();
            logConfig.AddTarget("file", logFile);
            logFile.Layout = "${longdate} ${level} ${message} ${exception:format=ToString}";
            logFile.FileName = Path.Combine(ConfigurationService.GetAppDataFolderStatic(), "log.txt");
            logFile.ArchiveFileName = "log.{#####}.txt";
            logFile.ArchiveAboveSize = 500000;
            logFile.MaxArchiveFiles = 1;
            logFile.KeepFileOpen = false;
            logFile.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
            var logFileRule = new LoggingRule("*", LogLevel.Info, logFile);
            logConfig.LoggingRules.Add(logFileRule);

            var logConsole = new ColoredConsoleTarget();
            logConfig.AddTarget("console", logConsole);
            
            logConsole.Layout = "${longdate} ${level} ${message} ${exception:format=ToString}";
            var logConsoleRule = new LoggingRule("*", Startup.TracingEnabled ? LogLevel.Debug : LogLevel.Info, logConsole);
            logConfig.LoggingRules.Add(logConsoleRule);

            LogManager.Configuration = logConfig;
            builder.RegisterInstance<Logger>(LogManager.GetCurrentClassLogger()).SingleInstance();
        }
    }
}
