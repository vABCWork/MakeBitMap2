using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MakeBitMap
{
    /// <summary>
    /// SetPixelSize.xaml の相互作用ロジック
    /// </summary>
    public partial class SetPixelSize : Window
    {
        public SetPixelSize()
        {
            InitializeComponent();

            MainWindow.disp_width = 8;     // 横のピクセル数(初期値)
            MainWindow.disp_height = 16;    // 縦のピクセル数
            MainWindow.em_font_size = 16;   // デフォルトフォントサイズ
        }


        // 8x16 チェック時
        private void RadioButton_8_16_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.disp_width = 8;     // 横のピクセル数
            MainWindow.disp_height = 16;    // 縦のピクセル数
            MainWindow.em_font_size = 16;   // デフォルトフォントサイズ
        }

        // 16x32 チェック時
        private void RadioButton_16_32_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.disp_width = 16;     // 横のピクセル数
            MainWindow.disp_height = 32;    // 縦のピクセル数
            MainWindow.em_font_size = 24;
        }


        // 32x64 チェック時
        private void RadioButton_32_64_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.disp_width = 32;
            MainWindow.disp_height = 64;
            MainWindow.em_font_size = 48;
        }


        // 48x24 チェック時
        private void RadioButton_48_24_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.disp_width = 48;
            MainWindow.disp_height = 24;
            MainWindow.em_font_size = 20;
        }


        // 48x96 チェック時
        private void RadioButton_48_96_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.disp_width = 48;
            MainWindow.disp_height = 96;
            MainWindow.em_font_size = 72;
        }

        // 64x64 チェック時
        private void RadioButton_64_64_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.disp_width = 64;
            MainWindow.disp_height = 64;
            MainWindow.em_font_size = 48;
        }

        // OKボタンの処理
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

      
    }
}
