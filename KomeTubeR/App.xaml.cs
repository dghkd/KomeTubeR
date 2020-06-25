using KomeTubeR.Kernel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KomeTubeR
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        public static StartupParameter AppStartupParameter = new StartupParameter();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //解析命令列參數
            AppStartupParameter.IsParsed = ParseArgs(e);
        }

        /// <summary>
        /// 解析命令列參數
        /// <para>命令列參數必須同時包含-url影片網址與-o輸出檔案位置才會回傳true</para>
        /// </summary>
        /// <param name="e">程式啟動時帶入的命令列參數</param>
        /// <returns>參數中同時包含-url影片網址與-o輸出檔案位置時回傳true.否則為false.</returns>
        private bool ParseArgs(StartupEventArgs e)
        {
            bool hasUrl = false;
            bool hasOutput = false;

            for (int i = 0; i < e.Args.Length;)
            {
                string arg = e.Args[i];
                switch (arg)
                {
                    case "-url":
                        AppStartupParameter.Url = e.Args[i + 1];
                        i += 2;
                        hasUrl = true;
                        break;

                    case "-o":
                        AppStartupParameter.OutputFilePath = e.Args[i + 1];
                        i += 2;
                        hasOutput = true;
                        break;

                    case "-hide":
                        AppStartupParameter.IsHide = true;
                        i++;
                        break;

                    case "-close":
                        App.AppStartupParameter.IsClose = true;
                        i++;
                        break;

                    default:
                        break;
                }
            }

            return hasUrl && hasOutput;
        }
    }
}