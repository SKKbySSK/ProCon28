using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProCon28
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Exit += App_Exit;
            Startup += App_Startup;

            System.IO.Directory.CreateDirectory("Batch");
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            foreach(string arg in e.Args)
            {
                switch (arg.ToUpper())
                {
                    case "PROCON":
                        Instance.ProConMode = true;
                        DispatcherUnhandledException += App_DispatcherUnhandledException;
                        break;
                    case "SAVE":
                        Instance.SaveConfig = true;
                        break;
                }
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            switch (e.Exception)
            {
                case IndexOutOfRangeException ex:
                    e.Handled = true;
                    break;
                case ArgumentOutOfRangeException ex:
                    e.Handled = true;
                    break;
                case Exception ex:
                    if (MessageBox.Show("未知のエラーが発生しました\n\n" + ex.Message + "\n\n続行しますか？", "エラー", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        e.Handled = true;
                    break;
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            var result = Config.Save();
            if (!result.Item1)
            {
                MessageBox.Show("設定の保存に失敗\n\n" + result.Item2.Message);
            }
        }
    }
}
