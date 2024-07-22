/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/17 11:44:56
* Note  : 补充Type的RuntimeAssemblyNames和RuntimeOrEditorAssemblyNames
*         仅对静态共有的字段生效
***************************************************************/
using System;

namespace UnityGameFramework.Editor
{
    /// <summary>
    /// 编辑器下补充Type的RuntimeAssemblyNames和RuntimeOrEditorAssemblyNames
    /// 仅对静态共有的字段生效
    /// 解决因为划分了程序集导致读不到部分Type定义的问题，比如procedure
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReplenishTypeAttribute : Attribute
    {
        /// <summary>
        /// true为运行时assembly，否则为编辑器assembly
        /// </summary>
        public bool runtime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runtime"></param>
        public ReplenishTypeAttribute(bool runtime)
        {
            this.runtime = runtime;
        }
    }
}
