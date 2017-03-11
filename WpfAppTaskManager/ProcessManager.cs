using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows;

namespace WpfAppTaskManager
{
    /// <summary>
    /// Class which contains of information about all using processes 
    /// </summary>
    class ProcessManager
    {
        // list of current processes 
        public List<ProcessInfo> ListProcessInfos { get; protected set; }

        public ProcessManager()
        {
            try
            {
                ListProcessInfos = new List<ProcessInfo>();
                ListProcessInfos = (from x in Process.GetProcesses()
                    select new ProcessInfo
                    {
                        ProcessName = GetProcessorsName(x),
                        Id = GetProcessorsId(x),
                        HandleCount = GetProcessorsHandleCount(x),
                        Priority = GetPriority(x),
                        MainWindowTitle = GetProcessorsMainWindowTitle(x),
                        FileName = GetProcessorsFileName(x)
                    }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// Method for constructor 
        /// To avoid critical exception with system processes 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private string GetProcessorsName(Process process)
        {
            string strRet = string.Empty;
            try
            {
                strRet = process.ProcessName;
            }
            catch (Exception ex)
            {
            }
            return strRet;
        }

        /// <summary>
        /// Method for constructor 
        /// To avoid critical exception with system processes 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private int GetProcessorsId(Process process)
        {
            int iRet = -1;
            try
            {
                iRet = process.Id;
            }
            catch (Exception ex)
            {
            }
            return iRet;
        }

        /// <summary>
        /// Method for constructor 
        /// To avoid critical exception with system processes 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private int GetProcessorsHandleCount(Process process)
        {
            int iRet = -1;
            try
            {
                iRet = process.HandleCount;
            }
            catch (Exception ex)
            {
            }
            return iRet;
        }

        /// <summary>
        /// Method for constructor 
        /// To avoid critical exception with system processes 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private string GetPriority(Process process)
        {
            string strRet = string.Empty;
            try
            {
                switch (process.PriorityClass)
                {
                    case ProcessPriorityClass.AboveNormal:
                        strRet = "Above Normal";
                        break;
                    case ProcessPriorityClass.BelowNormal:
                        strRet = "Below Normal";
                        break;
                    case ProcessPriorityClass.High:
                        strRet = "High";
                        break;
                    case ProcessPriorityClass.Idle:
                        strRet = "Idle";
                        break;
                    case ProcessPriorityClass.Normal:
                        strRet = "Normal";
                        break;
                    case ProcessPriorityClass.RealTime:
                        strRet = "Real Time";
                        break;
                }
            }
            catch (Exception ex)
            {
            }
            return strRet;
        }

        /// <summary>
        /// Method for constructor 
        /// To avoid critical exception with system processes 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private string GetProcessorsMainWindowTitle(Process process)
        {
            string strRet = string.Empty;
            try
            {
                strRet = process.MainWindowTitle;
            }
            catch (Exception ex)
            {
            }
            return strRet;
        }

        /// <summary>
        /// Method for constructor 
        /// To avoid critical exception with system processes 
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private string GetProcessorsFileName(Process process)
        {
            string strRet = string.Empty;
            try
            {
                strRet = process.MainModule.FileName;
            }
            catch (Exception ex)
            {
            }
            return strRet;
        }
    }


    /// <summary>
    /// Class with information about current process
    /// </summary>
    public class ProcessInfo
    {
        public string ProcessName { get; set; }
        public int Id { get; set; }
        public int HandleCount { get; set; }
        public string Priority { get; set; }
        public string MainWindowTitle { get; set; }
        public string FileName { get; set; }

        /// <summary>
        /// Method finalize selected process 
        /// </summary>
        public void KillProcess()
        {
            try
            {
                Process process = Process.GetProcessById(Id);
                process.Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}