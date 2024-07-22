/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/18 19:40:26
* Note  : 
***************************************************************/
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace UnityGameFramework.Editor.Settings
{
    public class SettingsPresetReceiver : PresetSelectorReceiver
    {
        /// <summary>
        /// 
        /// </summary>
        private Object m_Target;

        /// <summary>
        /// 
        /// </summary>
        private Preset m_InitialValue;

        /// <summary>
        /// 
        /// </summary>
        private SettingsProvider m_Provider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="provider"></param>
        public void Init(Object target, SettingsProvider provider)
        {
            m_Target = target;
            m_InitialValue = new Preset(target);
            m_Provider = provider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selection"></param>
        public override void OnSelectionChanged(Preset selection)
        {
            if (selection != null)
            {
                Undo.RecordObject(m_Target, "Apply Preset " + selection.name);
                selection.ApplyTo(m_Target);
            }
            else
            {
                Undo.RecordObject(m_Target, "Cancel Preset");
                m_InitialValue.ApplyTo(m_Target);
            }
            m_Provider.Repaint();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selection"></param>
        public override void OnSelectionClosed(Preset selection)
        {
            OnSelectionChanged(selection);
            Object.DestroyImmediate(this);
        }
    }
}
