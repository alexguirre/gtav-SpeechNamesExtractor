namespace GTA_V_Speech_Names_Extractor
{
    // System
    using System.Collections.Generic;
    using System.Windows;
    using System.IO;
    using System.Text;
    using System.Linq;

    public struct Result
    {
        public string VoiceName { get; set; }
        public string SpeechName { get; set; }
        public int Index { get; set; }
    }

    public class Results
    {
        public ResultsWindow Window { get; set; }

        public List<Result> ResultsCollection { get; } = new List<Result>();

        public Results()
        {
            Window = new ResultsWindow();
            Window.GenerateCodeButton.Click += GenerateCodeClick;
            Window.GenerateListButton.Click += GenerateListClick;
        }

        public void AddResult(Result result)
        {
            ResultsCollection.Add(result);
            Window.ResultsList.Items.Add(result);
        }

        public bool? ShowDialog()
        {
            return Window.ShowDialog();
        }

        void GenerateCodeClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;

            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string path = folderBrowserDialog.SelectedPath;
                string filename;
                if (SetSavedFileNameWindow.ShowDialog(out filename))
                {
                    string fullpath = Path.Combine(path, filename);
                    GenCode(fullpath);
                }
            }

            //Window.Close();
        }

        void GenerateListClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;

            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                string path = folderBrowserDialog.SelectedPath;
                string filename;
                if(SetSavedFileNameWindow.ShowDialog(out filename))
                {
                    string fullpath = Path.Combine(path, filename);

                    using (StreamWriter writer = new StreamWriter(fullpath, true))
                    {
                        foreach (Result r in ResultsCollection)
                        {
                            writer.WriteLine($"{r.VoiceName} = {r.SpeechName.Replace(".PROCESSED", "")}, {r.Index}");
                        }
                    }
                }
            }

            //Window.Close();
        }


        void GenCode(string fullpath)
        {
            using (StreamWriter writer = new StreamWriter(fullpath + ".cs", true))
            {
                StringBuilder staticClassSB = new StringBuilder();

                IEnumerable<IGrouping<string, Result>> separatedResults = ResultsCollection.GroupBy(r => r.VoiceName);

                foreach (IGrouping<string, Result> group in separatedResults)
                {

                    staticClassSB.AppendLine($"        public static class {group.Key}");
                    staticClassSB.AppendLine("        {");

                    string prevSpeechName = "";
                    foreach (Result r in group)
                    {
                        if (r.SpeechName != prevSpeechName)
                        {
                            staticClassSB.AppendLine($"           public static Speech {(r.SpeechName + "_RANDOM").Replace('-', '_').Replace(".PROCESSED", "")} {{ get {{ return new Speech(\"{r.VoiceName}\", \"{r.SpeechName.Replace(".PROCESSED", "")}\", 0); }} }}");
                        }

                        staticClassSB.AppendLine($"           public static Speech {(r.SpeechName + "_" + r.Index.ToString("D2")).Replace('-', '_').Replace(".PROCESSED", "")} {{ get {{ return new Speech(\"{r.VoiceName}\", \"{r.SpeechName.Replace(".PROCESSED", "")}\", {r.Index}); }} }}");

                        prevSpeechName = r.SpeechName;
                    }
                    staticClassSB.AppendLine("        }");
                }

                writer.WriteLine( SpeechStructCode, staticClassSB.ToString());
            }
        }

        const string SpeechStructCode = @"namespace Put.Your.Namespace.Here
{{
    using Rage;

    internal struct Speech
    {{
        public string Voice {{ get; }}
        public string Name {{ get; }}
        public int Index {{ get; }}

        public Speech(string voice, string name, int index)
        {{
            Voice = voice;
            Name = name;
            Index = index;
        }}

        public void PlayOn(Ped ped, SpeechModifier modifier)
        {{
            ped.PlayAmbientSpeech(Voice, Name, Index, modifier);
        }}

{0}
    }}
}}";
    }
}
