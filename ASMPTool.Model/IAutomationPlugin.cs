// 檔案：IAutomationPlugin.cs
// 專案：PluginContracts
using System;

namespace PluginContracts
{
    public interface IAutomationPlugin
    {
        /// <summary>
        /// 執行自動化腳本的主要方法
        /// </summary>
        /// <param name="ownerHwnd">主視窗的 Handle</param>
        /// <param name="configPath">設定檔路徑</param>
        /// <returns>執行結果的日誌字串</returns>
        string Execute(IntPtr ownerHwnd, string configPath);
    }
}