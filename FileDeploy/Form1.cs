using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;

namespace FileDeploy
{
    public partial class Form1 : Form
    {
        #region Define

        public struct TemplateSetting
        {

        }

        public struct TemplateParamSetting
        {
            public static TemplateParamSetting DefaultSetting
            {
                get
                {
                    TemplateParamSetting template;
                    template.MultiLine = false;
                    template.Default = null;

                    return template;
                }
            }

            public bool MultiLine;
            public string Default;
        }

        #endregion

        #region Hot Key

        private KeyboardHook m_globalHotKeyHook = new KeyboardHook();

        private void OnInit_HotKey()
        {
            // register the event that is fired after the key press.
            m_globalHotKeyHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(OnAfterHotKeyPressed);

            // register the control + alt + F12 combination as hot key.
            m_globalHotKeyHook.RegisterHotKey(FileDeploy.ModifierKeys.Control | FileDeploy.ModifierKeys.Alt, Keys.F12);
        }

        private void OnAfterHotKeyPressed(object sender, KeyPressedEventArgs e)
        {
            SwitchVisibility();


            // show the keys pressed in a label.
            Console.WriteLine(e.Modifier.ToString() + " + " + e.Key.ToString());
        }

        private void SwitchVisibility()
        {
            Visible = !Visible;
            if (Visible)
            {
                Activate();
                templatesSearchBox.Text = "";
                templatesSearchBox.Focus();
            }
        }

        #endregion

        #region Template Analysis

        private const string MAIN_LUA_FILE_NAME = "__main.lua";
        private const string COPY_FILE_NAME = "__copy.txt";
        private const string COPY_DIR_NAME = "__copy";

        private struct SearchSortStruct
        {
            public int MatchCount;
            public int EditLength;
            public int IndexToTemplate;
        }

        private class SearchSorterClass
        {
            public static int Compare(SearchSortStruct x, SearchSortStruct y)
            {
                if (x.MatchCount < y.MatchCount)
                {
                    return -1;
                }

                if (x.MatchCount > y.MatchCount)
                {
                    return 1;
                }

                if (x.EditLength > y.EditLength)
                {
                    return -1;
                }

                if (x.EditLength < y.EditLength)
                {
                    return 1;
                }

                return 0;
            }

            public static int CompareReverse(SearchSortStruct x, SearchSortStruct y)
            {
                return -Compare(x, y);
            }
        }

        private string[] m_templatesDirectory = {
            "./Template",
        };

        private const string m_tempCopyDirPath = "./Temp";

        private string m_currentlyUsingTemplate;

        private List<string> m_allAvailableTemplates;

        private void OnInit_Analysis()
        {
            AnalyzeAllTemplates();
        }

        private DirectoryInfo GetTemplatesDirectory(bool createIfNotFound = false)
        {
            DirectoryInfo result = null;

            foreach (var dir in m_templatesDirectory)
            {
                result = new DirectoryInfo(dir);
                if (result != null && result.Exists)
                {
                    return result;
                }
            }

            return Directory.CreateDirectory(m_templatesDirectory[0]);
        }

        private bool AnalyzeAllTemplates()
        {
            if (m_templatesDirectory.Length == 0)
            {
                return false;
            }

            m_allAvailableTemplates = new List<string>();

            DirectoryInfo templatesDirectory = GetTemplatesDirectory(true);
            var templateDirectories = templatesDirectory.GetDirectories();
            foreach (var dir in templateDirectories)
            {
                m_allAvailableTemplates.Add(dir.Name);
            }

            m_currentlyUsingTemplate = templatesDirectory.FullName;

            currentTemplateDirLable.Text = "Current Template Dir:\n" + m_currentlyUsingTemplate;

            return true;
        }

        #endregion

        #region Search Box

        private void ClearTemplateListBox()
        {
            templatesListBox.Items.Clear();
        }

