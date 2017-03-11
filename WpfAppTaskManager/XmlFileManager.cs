using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WpfAppTaskManager
{
    /// <summary>
    /// Class for creation xml-files with list of banned processes
    /// </summary>
    public static class XmlFileManager
    {
        /// <summary>
        /// Method writes the list of banned processes into the file
        /// </summary>
        /// <param name="strPathToXmlFile"></param>
        /// <param name="listOfInfos"></param>
        public static void SaveToXmlFile(string strPathToXmlFile, List<ProcessInfo> listOfInfos)
        {
            try
            {
                XElement root = new XElement("Elements");

                for (int i = 0; i < listOfInfos.Count; i++)
                {
                    root.Add(new XElement("ExecutedFile", listOfInfos[i].FileName));

                }
                root.Save(strPathToXmlFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method returns the list of banned processes from file
        /// </summary>
        /// <param name="strPathToXmlFile"></param>
        /// <returns></returns>
        public static List<ProcessInfo> ReadFromXmlFile(string strPathToXmlFile)
        {
            List<ProcessInfo> listOfInfos = new List<ProcessInfo>();
            if (File.Exists(strPathToXmlFile))
            {
                try
                {
                    XElement root = XElement.Load(strPathToXmlFile);
                    List<XElement> list = root.Elements().ToList();
                    foreach (XElement element in list)
                    {
                        listOfInfos.Add(new ProcessInfo
                        {
                            FileName = element.Value
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return listOfInfos;
        }
    }
}