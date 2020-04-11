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

namespace GTA_V_Speech_Names_Extractor
{
    /// <summary>
    /// Interaction logic for SetFileNameWindow.xaml
    /// </summary>
    public partial class SetSavedFileNameWindow : Window
    {
        public bool Accepted { get; private set; }

        public SetSavedFileNameWindow()
        {
            InitializeComponent();
            AcceptButton.Click += AcceptButtonClick;
            CancelButton.Click += CancelButtonClick;
        }

        private void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(FileNameTextBox.Text) || String.IsNullOrWhiteSpace(FileNameTextBox.Text))
            {
                MessageBox.Show("Enter a file name.", "Error!");
                return;
            }

            if(FileNameTextBox.Text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
            {
                MessageBox.Show("The file name contains invalid characters.", "Error!");
                return;
            }
            Accepted = true;
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static bool ShowDialog(out string filename)
        {
            SetSavedFileNameWindow win = new SetSavedFileNameWindow();

            win.ShowDialog();

            if (win.Accepted)
            {
                filename = win.FileNameTextBox.Text;
                return true;
            }
            else
            {
                filename = null;
                return false;
            }
        }
    }
}