        private void ShowCurrentSearchResult()
        {
            ClearTemplateListBox();

            if (templatesSearchBox.TextLength == 0)
            {
                // Show all result
                foreach (var template in m_allAvailableTemplates)
                {
                    templatesListBox.Items.Add(template);
                }
            }
            else
            {
                string searchString = templatesSearchBox.Text;
                Regex rx = new Regex(searchString, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                int length = m_allAvailableTemplates.Count;
                SearchSortStruct[] editLengths = new SearchSortStruct[length];
                for (int i = 0; i < length; i++)
                {
                    string inputStr = m_allAvailableTemplates[i];

                    SearchSortStruct sortStruct;
                    sortStruct.MatchCount = rx.Matches(inputStr).Count;
                    sortStruct.EditLength = EditLengthAlgorithm.GetLevenshteinDistance(searchString, inputStr);
                    sortStruct.IndexToTemplate = i;
                    editLengths[i] = sortStruct;
                }

                Array.Sort(editLengths, SearchSorterClass.CompareReverse);
                for (int i = 0; i < length; i++)
                {
                    templatesListBox.Items.Add(m_allAvailableTemplates[editLengths[i].IndexToTemplate]);
                }
            }
        }

        private void templatesSearchBox_TextChanged(object sender, EventArgs e)
        {
            ShowCurrentSearchResult();
        }

        private void templatesSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                templatesListBox.Focus();
            }
        }

