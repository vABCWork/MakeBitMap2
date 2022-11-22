using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace MakeBitMap
{



    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        public static int disp_width;  // 一文字の横のピクセル数
        public static int disp_height; // 一文字の縦のピクセル数
        public static int em_font_size;    // FormattedTextでのフォントサイズ　
                                           // デバイスに依存しない単位 (ユニットあたり 1/96 インチ) で提供されるテキストの em メジャーのフォント サイズ。

        UInt16[] rgb565_data_buf;  // 読込み画像のRGB565データ


        UInt32 chkbox_checked_cnt;  // イベント(CheckBox_Checked)の処理回数
        int pixel_on_cnt;       　　// ファイル(ビットマップまたは画像)読み出し時に、ONしているpixel数
        Boolean pixel_on_chk_fg;      //  ファイル(ビットマップまたは画像)読み出し時 true

        int drawing_tx_pt_x;        //  drawingContext.DrawText(text, new Point(x,y)) の Pointのx値  
        int drawing_tx_pt_y;　　　　//        :                                                 y値

        string ft_font_name;       //　使用するフォント名(FormattedTextで使用されるフォント名)

        Boolean set_mov_key_fg;      //  Setボタン,移動(上下左右)ボタンが押された場合 true

        public MainWindow()
        {
            InitializeComponent();

            pixel_on_chk_fg = false;
            set_mov_key_fg= false;    

        }


        //
        // 最初の処理
        // (ウィンドウが表示された後に呼び出されます。)
        //
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            var window = new SetPixelSize();      //　作成するpixelサイズの設定
            window.Owner = this;
            window.ShowDialog();                 // ダイアログボックスの表示


            rgb565_data_buf = new UInt16[disp_width * disp_height];

            BitMap_DataGrid.DataContext = CreateData();     // データテーブルを作成し、BitMap_DataGridのデータとする。

            pixel_on_chk_fg = false;

            Bit_Map_Image();            // DataGridから画像を作成 
            Make_Bit_Map_Data();        // ビットマップ文字列　作成
            Make_Bit_Map_Pack_Data();   // 1バイト単位のビットマップ文字列　作成


            FontSize_TB.Text = em_font_size.ToString(); // フォントサイズ (デフォルト)

            ft_font_name = "TestFont0123";     // フォント名 (TestFont0123-Regular.ttf)



        }

        // Windowを閉じる時の確認処理
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Close window? \r\n Data is lost.", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning);


            if (result == MessageBoxResult.No)   // Noの場合、Windowを閉じない。
            {
                e.Cancel = true;
            }
        }

        // BitMap_DataGridの行作成時のイベント
        //  DataGridの行ヘッダに行番号を表示
        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()).ToString();
        }


        //   DataTable作成 ( widht,height )
        //

        public DataTable CreateData()
        {
            DataTable dt = new DataTable();

            for (int i = 0; i < disp_width; ++i)        // チェックボックスのコントロールを配置
            {
                dt.Columns.Add("b" + i.ToString(), typeof(bool));   // データテーブルへカラムを追加 (boolean はチェックボックス)

            }

            for (int i = 0; i < disp_height; i++)
            {
                DataRow dr = dt.NewRow();

                for (int j = 0; j < disp_width; j++)
                {
                    dr[j] = false;    // カラムの初期化
                }

                dt.Rows.Add(dr);         // データテーブルへ1行追加
            }

            return dt;
        }


        // カラム追加時のイベント
        //
        // データテーブルの列がBoole型の場合、チェックボックス(DataGridCheckBoxColumn)が自動的に使用されるが、
        // このチェックボックスはチェックに2クリックが必要。
        // 1クリックとしたので、カラム追加時のイベント( AutoGeneratingColumn )時に、
        //  DataGridTemplateColumn を使用して、ChekcBox (1 click)を配置している。
        //
        private void BitMap_DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

            if (e.PropertyName == "b0")  //  追加するセルの名称が、"b0"の場合
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b0"],
                    Header = "00",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b1")  //  追加するセルの名称が、"b1"の場合
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b1"],
                    Header = "01"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b2")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b2"],
                    Header = "02",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b3")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b3"],
                    Header = "03"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b4")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b4"],
                    Header = "04"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b5")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b5"],
                    Header = "05"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b6")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b6"],
                    Header = "06"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b7")  //  追加するセルの名称が、"b7"の場合
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b7"],
                    Header = "07"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b8")  //  追加するセルの名称が、"b8"の場合
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b8"],
                    Header = "08",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b9")  //  追加するセルの名称が、"b9"の場合
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b9"],
                    Header = "09"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b10")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b10"],
                    Header = "10",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b11")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b11"],
                    Header = "11"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b12")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b12"],
                    Header = "12"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b13")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b13"],
                    Header = "13"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b14")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b14"],
                    Header = "14"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b15")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b15"],
                    Header = "15"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b16")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b16"],
                    Header = "16",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b17")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b17"],
                    Header = "17"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b18")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b18"],
                    Header = "18",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b19")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b19"],
                    Header = "19"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b20")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b20"],
                    Header = "20"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b21")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b21"],
                    Header = "21"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b22")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b22"],
                    Header = "22"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b23")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b23"],
                    Header = "23"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b24")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b24"],
                    Header = "24",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b25")  //  追加するセルの名称が、"b9"の場合
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b25"],
                    Header = "25"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b26")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b26"],
                    Header = "26",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b27")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b27"],
                    Header = "27"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b28")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b28"],
                    Header = "28"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b29")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b29"],
                    Header = "29"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b30")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b30"],
                    Header = "30"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b31")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b31"],
                    Header = "31"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b32")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b32"],
                    Header = "32"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b33")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b33"],
                    Header = "33"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b34")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b34"],
                    Header = "34"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b35")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b35"],
                    Header = "35"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b36")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b36"],
                    Header = "36",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b37")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b37"],
                    Header = "37"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b38")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b38"],
                    Header = "38",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b39")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b39"],
                    Header = "39"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b40")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b40"],
                    Header = "40"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b41")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b41"],
                    Header = "41"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b42")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b42"],
                    Header = "42"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b43")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b43"],
                    Header = "43"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b44")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b44"],
                    Header = "44",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b45")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b45"],
                    Header = "45"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b46")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b46"],
                    Header = "46",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b47")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b47"],
                    Header = "47"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b48")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b48"],
                    Header = "48",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b49")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b49"],
                    Header = "49"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b50")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b50"],
                    Header = "50"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b51")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b51"],
                    Header = "51"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b52")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b52"],
                    Header = "52"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b53")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b53"],
                    Header = "53"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b54")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b54"],
                    Header = "54",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b55")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b55"],
                    Header = "55"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b56")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b56"],
                    Header = "56",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b57")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b57"],
                    Header = "57"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b58")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b58"],
                    Header = "58",
                };
                e.Column = c;
            }

            if (e.PropertyName == "b59")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b59"],
                    Header = "59"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b60")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b60"],
                    Header = "60"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b61")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b61"],
                    Header = "61"
                };
                e.Column = c;
            }

            if (e.PropertyName == "b62")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b62"],
                    Header = "62"
                };
                e.Column = c;
            }


            if (e.PropertyName == "b63")
            {
                var c = new DataGridTemplateColumn()
                {
                    CellTemplate = (DataTemplate)BitMap_DataGrid.Resources["b63"],
                    Header = "63"
                };
                e.Column = c;
            }

        }



        // ビットマップ データ 文字列の作成
        //  (ON="1", OFF="0"の文字列作成)

        private void Make_Bit_Map_Data()
        {
            Boolean b;
            string st;

            st = "";

            for (int i = 0; i < disp_height; i++)  // 縦のピクセル数分　繰り返す
            {

                DataRowView dataRowView = (DataRowView)BitMap_DataGrid.Items[i];  //  i行目のビット情報 

                for (int j = 0; j < disp_width; j++)  // 横のピクセル数分
                {
                    b = (Boolean)dataRowView.Row.ItemArray[j];  // j列のビット情報(true,flase)

                    if (b == true)
                    {
                        st = st + "1";
                    }
                    else if (b == false)
                    {
                        st = st + "0";
                    }

                    if (j < disp_width - 1)     // 最終列でない場合
                    {
                        st = st + ",";            // 最終列でない場合、コンマ
                    }

                }

                if (i < (disp_height - 1))     // 最終行でない場合
                {
                    st = st + "," + "\r\n";    //  次の行にデータあり
                }

            }

            tB_BitMapData.Text = st;        // 表示
        }


        // 　1バイト単位にした、ビットマップ パックデータ 文字列の作成
        //  　0100 0001 ならば、0x41 を作成する。
        //  disp_widthは 8で割り切れる値とする。
        //  
        private void Make_Bit_Map_Pack_Data()
        {
            Boolean b = false;
            string st = "";

            int col_num;
            Byte bd;
            Byte on_bd;
           
            col_num = disp_width / 8;   // 横のバイト数

            for (int i = 0; i < disp_height; i++)  // 縦のピクセル数分　繰り返す
            {
                DataRowView dataRowView = (DataRowView)BitMap_DataGrid.Items[i];  //  i行目のビット情報 

                for (int j = 0; j < col_num; j++)  // 横のバイト数分　繰り返す
                {
                    bd = 0;
                    on_bd = 0x80;

                    for ( int k = 0; k < 8; k++)     // 1バイト単位のデータ作成
                    { 
                       b = (Boolean)dataRowView.Row.ItemArray[j*8+k];  // ( j*8 + k )列のビット情報(true,flase)

                       if ( b == true)
                       {
                         bd = (Byte)( bd | on_bd );
                       }

                       on_bd = (Byte)( on_bd >> 1 );

                    }
                    st = st + "0x" + bd.ToString("x2");

                    if ( j < (col_num - 1) 　)       // 1行の最終バイトでない場合
                    {
                        st = st + ",";
                    }
                }

                if (i < (disp_height - 1))     // 最終行でない場合
                {
                    st = st + "," + "\r\n";    //  次の行にデータあり
                }

            }

            tB_BitMapPackData.Text = st;        // 表示
        }


        //
        //　  DataGridから画像を作成 
        //    WriteableBitmap  

        private void Bit_Map_Image()
        {
            Byte val_r ;
            Byte val_g ;    
            Byte val_b ;    
            
            int disp_dpi = 96;
        
            int pt = 0;         // pixel_buf[pt]へ、データを格納

            WriteableBitmap bitmap = new WriteableBitmap(disp_width, disp_height, disp_dpi, disp_dpi, PixelFormats.Pbgra32, null);

            int size = disp_width * disp_height * 4;

            byte[] pixel_buf = new byte[size];  // 1pixel当たり、4byteの情報が必要 (Alpha値, R, G, B)

                                                　// 各ピクセルのOn/Off情報から、表示用の4byteデータを作成して、pixel_bufへ格納
            for (int i = 0; i < disp_height; i++)  // 縦のピクセル数分　繰り返す
            {
                DataRowView dataRowView = (DataRowView)BitMap_DataGrid.Items[i];  //  i行目のビット情報  

                for (int j = 0; j < disp_width; j++)  // 横のピクセル数分
                {
                    Boolean b = (Boolean)dataRowView.Row.ItemArray[j];  // j列のビット情報(true,flase)
           
                    if (b == true)              // bit=ONの場合、黒で表示)
                    {
                        val_r = 0x00;
                        val_g = 0x00;
                        val_b = 0x00;

                    }
                    else                       // bit = OFFの場合 白
                    {
                        val_r = 0xff;
                        val_g = 0xff;
                        val_b = 0xff;
                    }

                    pixel_buf[pt] = val_b;    // B
                    pt++;
                    pixel_buf[pt] = val_g;    // G
                    pt++;
                    pixel_buf[pt] = val_r;    // R
                    pt++;
                    pixel_buf[pt] = 255;    // Alpha値
                    pt++;
                }
            }
            // pixel_buf[]の情報を、WriteableBitmapへ書き込み
            var stride = disp_width * 4;    // 1行あたりのバイト数
            bitmap.WritePixels(new Int32Rect(0, 0, disp_width, disp_height), pixel_buf, stride, 0, 0);

            Bit_Image.Source = bitmap;          // 表示

        }


        // 
        // チェックボックスが変更された場合の処理
        // 発生タイミング:
        // 1)マウスでチェックボックスをON/OFFした時
        // 2)データテーブル変更時 
        //
        // ・pixel_on_chk_fg:
        //  ビットフォントデータファイル(CSV形式)の読み出し時 (Load_Button_Click)と、
        //  文字入力での画像からビットフォントデータ作成時(Set_Button_Click)に、
        //  RGB565_to_DataTable()が実行されると、このルーチンが、ONのビット数回実行される。
        //  これを回避するため、pixel_on_chk_fg=ONの場合は、1回だけ実行するようにしている。
        //
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            if (pixel_on_chk_fg == true)   // 
            {
                chkbox_checked_cnt++;           // この処理の回数をインクリメント
                if (chkbox_checked_cnt < pixel_on_cnt) // pixelのON回数より小さい場合、何もせずリターン
                {
                    return;
                }
                else                   //  pixelのON回数分、CheckBox_Checkedに入った場合、処理を行う
                {
                    pixel_on_chk_fg = false;
                }
            }

            Bit_Map_Image();            // DataGridから画像を作成 

            Make_Bit_Map_Data();        // ビットマップ文字列　作成
            Make_Bit_Map_Pack_Data();   // 1バイト単位のビットマップ文字列　作成


            if ( set_mov_key_fg == true)        // 設定ボタン、移動ボタンを押された直後は、移動ボタンは有効
            {
                ImageUp_Button.IsEnabled = true;     // UPボタン　有効
                ImageDown_Button.IsEnabled = true;   // Downボタン 有効
                ImageRight_Button.IsEnabled = true;  // Rightボタン 有効
                ImageLeft_Button.IsEnabled = true;   // Leftボタン 有効

            }

            else                        　　// チェックボックスのON/OFFで移動ボタン無効
            {
                ImageUp_Button.IsEnabled = false;    // UPボタン　無効
                ImageDown_Button.IsEnabled = false;  // Downボタン　無効
                ImageRight_Button.IsEnabled = false; // Rightボタン 無効
                ImageLeft_Button.IsEnabled = false;  // Leftボタン 無効
            }

            set_mov_key_fg = false;            // 設定ボタンフラグのクリア
        }


        // 1文字設定ボタンの処理
        private void Set_Char_Button_Click(object sender, RoutedEventArgs e)
        {
           
            int.TryParse(Offset_X_TB.Text, out drawing_tx_pt_x);  // 文字表示位置の指定
            int.TryParse(Offset_Y_TB.Text,out drawing_tx_pt_y);


            Convert_Image_to_Bitmap_data(); // 文字の画像をビットマップデータに変換
            
            set_mov_key_fg = true;         // 設定ボタンが押された
           

        }

        

        // 画像イメージの移動（上へ)
        private void Image_Move_Up_Button_Click(object sender, RoutedEventArgs e)
        {
            drawing_tx_pt_y = drawing_tx_pt_y - 1;
            set_mov_key_fg = true;         // 移動ボタンが押された

            Convert_Image_to_Bitmap_data(); // 文字の画像をビットマップデータに変換
        }
        
        // 画像イメージの移動（左へ)
        private void Image_Move_Left_Button_Click(object sender, RoutedEventArgs e)
        {
            drawing_tx_pt_x = drawing_tx_pt_x - 1;
            set_mov_key_fg = true;         // 移動ボタンが押された

            Convert_Image_to_Bitmap_data(); // 文字の画像をビットマップデータに変換
        }

        // 画像イメージの移動（右へ)
        private void Image_Move_Right_Button_Click(object sender, RoutedEventArgs e)
        {
            drawing_tx_pt_x = drawing_tx_pt_x + 1;
            set_mov_key_fg = true;         // 移動ボタンが押された

            Convert_Image_to_Bitmap_data(); // 文字の画像をビットマップデータに変換
        }
        // 画像イメージの移動（下へ)
        private void Image_Move_Down_Button_Click(object sender, RoutedEventArgs e)
        {
            drawing_tx_pt_y = drawing_tx_pt_y + 1;
            set_mov_key_fg = true;         // 移動ボタンが押された

            Convert_Image_to_Bitmap_data(); // 文字の画像をビットマップデータに変換
        }

        //
        // 入力文字を画像(背景色 白)に変換して、ビットマップデータ(ピクセル毎のON/OFF情報)を作成
        //  サイズは、 em_font_size (作成するビットマップフォントのサイズにより設定）
        //
        private void Convert_Image_to_Bitmap_data()
        {

            int.TryParse(FontSize_TB.Text, out em_font_size); // フォントサイズ入力値の反映

            // 入力文字(Input_TB.TEXT)のフォントタイプとサイズを設定
          FormattedText text = new FormattedText(
              Input_TB.Text,
              System.Globalization.CultureInfo.CurrentUICulture,
              System.Windows.FlowDirection.LeftToRight,
              new Typeface(ft_font_name),
              em_font_size,
              Brushes.Black,
              VisualTreeHelper.GetDpi(this).PixelsPerDip);

       

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, disp_width,disp_height));    // 長方形(白)を描く　枠なし

            drawingContext.DrawText(text, new Point(drawing_tx_pt_x,drawing_tx_pt_y ));  // 文字表示

            drawingContext.Close();

            int pixelWidth = disp_width;         // ビットマップの幅。
            int pixelHeight = disp_height;       // ビットマップの高さ。
            double dpiX = 96;                    //  (dpi) 
            double dpiY = 96;

            RenderTargetBitmap bmp = new RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);

            bmp.Render(drawingVisual);
            
            Src_Image.Source = bmp;                          // 画像を表示

            // Pbgra32: 各カラー チャネル (青、緑、赤、およびアルファ) に割り当てられるピクセルあたりのビット数 (BPP) は 8 
            FormatConvertedBitmap conv_bmap = new FormatConvertedBitmap(bmp, PixelFormats.Pbgra32, null, 0); // BitmapImageのPixelFormatをPbgra32に変換する

            Image_to_rgb565(conv_bmap);  // 画像のRGB565データ作成

            RGB565_to_DataTable();       // RGB565からデータテーブル作成して、BitMap_DataGridのデータとして表示
            
        }




        //  RGB565からデータテーブル作成
        //  BitMap_DataGridのデータとして表示

        // RGB565データと 白/黒の対応　(白: pixel OFF(空欄の意味), 黒: pixel ON )
        // RGB565データ
        //  a) 0xffff  or 0xffdf  : 白 (pixel OFF)
        //  b) 0x0000      　　　 : 黒 (pixel ON)
        //  c) a)以外　　　　　　 : 黒　(pixel ON)
        // 
        // ・a)は、白でも、0xffffとならない場合があるための対応
        //
        private void RGB565_to_DataTable()
        {
      
            int pt =0;
            
            pixel_on_chk_fg = true;     
            pixel_on_cnt = 0;          // ONピクセル数のクリア
            chkbox_checked_cnt = 0;    // イベント(CheckBox_Checked)の処理回数クリア

            DataTable dt = new DataTable();             // データテーブル作成

            for (int i = 0; i < disp_width; ++i)        // データテーブルへチェックボックスのコントロールを配置
            {
                dt.Columns.Add("b" + i.ToString(), typeof(bool));   // データテーブルへカラムを追加 (boolean はチェックボックス)
            }


            for (int i = 0; i < disp_height; i++)
            {
                DataRow dr = dt.NewRow();       //　データテーブルの行データ作成

                for (int j = 0; j < disp_width; j++)  // 横のピクセル数分
                {
                    if ((rgb565_data_buf[pt] == 0xffff) || (rgb565_data_buf[pt] == 0xffdf))     // 白(RGB565)の場合、pixel OFF
                    {
                        dr[j] = false;                       // j列のビット情報(false)
                    }
                    else                                    // 白以外の場合、pixel ON
                    {
                        dr[j] = true;                       // j列のビット情報(true)
                        pixel_on_cnt++;
                    }

                    pt++;                       //　読出し位置のインクリメント

                }
                dt.Rows.Add(dr);         // データテーブルへ1行追加
            }

            BitMap_DataGrid.DataContext = dt;     // データをセットしたデータテーブルをBitMap_DataGridのデータとする。
            

        }

        // ビットマップデータの保存
        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            string path;
            string str_one_line;

            SaveFileDialog sfd = new SaveFileDialog();           //　SaveFileDialogクラスのインスタンスを作成 

            sfd.FileName = "bitmap.csv";                              //「ファイル名」で表示される文字列を指定する

            sfd.Title = "保存先のファイルを選択してください。";        //タイトルを設定する 

            sfd.RestoreDirectory = true;                 //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする

            if (sfd.ShowDialog() == true)            //ダイアログを表示する
            {
                path = sfd.FileName;

                try
                {
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false, System.Text.Encoding.Default);

                    str_one_line = tB_BitMapData.Text;
                    sw.WriteLine(str_one_line);         // 1行保存

                    sw.Close();
                }

                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }

        // 1バイト単位のビットマップデータ(PackData)の保存
        private void Save_Pack_Button_Click(object sender, RoutedEventArgs e)
        {

            string path;
            string str_one_line;

            SaveFileDialog sfd = new SaveFileDialog();           //　SaveFileDialogクラスのインスタンスを作成 

            sfd.FileName = "pack.csv";                              //「ファイル名」で表示される文字列を指定する

            sfd.Title = "保存先のファイルを選択してください。";        //タイトルを設定する 

            sfd.RestoreDirectory = true;                 //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする

            if (sfd.ShowDialog() == true)            //ダイアログを表示する
            {
                path = sfd.FileName;

                try
                {
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false, System.Text.Encoding.Default);

                    str_one_line = tB_BitMapPackData.Text;
                    sw.WriteLine(str_one_line);         // 1行保存

                    sw.Close();
                }

                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

        }






        //  ビットマップデータの読み出しと、データテーブル作成
        //  ビットマップデータの例: "0,0,0,1,1,1,0,0"
        //処理:
        // 1) ビットマップデータを読み出し、rgb565_data_buf[pt]へ格納。
        //  　ビットマップデータの"0"は pixel OFFで、rgb565では0xffff(白）とする。
        //    ビットマップデータの"1"は pixel ONで、rgb565では0x0000(黒）とする。
        // 2) RGB565からデータテーブル作成して、BitMap_DataGridのデータとして表示
        //
        private void Load_Button_Click(object sender, RoutedEventArgs e)
        {

            Load_Data_to_DataTable(0);      
        }

        //  Packデータの読み出しと、データテーブル作成
        //  Packデータの例: "0x1c"
        //処理:
        // 1) Pack データを読み出し、rgb565_data_buf[pt]へ格納。
        //  　ビットマップデータの0は pixel OFFで、rgb565では0xffff(白）とする。
        //    ビットマップデータの1は pixel ONで、rgb565では0x0000(黒）とする。
        // 2) RGB565からデータテーブル作成して、BitMap_DataGridのデータとして表示
        //
        private void Load_Pack_Button_Click(object sender, RoutedEventArgs e)
        {

            Load_Data_to_DataTable(1);
        }

        //  読み出しデータ(ビットデータまたはPackデータ)からデータテーブルの作成
        //
        //   pack_data_flg = 0: ビットデータのロード
        //                 =~1: Packデータのロード
        private void Load_Data_to_DataTable( Byte pack_data_flg)
        {
            var dialog = new OpenFileDialog();   // ダイアログのインスタンスを生成

            dialog.Filter = "CSVファイル (*.csv)|*.csv|全てのファイル (*.*)|*.*";  //  // ファイルの種類を設定

            dialog.RestoreDirectory = true;                 //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする

            if (dialog.ShowDialog() == false)     // ダイアログを表示する
            {
                return;                          // キャンセルの場合、リターン
            }

            try
            {
                StreamReader sr = new StreamReader(dialog.FileName, Encoding.GetEncoding("SHIFT_JIS"));    //  CSVファイルを読みだし

                int pt = 0;
                UInt16 wk = 0;

                for (int i = 0; i < disp_height; i++)
                {
                    string line = sr.ReadLine();    // 1行読みだし
                    string[] fields = line.Split(',');  // カンマ(,)区切りで、分割して配列へ格納


                    if (pack_data_flg == 0)                 // ビットデータ読み出しの場合
                    {
                        for (int j = 0; j < disp_width; j++)  // 横のピクセル数分
                        {
                            string b_str = fields[j];
                            if (b_str == "0")                         // pixel OFFの場合
                            {
                                wk = 0xffff;                       // RGB565 白のデータ
                            }
                            else if (b_str == "1")                   // pixel ONの場合
                            {
                                wk = 0x0000;                       // RGB565 黒のデータ
                            }

                            rgb565_data_buf[pt] = wk;
                            pt++;

                        }
                    }
                    else                                            // Packデータ読み出しの場合
                    {
                        for (int j = 0; j < (disp_width / 8); j++)  // 横のピクセル数分/8  ( 1バイト単位で読み出し)
                        {
                            string hex_str_prefix = fields[j];

                            string hex_str = hex_str_prefix.Substring(2, 2);  // 0x80を 80(16進数)とする 

                            uint.TryParse(hex_str, System.Globalization.NumberStyles.HexNumber, null, out uint hex_data);

                            uint test_bit = 0x80;

                            for (int k = 0; k < 8; k++)
                            {
                                uint on_off_val = (test_bit >> k) & hex_data; // bit毎のOn/OFF判定

                                if (on_off_val == 0)                 // 0の場合 (pixel OFF)
                                {
                                    wk = 0xffff;                       // RGB565 白のデータ
                                }
                                else                                   // 0以外の場合(pixel ON)
                                {
                                    wk = 0x0000;                       // RGB565 黒のデータ
                                }

                                rgb565_data_buf[pt] = wk;

                                pt++;
                            }

                        }
                    }
                }

                RGB565_to_DataTable();       // RGB565からデータテーブル作成して、BitMap_DataGridのデータとして表示

                Input_Str_Image_Clear();     // 文字のイメージをクリア,　移動ボタンの無効化
            }

            catch (Exception ex) when (ex is IOException || ex is IndexOutOfRangeException)
            {

                MessageBox.Show(ex.Message + "\r\n File data mismatch.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }



        //  画像ファイルの読み出しボタンの処理
        // 　画像を読み出して表示
        // 　画像を Pbgra32に変換　　(1 pixelは、B,G,R,A の順)
        //   BGRa(32bit)からRGB565(16bit)へ変換
        //   RGB565データ表示用文字列作成
        //  
        private void Load_image_Button_Click(object sender, RoutedEventArgs e)
        {

            var dialog = new OpenFileDialog();   // ダイアログのインスタンスを生成

            dialog.Filter = "画像 | *.bmp; *.jpg; *.png; *.tif; *.gif; | 全てのファイル (*.*)|*.*";  //  // ファイルの種類を設定

            dialog.RestoreDirectory = true;                 //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする


            if (dialog.ShowDialog() == false)     // ダイアログを表示する
            {
                return;                          // キャンセルの場合、リターン
            }


            try
            {
                BitmapImage bitmapImage = new BitmapImage(new Uri(dialog.FileName, UriKind.Absolute)); // ファイル名の画像を BitmapImage型へ
                Src_Image.Source = bitmapImage;                          // ファイル名の画像を表示


                // Pbgra32: 各カラー チャネル (青、緑、赤、およびアルファ) に割り当てられるピクセルあたりのビット数 (BPP) は 8 
                FormatConvertedBitmap conv_bmap = new FormatConvertedBitmap(bitmapImage, PixelFormats.Pbgra32, null, 0); // BitmapImageのPixelFormatをPbgra32に変換する

                int height = conv_bmap.PixelHeight;    // 高さ pixel数
                int width = conv_bmap.PixelWidth;      // 幅　 pixel数

                tB_Load_image_FileName.Text = dialog.FileName;          // ファイル名の表示
                tB_Load_image_File_pixel_size.Text = width.ToString() + "(W)x" + height.ToString() + "(H)";  // ピクセルサイズの表示


                if ((width != disp_width) || (height != disp_height))   // 読み出した画像のピクセル数が、作成する文字の幅、高さのピクセル数と一致していない場合、
                {
                    MessageBox.Show("画像のピクセル数が、作成文字のピクセル数と一致していません。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Image_to_rgb565(conv_bmap);  // 画像のRGB565データ作成

                RGB565_to_DataTable();       // RGB565からデータテーブル作成して、BitMap_DataGridのデータとして表示



            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        // 画像のRGB565データ作成
        private void Image_to_rgb565(FormatConvertedBitmap bitmap )
        {

                byte[] conv_bmap_array = new byte[disp_width * disp_height * 4];    // 画像ビット情報格納用の配列を作成  

                int stride = disp_width * 4;         // 1行のバイト数

                bitmap.CopyPixels(conv_bmap_array, stride, 0);       // conv_bmapを配列(conv_bmap_array)へコピー

                int pt = 0;

                for (int i = 0; i < conv_bmap_array.Length; i = i + 4)      //   BGRa(32bit)からRGB565(16bit)へ変換
                {
                    Byte b = conv_bmap_array[i];      // Blue
                    Byte g = conv_bmap_array[i + 1];  // Green
                    Byte r = conv_bmap_array[i + 2];  // Red

                    UInt16 r1 = (UInt16)(r >> 3);
                    UInt16 g1 = (UInt16)(g >> 2);
                    UInt16 b1 = (UInt16)(b >> 3);

                    UInt16 wk = (UInt16)((r1 << 11) | (g1 << 5));

                    wk = (UInt16)(wk | b1);

                    rgb565_data_buf[pt] = wk;
                
                    pt++;
                }

                string st = "";

                for (int i = 0; i < rgb565_data_buf.Length; i++)      //   配列要素の文字列作成
                {
                    st = st + "0x" + rgb565_data_buf[i].ToString("x4") + ",";

                    if ((i + 1) % disp_width == 0)
                    {
                        st = st + "\r\n";
                    }
                }

           // tB_RGB565_BitMapData.Text = st;  // RGB565 データ表示 (確認用)
        }


        // 
        // 文字イメージ（Setボタンで設定した文字)をクリア（白の長方形を書く)
        // 文字イメージ移動ボタンを無効
        private void Input_Str_Image_Clear()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, disp_width, disp_height));    // 長方形(白)を描く　枠なし

            drawingContext.Close();

            int pixelWidth = disp_width;         // ビットマップの幅。
            int pixelHeight = disp_height;       // ビットマップの高さ。
            double dpiX = 96;                    //  (dpi) 
            double dpiY = 96;

            RenderTargetBitmap bmp = new RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);

            bmp.Render(drawingVisual);

            Src_Image.Source = bmp;                          // 画像を表示


            ImageUp_Button.IsEnabled = false;    // UPボタン　無効
            ImageDown_Button.IsEnabled = false;  // Downボタン　無効
            ImageRight_Button.IsEnabled = false; // Rightボタン 無効
            ImageLeft_Button.IsEnabled = false;  // Leftボタン 無効


        }

      
    }
}
