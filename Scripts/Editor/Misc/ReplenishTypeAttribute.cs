/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/17 11:44:56
* Note  : ����Type��RuntimeAssemblyNames��RuntimeOrEditorAssemblyNames
*         ���Ծ�̬���е��ֶ���Ч
***************************************************************/
using System;

namespace UnityGameFramework.Editor
{
    /// <summary>
    /// �༭���²���Type��RuntimeAssemblyNames��RuntimeOrEditorAssemblyNames
    /// ���Ծ�̬���е��ֶ���Ч
    /// �����Ϊ�����˳��򼯵��¶���������Type��������⣬����procedure
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReplenishTypeAttribute : Attribute
    {
        /// <summary>
        /// trueΪ����ʱassembly������Ϊ�༭��assembly
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
