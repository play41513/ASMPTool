using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASMPTool.Model;

namespace ASMPTool.DAL
{
    public class NLogDAL
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private NLogDAL() { }

        private static readonly Lazy<NLogDAL> instance = new(() => new NLogDAL());

        public static NLogDAL Instance => instance.Value;


        #pragma warning disable IDE0079 // 移除非必要的隱藏項目
        #pragma warning disable CA1822 // 將成員標記為靜態
        public void LogInfo(NLogModel logModel)
        {
            logger.Info($"{logModel.Timestamp} [{logModel.LogLevel}] {logModel.Message}");
        }

        public void LogWarning(NLogModel logModel)
        {
            logger.Warn($"{logModel.Timestamp} [{logModel.LogLevel}] {logModel.Message}");
        }

        public void LogError(NLogModel logModel)
        {
            if (logModel.Exception != null)
            {
                logger.Error(logModel.Exception, $"{logModel.Timestamp} [{logModel.LogLevel}] {logModel.Message}");
            }
            else
            {
                logger.Error($"{logModel.Timestamp} [{logModel.LogLevel}] {logModel.Message}");
            }
        }
    }
}
