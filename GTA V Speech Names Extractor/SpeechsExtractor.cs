namespace GTA_V_Speech_Names_Extractor
{
    // System
    using System;
    using System.Windows;
    using System.Windows.Forms;
    using System.Reflection;
    using System.Globalization;
    using System.Diagnostics;
    using System.Threading;
    using System.Collections.Generic;
    using System.Xml;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Linq;

    internal class SpeechsExtractor
    {
        public static SpeechsExtractor Instance { get; private set; }
        public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        public MainWindow Window { get; }
        public UIManager UIManager { get; private set; }

        public Results Results { get; private set; }

        public Thread CurrentThread { get; set; }

        public SpeechsExtractor(MainWindow window)
        {
            Instance = this;

            Window = window;

        }

        public void InitializeUI()
        {
            Window.Closing += OnWindowClosing;
            Window.ExtractSpeechesButton.Click += ExtractSpeechesButton_Click;
            UIManager = new UIManager(Window);
            UIManager.WindowTitle += " v" + Version;
        }

        string[] selectedFiles = new string[0];
        private void ExtractSpeechesButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.DefaultExt = ".awc";
            fileDialog.Filter = "AWC Files (*.awc)|*.awc";
            fileDialog.Title = "Please select the audio wave container files to parse.";

            bool? result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                List<Result> results = new List<Result>();
                CurrentThread = new Thread(() =>
                {

                    selectedFiles = fileDialog.FileNames;

                    string[] playerVoicesToFix = { "TREVOR_1_NORMAL", "TREVOR_2_NORMAL", "TREVOR_3_NORMAL",
                                                   "FRANKLIN_1_NORMAL", "FRANKLIN_2_NORMAL", "FRANKLIN_3_NORMAL",
                                                   "MICHAEL_1_NORMAL", "MICHAEL_2_NORMAL", "MICHAEL_3_NORMAL",
                                                  };

                    Func<string, string> fixPlayerVoiceNames = (s) =>
                    {

                        if (playerVoicesToFix.Contains(s))
                        {
                            if (s.IndexOf("_1") >= 0)
                                s = s.Remove(s.IndexOf("_1"), 2);
                            if (s.IndexOf("_2") >= 0)
                                s = s.Remove(s.IndexOf("_2"), 2);
                            if (s.IndexOf("_3") >= 0)
                                s = s.Remove(s.IndexOf("_3"), 2);
                        }

                        return s;
                    };


                    foreach (string filename in selectedFiles)
                    {
                        List<string> namesFullPath = new List<string>();
                        
                        string fileText = File.ReadAllText(filename, Encoding.ASCII);

                        MatchCollection m = Regex.Matches(fileText, "X:/GTA5(.*).clip");
                        for (int i = 0; i < m.Count; i++)
                        {
                            string fullPath = m[i].Value;


                            
                            string voiceName = Regex.Match(fullPath, "/(.*)/pc64/").Value;
                            voiceName = voiceName.Remove(voiceName.IndexOf("/pc64/"), 6); // remove /pc64/
                            voiceName = voiceName.Remove(0, voiceName.LastIndexOf("/") + 1); // remove everything before the voice name

                            string speechName = Regex.Match(fullPath, "/pc64/(.*).clip").Value;
                            speechName = speechName.Remove(speechName.IndexOf(".clip"), 5); // remove .clip extension
                            string indexStr = speechName.Substring(speechName.LastIndexOf("_") + 1, 2); // get index, e.g "01"
                            speechName = speechName.Remove(speechName.LastIndexOf("_"), 3); // remove index, e.g "_01"
                            speechName = speechName.Remove(0, speechName.LastIndexOf("/") + 1); // remove /pc64/

                            int index = Int32.Parse(indexStr);

                            results.Add(new Result() { VoiceName = fixPlayerVoiceNames(voiceName), SpeechName = speechName, Index = index });
                        }

                    }

                    UIManager.Dispatcher.Invoke(() =>
                    {
                        Results = new Results();
                        IOrderedEnumerable<Result> orderedResults = results.OrderBy(r => r.Index).OrderBy(r => r.SpeechName).OrderBy(r => r.VoiceName);
                        foreach (Result r in orderedResults)
                        {
                            Results.AddResult(r);
                        }
                        Results.ShowDialog();
                        Results = null;
                    });
                });
                CurrentThread.IsBackground = true;
                CurrentThread.Priority = ThreadPriority.Highest;
                CurrentThread.Start();
                

            }
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CurrentThread != null && CurrentThread.IsAlive)
                CurrentThread.Abort();
        }
    }
}
