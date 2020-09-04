using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using RayMMDMaterialEditor.Models.Materials;

namespace RayMMDMaterialEditor {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        private File materialFile;

        public MainWindow() {
            InitializeComponent();
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Material File(.fx)|*.fx|All files|*.*";
            openFileDialog.FilterIndex = 1;
            bool? result = openFileDialog.ShowDialog();
            if (result == true) {
                this.materialFile = File.Load(openFileDialog.FileName);

                var sb = new StringBuilder();
                foreach (var stmt in this.materialFile.Statements) {
                    sb.Append(stmt.Render() + "\n");
                }
                this.tbMaterial.Text = sb.ToString();
            }
        }
    }
}
