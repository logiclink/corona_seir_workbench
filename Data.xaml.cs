using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LogicLink.Corona {
    /// <summary>
    /// Interaktionslogik für Data.xaml
    /// </summary>
    public partial class Data : Window {
        public Data() {
            InitializeComponent();
        }

        public Data(DataTable tbl): this() {
            this.DataContext = tbl;
        }
    }
}
