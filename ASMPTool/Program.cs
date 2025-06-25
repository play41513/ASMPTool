using ASMPTool;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.IO;

namespace ASMPTool
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // �]�w NLog �t�m
            var config = new LoggingConfiguration();
            // ���o��e�{������Ƨ����|
            var currentDirectory = Directory.GetCurrentDirectory();
            var debugDirectory = Path.Combine(currentDirectory, "debug");
            if (!Directory.Exists(debugDirectory))
            {
                Directory.CreateDirectory(debugDirectory);
            }
            // �Ыؤ�x��X���󪺥ؼ�
            // �]�w��x�x�s�� "debug" ��Ƨ��U
            var logfile = new FileTarget("logfile")
            {
                FileName = Path.Combine(currentDirectory, "debug", "log-${shortdate}.txt"), // �ʺA�]�m��x���|
                                                                                            // �]�w��x��X�榡
                Layout = "${longdate} ${level:uppercase=true} ${message} ${exception}"
            };

            // �N�ؼХ[��t�m��
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            // �]�w�t�m
            LogManager.Configuration = config;

            // ���o�O����
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Start Program..");

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new frmLogin());
        }
    }
}