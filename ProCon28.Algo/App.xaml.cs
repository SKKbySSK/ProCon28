using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProCon28.Algo
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Exit += App_Exit;
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
