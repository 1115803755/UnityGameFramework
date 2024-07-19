/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/18 19:30:12
* Note  : 框架配置
***************************************************************/
using UnityEngine;

namespace UnityGameFramework.Editor.Settings
{
    [FilePath("ProjectSettings/UGFSettings.asset")]
    public class UGFSettings : ScriptableSingleton<UGFSettings>
    {
        /// <summary>
        /// 工具相关配置文件存放的根目录
        /// </summary>
        public string toolsConfigRootDir = "Assets/";
    }
}
