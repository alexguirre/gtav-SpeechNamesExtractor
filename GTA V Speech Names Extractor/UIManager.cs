namespace GTA_V_Speech_Names_Extractor
{
    // System
    using System;
    using System.Windows.Threading;
    using System.ComponentModel;

    internal class UIManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow Window { get; }

        public Dispatcher Dispatcher { get { return Window.Dispatcher; } }

        private string windowTitle = "GTA V Speech Names Extractor";
        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                if (value != windowTitle)
                {
                    windowTitle = value;
                    RaisePropertyChanged(nameof(WindowTitle));
                }
            }
        }

        public UIManager(MainWindow window)
        {
            Window = window;
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
