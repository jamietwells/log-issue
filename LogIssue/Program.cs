using log4net.Config;
using System.IO;
using Topshelf;
using Topshelf.HostConfigurators;
using Topshelf.Logging;
using Topshelf.ServiceConfigurators;

namespace LogIssue
{
    public class Program
    {
        public const string Name = "LogIssue";

        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            HostFactory.Run(ConfigureHost);
        }

        private static void ConfigureHost(HostConfigurator x)
        {
            x.UseLog4Net();
            x.Service<WindowsService>(ConfigureService);

            x.SetServiceName(Name);
            x.SetDisplayName(Name);
            x.SetDescription(Name);

            x.RunAsLocalSystem();
            x.StartAutomatically();
            x.OnException(ex => HostLogger.Get(Name).Error(ex));
        }

        private static void ConfigureSystemRecovery(ServiceRecoveryConfigurator serviceRecoveryConfigurator) =>
            serviceRecoveryConfigurator.RestartService(delayInMinutes: 1);

        private static void ConfigureService(ServiceConfigurator<WindowsService> serviceConfigurator)
        {
            serviceConfigurator.ConstructUsing(() => new WindowsService(HostLogger.Get(Name)));
            serviceConfigurator.WhenStarted(service => service.OnStart());
            serviceConfigurator.WhenStopped(service => service.OnStop());
        }
    }

    internal class WindowsService
    {
        private LogWriter _logWriter;

        public WindowsService(LogWriter logWriter)
        {
            _logWriter = logWriter;
        }

        internal bool OnStart() {
            new Worker(_logWriter).DoWork();
            return true;
        }

        internal bool OnStop() => true;
    }

    internal class Worker
    {
        private LogWriter _logWriter;

        public Worker(LogWriter logWriter)
        {
            _logWriter = logWriter;
        }

        public async void DoWork() {
            _logWriter.Info("Why is this line not logged?");
            File.WriteAllText("D:\\file.txt", "Hello, World!");
        }
    }
}
