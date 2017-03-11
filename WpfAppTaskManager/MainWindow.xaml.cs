using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace WpfAppTaskManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void DelegateFinishedInitialization(object sender, EventArgs args);

        // event informs system about initialization process completing 
        public event DelegateFinishedInitialization InformAboutInitialization;

        //list with processes after least refresh action
        private ProcessManager _processManagerBeforeRefresh = new ProcessManager();

        //list with processes after at start of current refresh action
        private ProcessManager _processManagerAfterRefresh = new ProcessManager();

        // creation of two temporary lists for operation 
        // of refreshing the information in ListView 
        private List<ProcessInfo> tempProcessUniqueBeforeRefresh = new List<ProcessInfo>();

        private List<ProcessInfo> tempProcessUniqueAfterRefresh = new List<ProcessInfo>();

        // list with banned processes
        private List<ProcessInfo> _listOfBannedProcesses = new List<ProcessInfo>();

        private System.Windows.Forms.NotifyIcon _notifyIcon = new NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            // initialization of list with banned processes
            _listOfBannedProcesses.AddRange(
                XmlFileManager.ReadFromXmlFile(ConfigurationManager.AppSettings["FileWithListOfBanned"]));

            InformAboutInitialization += InitializeListView;

            InformAboutInitialization(this, new EventArgs());
        }

        /// <summary>
        /// Method initializes view of ListView using GridView 
        /// And binds columns of gridview to titles of ProcessInfo() class properties
        /// Than, method runs infinite loop, which refreshes information about processes 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void InitializeListView(object sender, EventArgs args)
        {
            try
            {
                GridView gridView = new GridView();
                ListViewMain.View = gridView;

                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Process Name",
                    DisplayMemberBinding = new System.Windows.Data.Binding("ProcessName"),
                    Width = 200,
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Id",
                    DisplayMemberBinding = new System.Windows.Data.Binding("Id"),
                    Width = 80
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Amount of handles",
                    DisplayMemberBinding = new System.Windows.Data.Binding("HandleCount"),
                    Width = 80
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Executing file",
                    DisplayMemberBinding = new System.Windows.Data.Binding("FileName"),
                    Width = 150
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Priority",
                    DisplayMemberBinding = new System.Windows.Data.Binding("Priority"),
                    Width = 80
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Window Title",
                    DisplayMemberBinding = new System.Windows.Data.Binding("MainWindowTitle"),
                    Width = 80
                });

                AnalizeProcesses();

                // Refreshing process after each second 
                while (true)
                {
                    await Task.Run(() =>
                    {
                        Thread.Sleep(1000);
                        PrepareForRefresh();
                    });
                    RefreshProcesses();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method creates first image with processes and details about them
        /// </summary>
        public void AnalizeProcesses()
        {
            try
            {
                ProcessManager manager = new ProcessManager();

                foreach (ProcessInfo process in manager.ListProcessInfos)
                {
                    ListViewMain.Items.Add(process);
                }

                _processManagerBeforeRefresh = manager;
                TextBlockProcessesCounter.Text = ListViewMain.Items.Count.ToString();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method describes system's behavior after button "End process" was clicked.
        /// Method kills current selected process 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonKillProcess_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessInfo process = (ProcessInfo) ListViewMain.SelectedItem;
                process.KillProcess();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method prepares lists for refreshing process 
        /// </summary>
        private void PrepareForRefresh()
        {
            try
            {
                // assignation of new and keast lists 
                if (_processManagerAfterRefresh.ListProcessInfos.Count != 0)
                {
                    _processManagerBeforeRefresh = _processManagerAfterRefresh;
                }

                _processManagerAfterRefresh = new ProcessManager();

                for (int i = 0; i < _processManagerAfterRefresh.ListProcessInfos.Count; i++)
                {
                    if (IsBanned(_processManagerAfterRefresh.ListProcessInfos[i]))
                    {
                        // show information about ended banned process 
                        if (_notifyIcon.Visible == true && Visibility == Visibility.Hidden)
                        {
                            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                            _notifyIcon.BalloonTipTitle = @"Banned process was successfully ended";
                            _notifyIcon.BalloonTipText = _processManagerAfterRefresh.ListProcessInfos[i].FileName;
                            _notifyIcon.ShowBalloonTip(5000);
                        }
                        _processManagerAfterRefresh.ListProcessInfos[i].KillProcess();
                        _processManagerAfterRefresh.ListProcessInfos.RemoveAt(i);
                    }
                }

                // creation of two temporary lists for operation 
                // of refreshing the information in ListView 
                tempProcessUniqueBeforeRefresh.Clear();
                tempProcessUniqueBeforeRefresh.AddRange(_processManagerBeforeRefresh.ListProcessInfos);

                tempProcessUniqueAfterRefresh.Clear();
                tempProcessUniqueAfterRefresh.AddRange(_processManagerAfterRefresh.ListProcessInfos);

                // loop makes search of process, which are located in both lists, and deletes them
                // from both lists 
                if (tempProcessUniqueBeforeRefresh.Count != 0)
                {
                    for (int i = tempProcessUniqueBeforeRefresh.Count - 1; i >= 0; i--)
                    {
                        if (tempProcessUniqueAfterRefresh.Any(x => x.Id == tempProcessUniqueBeforeRefresh[i].Id &&
                                                                   x.MainWindowTitle ==
                                                                   tempProcessUniqueBeforeRefresh[i].MainWindowTitle))
                        {
                            // deleting process
                            tempProcessUniqueAfterRefresh.Remove(
                                tempProcessUniqueAfterRefresh.Where(x => x.Id == tempProcessUniqueBeforeRefresh[i].Id &&
                                                                         x.MainWindowTitle ==
                                                                         tempProcessUniqueBeforeRefresh[i]
                                                                             .MainWindowTitle)
                                    .FirstOrDefault());
                            tempProcessUniqueBeforeRefresh.Remove(tempProcessUniqueBeforeRefresh[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method refreshes table with processes
        /// It compares two lists - list with processes after least refresh action and 
        /// list with processes at the start of current refresh action 
        /// Than, method deletes from ListView information about non-active processes 
        /// And adds new information about new processes
        /// </summary>
        private void RefreshProcesses()
        {
            try
            {
                // after loop method has two lists with distinct processes 

                for (int i = 0; i < ListViewMain.Items.Count; i++)
                {
                    // if the list of processes to delete is empty - quit the loop
                    if (tempProcessUniqueBeforeRefresh.Count == 0)
                    {
                        break;
                    }

                    // if the list of processes to delete has the same process - delete it from ListView, 
                    // list of processes to delete and insert new process (if it is possible)
                    if (
                        tempProcessUniqueBeforeRefresh.Any(
                            x => x.Id == ((ProcessInfo) ListViewMain.Items.GetItemAt(i)).Id &&
                                 x.MainWindowTitle == ((ProcessInfo) ListViewMain.Items.GetItemAt(i)).MainWindowTitle))
                    {
                        // if list of new processes is not empty - us it 
                        if (tempProcessUniqueAfterRefresh.Count > 0)
                        {
                            ListViewMain.Items.RemoveAt(i);
                            ListViewMain.Items.Insert(i, tempProcessUniqueAfterRefresh.First());

                            tempProcessUniqueAfterRefresh.Remove(tempProcessUniqueAfterRefresh.First());
                        }
                        // if list of new processes is emptey - just delete current process prom ListView
                        else
                        {
                            tempProcessUniqueBeforeRefresh.Remove(
                                tempProcessUniqueBeforeRefresh.Where(
                                    x => x.Id == ((ProcessInfo) ListViewMain.Items.GetItemAt(i)).Id &&
                                         x.MainWindowTitle ==
                                         ((ProcessInfo) ListViewMain.Items.GetItemAt(i)).MainWindowTitle)
                                    .FirstOrDefault());

                            ListViewMain.Items.RemoveAt(i);
                        }
                    }
                }

                // if list of new processes is STILL not empty - us it again
                if (tempProcessUniqueAfterRefresh.Count > 0)
                {
                    for (int i = 0; i < tempProcessUniqueAfterRefresh.Count;)
                    {
                        ListViewMain.Items.Add(tempProcessUniqueAfterRefresh.First());
                        tempProcessUniqueAfterRefresh.RemoveAt(0);
                    }
                }

                TextBlockProcessesCounter.Text = ListViewMain.Items.Count.ToString();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method starts new choosen process 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemStartNewTask_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                fileDialog.Title = string.Format("Choose application to run or file to open");
                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ProcessInfo info = new ProcessInfo
                    {
                        FileName = fileDialog.FileName
                    };
                    if (!IsBanned(info))
                    {
                        Process process = Process.Start(fileDialog.FileName);
                    }
                    else
                    {
                        throw new Exception(@"Selected file executes banned process, so you can't choose it to start!");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method runs function of addition to list of banned processes  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemAddToBannedProcesses_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsBanned((ProcessInfo) ListViewMain.SelectedItem))
                {
                    throw new Exception("Selected process has been already in the list of banned processes");
                }
                _listOfBannedProcesses.Add((ProcessInfo) ListViewMain.SelectedItem);

                System.Windows.Forms.MessageBox.Show(
                    @"Selected process was successfully added to the list of banned processes");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method shows information about application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemHelp_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "All banned processes are listed in the file \"ListOfBanned.xml\". This file is situated in the common directory " +
                "with executed file of current application. If you want to expel some process from this list - just delete " +
                "corresponding node in specified file");
        }

        /// <summary>
        /// Method shows all banned processes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemListOfBanned_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string strBanned = string.Empty;

                for (int i = 0; i < _listOfBannedProcesses.Count; i++)
                {
                    strBanned += (i + 1).ToString() + ". " + _listOfBannedProcesses[i].FileName + "\r\n\r\n";
                }

                System.Windows.MessageBox.Show(strBanned, "The list of banned processes");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method inspects is selected process in the list of banned processes
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private bool IsBanned(ProcessInfo process)
        {
            bool bRet = false;

            try
            {
                if (_listOfBannedProcesses.Any(x => x.FileName == process.FileName))
                {
                    bRet = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            return bRet;
        }

        /// <summary>
        /// Method runs when user clicks closing button and sets application to NotifyIcon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemToNotify_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Visibility = Visibility.Hidden;

                _notifyIcon.Icon = new System.Drawing.Icon(ConfigurationManager.AppSettings["Icon"]);
                _notifyIcon.Visible = true;
                _notifyIcon.DoubleClick += Click_OpenMainWindow;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void Click_OpenMainWindow(object sender, EventArgs eventArgs)
        {
            // Show the form when the user double clicks on the notify icon.
            try
            {
                if (Visibility == Visibility.Hidden)
                {
                    Visibility = Visibility.Visible;
                }
                _notifyIcon.Visible = false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Save information about all banned processes to file 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            try
            {
                XmlFileManager.SaveToXmlFile(ConfigurationManager.AppSettings["FileWithListOfBanned"],
                    _listOfBannedProcesses);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
    }
}