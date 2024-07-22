/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/16 16:53:36
* Note  : 脚本格式化工具
***************************************************************/
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace UnityGameFramework.Editor.ScriptTools
{
    public class ScriptEncodingFormatTool
    {
        [MenuItem("Assets/Tools/Script Encoding GB2312->UTF8 no BOM %g", false, 100)]
        private static void CustomMenu()
        {
            /// 例如: 获取Project视图中选定的对象
            Object selectedObject = Selection.activeObject;

            if (selectedObject != null)
            {
                /// 获取选定对象的相对路径
                string relativeAssetPath = AssetDatabase.GetAssetPath(selectedObject);
                /// 获取项目根目录路径
                string projectPath = Path.GetDirectoryName(Application.dataPath);
                /// 获取选定对象的绝对路径
                string absoluteAssetPath = Path.Combine(projectPath, relativeAssetPath);
                /// 获取选定对象的文件名（包括后缀）
                string fileName = Path.GetFileName(relativeAssetPath);

                /// 判断是否是CSharp文件
                if (IsCSharpFile(fileName))
                {
                    ChangeFormat(absoluteAssetPath);
                    EditorUtility.SetDirty(selectedObject);
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.Log("please select a csharp file");
                }
            }
            else
            {
                Debug.LogWarning("please select a csharp file.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Tools/Script Encoding GB2312->UTF8 no BOM %g", true)]
        private static bool ValidateCustomMenu()
        {
            return Selection.activeObject != null;
        }

        /// <summary>
        /// 判断该文件是否是CSharp文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static bool IsCSharpFile(string fileName)
        {
            /// 获取文件扩展名（包括点）
            string fileExtension = Path.GetExtension(fileName);

            /// 将扩展名转换为小写并与 ".cs" 进行比较
            if (fileExtension.ToLower() == ".cs")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 文件格式转码：GB2312转成UTF8
        /// 读取指定的文件，转换成UTF8（无BOM标记）格式后，回写覆盖原文件
        /// </summary>
        /// <param name="sourceFilePath">文件路径</param>
        public static void ChangeFormat(string sourceFilePath)
        {
            string fileContent = File.ReadAllText(sourceFilePath, Encoding.GetEncoding("GB2312"));
            File.WriteAllText(sourceFilePath, fileContent, Encoding.UTF8);
            Debug.Log("Encoding Format End！");
        }
    }
}


