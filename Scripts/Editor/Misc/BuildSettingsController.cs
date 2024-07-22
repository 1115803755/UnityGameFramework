/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/19 14:04:55
* Note  : 
***************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GameFramework;
using System.Xml;
using UnityEditor;

namespace UnityGameFramework.Editor
{

    public sealed class BuildSettingsController
    {
        /// <summary>
        /// 
        /// </summary>
        private const string DefaultAssetRootPath = "Assets";

        /// <summary>
        /// 
        /// </summary>
        private readonly string m_ConfigurationPath;

        /// <summary>
        /// 
        /// </summary>
        public List<string> defaultScenes;

        /// <summary>
        /// 
        /// </summary>
        public List<string> searchScenePaths;

        /// <summary>
        /// 
        /// </summary>
        public BuildSettingsController()
        {
            defaultScenes = new List<string>();
            searchScenePaths = new List<string>();

            string rootDir = Settings.UGFSettings.Instance.toolsConfigRootDir ?? Application.dataPath;
            m_ConfigurationPath = Utility.Path.GetRegularPath(Path.Combine(rootDir, "BuildSettings.xml"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            if (!File.Exists(m_ConfigurationPath))
            {
                return false;
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(m_ConfigurationPath);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("UnityGameFramework");
                XmlNode xmlBuildSettings = xmlRoot.SelectSingleNode("BuildSettings");
                XmlNode xmlDefaultScenes = xmlBuildSettings.SelectSingleNode("DefaultScenes");
                XmlNode xmlSearchScenePaths = xmlBuildSettings.SelectSingleNode("SearchScenePaths");

                XmlNodeList xmlNodeList = null;
                XmlNode xmlNode = null;

                xmlNodeList = xmlDefaultScenes.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "DefaultScene")
                    {
                        continue;
                    }

                    string defaultSceneName = xmlNode.Attributes.GetNamedItem("Name").Value;
                    defaultScenes.Add(defaultSceneName);
                }

                xmlNodeList = xmlSearchScenePaths.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "SearchScenePath")
                    {
                        continue;
                    }

                    string searchScenePath = xmlNode.Attributes.GetNamedItem("Path").Value;
                    searchScenePaths.Add(searchScenePath);
                }
            }
            catch
            {
                File.Delete(m_ConfigurationPath);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null));

                XmlElement xmlRoot = xmlDocument.CreateElement("UnityGameFramework");
                xmlDocument.AppendChild(xmlRoot);

                XmlElement xmlBuildSettings = xmlDocument.CreateElement("BuildSettings");
                xmlRoot.AppendChild(xmlBuildSettings);

                XmlElement xmlDefaultScenes = xmlDocument.CreateElement("DefaultScenes");
                xmlBuildSettings.AppendChild(xmlDefaultScenes);

                XmlElement xmlSearchScenePaths = xmlDocument.CreateElement("SearchScenePaths");
                xmlBuildSettings.AppendChild(xmlSearchScenePaths);

                XmlElement xmlElement = null;
                XmlAttribute xmlAttribute = null;

                foreach (var item in defaultScenes)
                {
                    xmlElement = xmlDocument.CreateElement("DefaultScene");
                    xmlAttribute = xmlDocument.CreateAttribute("Name");
                    xmlAttribute.Value = item;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    xmlDefaultScenes.AppendChild(xmlElement);
                }

                foreach (var item in searchScenePaths)
                {
                    xmlElement = xmlDocument.CreateElement("SearchScenePath");
                    xmlAttribute = xmlDocument.CreateAttribute("Path");
                    xmlAttribute.Value = item;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    xmlSearchScenePaths.AppendChild(xmlElement);
                }

                string configurationDirectoryName = Path.GetDirectoryName(m_ConfigurationPath);
                if (!Directory.Exists(configurationDirectoryName))
                {
                    Directory.CreateDirectory(configurationDirectoryName);
                }

                xmlDocument.Save(m_ConfigurationPath);
                AssetDatabase.Refresh();
            }
            catch
            {
                if (File.Exists(m_ConfigurationPath))
                {
                    File.Delete(m_ConfigurationPath);
                }

                return false;
            }

            return true;
        }
    }
}