        private void templatesListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
                OnAfterSelectTemplate(templatesListBox.SelectedItem as string);
            }
        }

        #endregion
        
        #region Parameter Input

        private class ParameterInputBox
        {
            public FlowLayoutPanel HorizontalPanel;

            public Label ShowNameLabel;

            public TextBox InputTextBox;
            public Size InputTextBoxSize;
        }

        private string m_currentRenderingTemplate;
        private ParameterInputBox[] m_parameterInputBoxes;

        private FileInfo m_copyFile;
        private FileInfo m_mainFile;
        private DirectoryInfo m_copyDir;

        private Size m_inputBoxSize = new Size(200, 21);

        private ParameterInputBox CreateParameterInputBox(string paramName, int pos, TemplateParamSetting templateParamSetting)
        {
            ParameterInputBox result = new ParameterInputBox();

            result.HorizontalPanel = new FlowLayoutPanel();
            result.HorizontalPanel.FlowDirection = FlowDirection.LeftToRight;
            result.HorizontalPanel.Size = new Size(250, 150);
            result.HorizontalPanel.AutoSize = true;

            parameterInputLayout.Controls.Add(result.HorizontalPanel);

            result.ShowNameLabel = new Label();
            result.ShowNameLabel.AutoSize = true;
            result.ShowNameLabel.TextAlign = ContentAlignment.MiddleLeft;
            result.HorizontalPanel.Controls.Add(result.ShowNameLabel);


            result.InputTextBox = new TextBox();
            result.InputTextBoxSize = result.InputTextBox.Size;
            if (templateParamSetting.MultiLine)
            {
                result.InputTextBox.Multiline = true;
                result.InputTextBox.TextChanged += InputTextBox_MultiLine_AfterTextChanged;
                InputTextBox_MultiLine_AfterTextChanged(result.InputTextBox, null);
            }
            result.InputTextBox.GotFocus += InputTextBox_GotFocus;
            result.HorizontalPanel.Controls.Add(result.InputTextBox);

            var newLocation = result.InputTextBox.Location;
            newLocation.X += result.ShowNameLabel.Size.Width;
            result.InputTextBox.Location = newLocation;

            return result;
        }

        private void InputTextBox_GotFocus(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
        }

        private void InputTextBox_MultiLine_AfterTextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Size sz = new Size(textBox.ClientSize.Width, int.MaxValue);

            TextFormatFlags flags = TextFormatFlags.WordBreak;
            int padding = 3;
            int borders = textBox.Height - textBox.ClientSize.Height;

            string textToMesure = textBox.Text.Length == 0 ? "a" : textBox.Text;
            sz = TextRenderer.MeasureText(textToMesure, textBox.Font, sz, flags);

            int h = sz.Height + borders + padding;
            if (textBox.Top + h > this.ClientSize.Height - 10)
            {
                h = this.ClientSize.Height - 10 - textBox.Top;
            }

            int w = sz.Width + borders + padding;
            if (textBox.Left + w > this.ClientSize.Width - 10)
            {
                w = this.ClientSize.Width - 10 - textBox.Left;
            }
            else if (w < 100)
            {
                w = 100;
            }

            textBox.Height = h;
            textBox.Width = w;
        }

        private void CleanSelectTemplate()
        {
            if (m_parameterInputBoxes != null)
            {
                parameterInputLayout.Controls.Clear();
                m_parameterInputBoxes = null;
            }
            
            comfirmInputButton.Enabled = false;
            m_mainFile = null;
            m_copyFile = null;
        }

        private void OnAfterSelectTemplate(string template)
        {
            if (m_currentRenderingTemplate != template)
            {
                CleanSelectTemplate();
            }
            else
            {
                return;
            }

            m_currentRenderingTemplate = template;

            DirectoryInfo templateDir = GetTemplateDir(template);
            if (templateDir == null || !templateDir.Exists)
            {
                OnAfterNotifyIconMenuClick_RefreshLib(null, null);
                return;
            }

            foreach (var file in templateDir.GetFiles())
            {
                if (file.Name == MAIN_LUA_FILE_NAME)
                {
                    m_mainFile = file;
                }

                if (file.Name == COPY_FILE_NAME)
                {
                    m_copyFile = file;
                }
            }

            foreach (var dir in templateDir.GetDirectories())
            {
                if (dir.Name == COPY_DIR_NAME)
                {
                    m_copyDir = dir;
                }
            }

            comfirmInputButton.Enabled = m_copyFile != null || m_copyDir != null;

            if (m_mainFile != null)
            {
                string luaCode = File.ReadAllText(m_mainFile.FullName);

                Script luaStateScript = new Script();

                try
                {
                    luaStateScript.DoString(luaCode);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }

                Table luaParametersTable = luaStateScript.Globals.Get("Params").Table;
                List<DynValue> keys = luaParametersTable.Keys.ToList();
                int keysCount = keys.Count;
                m_parameterInputBoxes = new ParameterInputBox[keysCount];
                for (int i = 0; i < keysCount; i++)
                {
                    DynValue dynValue = keys[i];
                    TemplateParamSetting setting = GetParamSetting(luaParametersTable.Get(dynValue));

                    var inputBox = CreateParameterInputBox(dynValue.String, i, setting);
                    inputBox.ShowNameLabel.Text = dynValue.String;
                    inputBox.InputTextBox.Text = setting.Default != null ? setting.Default : "";
                    m_parameterInputBoxes[i] = inputBox;
                }
            }
        }

        private TemplateParamSetting GetParamSetting(DynValue paramSettingTable)
        {
            TemplateParamSetting result = TemplateParamSetting.DefaultSetting;

            Table table = paramSettingTable.Table;
            if (table == null)
            {
                return result;
            }

            // TODO: Use reflection
            result.MultiLine = true; // table.Get("MultiLine").IsNotNil(); // Seems that multiline should always be true
            result.Default = table.Get("Default").String;

            return result;
        }

        private DirectoryInfo GetTemplateDir(string template)
        {
            return new DirectoryInfo(Path.Combine(m_currentlyUsingTemplate, template));
        }

        #endregion

        #region Comfirm

        private string ProcessString(Script luaStateScript, string inputString)
        {
            Regex parameterMatch = new Regex(@"%[\w\d]+%");
            MatchEvaluator evaluator = delegate (Match match)
            {
                string matchParameterName = match.Value.Substring(1, match.Value.Length - 2);
                DynValue dynValue = luaStateScript.Globals.Get(matchParameterName);
                if (dynValue != null)
                {
                    return dynValue.String;
                }
                else
                {
                    return "";
                }
            };

            return parameterMatch.Replace(inputString, evaluator);
        }

        private void OnAfterConfirmCopy(FileInfo mainFile, FileInfo copyFile, DirectoryInfo copyDir, Dictionary<string, string> inputParameters)
        {
            if (copyFile == null && copyDir == null)
            {
                return;
            }

            Script luaStateScript = new Script();
            foreach (var inputParam in inputParameters)
            {
                // Sets the global variables
                luaStateScript.Globals.Set(DynValue.NewString(inputParam.Key), DynValue.NewString(inputParam.Value));
            }

            if (mainFile != null)
            {
                string luaCode = File.ReadAllText(m_mainFile.FullName);

                try
                {
                    luaStateScript.DoString(luaCode);
                    luaStateScript.Call(luaStateScript.Globals.Get("Main"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }
            }

            if (copyFile != null)
            {
                CopyFileToClipBoard(luaStateScript, copyFile);
            }
            else if (copyDir != null)
            {
                CopyDirAndFilesToClipBoard(luaStateScript, copyDir);
            }

            Visible = false;
        }

        private void comfirmInputButton_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> inputParameters = new Dictionary<string, string>();
            if (m_parameterInputBoxes != null)
            {
                foreach (var inputBox in m_parameterInputBoxes)
                {
                    inputParameters.Add(inputBox.ShowNameLabel.Text, inputBox.InputTextBox.Text);
                }
            }

            OnAfterConfirmCopy(m_mainFile, m_copyFile, m_copyDir, inputParameters);
        }

        private void CopyFileToClipBoard(Script luaStateScript, FileInfo copyFile)
        {
            string copyString = File.ReadAllText(copyFile.FullName);
            string result = ProcessString(luaStateScript, copyString);
            Clipboard.SetText(result);
        }

        private void ProcessAndDuplicateFiles(Script luaStateScript, DirectoryInfo from, DirectoryInfo to)
        {
            foreach (var fileInfo in from.GetFiles())
            {
                string processedFileName = ProcessString(luaStateScript, fileInfo.Name);
                string toFileName = Path.Combine(to.FullName, processedFileName);

                if (IsBinaryFile(fileInfo))
                {
                    // Binary file, ignore them
                    string fromFileName = Path.Combine(from.FullName, processedFileName);
                    File.Copy(fromFileName, toFileName);
                }
                else
                {
                    string fileContent = File.ReadAllText(fileInfo.FullName);
                    string processedContent = ProcessString(luaStateScript, fileContent);
                    File.WriteAllText(toFileName, processedContent);
                }
            }

            foreach (var dirInfo in from.GetDirectories())
            {
                string processedFileName = ProcessString(luaStateScript, dirInfo.Name);
                string toFileName = Path.Combine(to.FullName, processedFileName);
                DirectoryInfo newDir = Directory.CreateDirectory(toFileName);

                ProcessAndDuplicateFiles(luaStateScript, dirInfo, newDir);
            }
        }

        private void CopyDirAndFilesToClipBoard(Script luaStateScript, DirectoryInfo copyDir)
        {
            DirectoryInfo tempDir = new DirectoryInfo(m_tempCopyDirPath);
            if (tempDir.Exists)
            {
                Directory.Delete(tempDir.FullName, true);
            }
            tempDir = Directory.CreateDirectory(m_tempCopyDirPath);

            ProcessAndDuplicateFiles(luaStateScript, copyDir, tempDir);

            StringCollection collection = new StringCollection();
            foreach (var fileInfo in tempDir.GetFiles())
            {
                collection.Add(fileInfo.FullName);
            }
            foreach (var dirInfo in tempDir.GetDirectories())
            {
                collection.Add(dirInfo.FullName);
            }
            Clipboard.SetFileDropList(collection);
        }

        #endregion

        #region Notify Icon

        private void OnInit_NotifyIcon()
        {
            this.notifyIcon1.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.notifyIcon1.ContextMenuStrip.Items.Add("RefreshTemplate", null, this.OnAfterNotifyIconMenuClick_RefreshLib);
            this.notifyIcon1.ContextMenuStrip.Items.Add("Quit", null, this.OnAfterNotifyIconMenuClick_Quit);
        }

        private void OnAfterNotifyIconMenuClick_Quit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnAfterNotifyIconMenuClick_RefreshLib(object sender, EventArgs e)
        {
            CleanSelectTemplate();
            AnalyzeAllTemplates();
            ShowCurrentSearchResult();
        }

        #endregion

        #region Helper

        private static byte[] s_binaryFileBytePattern = new byte[]{
            0, 0
        };

        private static byte[] s_binaryCheckBuffer = new byte[1024];


        private static bool IsBinaryFile(FileInfo file)
        {
            //using (FileStream fs = new FileStream(file.FullName, FileMode.Open))
            //{
            //    int readBytes = fs.Read(s_binaryCheckBuffer, 0, s_binaryCheckBuffer.Length);
            //    if (readBytes < 3 || readBytes < s_binaryFileBytePattern.Length)
            //    {
            //        return true;
            //    }

            //    for (int i = 0; i < readBytes; i++)
            //    {

            //    }
            //}

            return false;
        }

        //private static int MatchPattern(byte[] buffer, int offset, byte[] pattern)
        //{

        //}

        #endregion

        public Form1()
        {
            InitializeComponent();

            OnInit_HotKey();
            OnInit_Analysis();
            OnInit_NotifyIcon();

            ShowCurrentSearchResult();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Icon = null;
            notifyIcon1.Dispose();
            System.Windows.Forms.Application.DoEvents();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            SwitchVisibility();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DirectoryInfo dir = GetTemplatesDirectory(true);
            System.Diagnostics.Process.Start("Explorer.exe", dir.FullName);
        }
    }
}
