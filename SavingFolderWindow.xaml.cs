using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BITTSyReporting
{
    /// <summary>
    /// Interaction logic for SavingFolderWindow.xaml
    /// </summary>
    public partial class SavingFolderWindow : Window
    {
        // Anytime that multiple reports are being generated for multiple log files, each report will be saved
        // into its own file, so a folder path needs to be selected instead of a file path for the
        // save location - in that case, this prompt appears to let the user select a folder path and stub filename
        // (see the explanations given on the Saving Folder Window for more info)

        public String folderpath = "";

        public SavingFolderWindow()
        {
            InitializeComponent();

            this.Owner = System.Windows.Application.Current.MainWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    folderpath = folderDialog.SelectedPath;
                }
            }
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            if (folderpath.Equals(""))
            {
                System.Windows.MessageBox.Show("No Save Location Selected Yet!", "Error");
            }
            else
            {
                this.Close();
            }
        }
    }
}
