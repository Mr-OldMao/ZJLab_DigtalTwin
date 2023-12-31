#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace MFramework
{
    /// <summary>
    /// 标题：自定义枚举特性
    /// 功能：对Enum添加[EnumLabel]特性，可在编辑器上映射特性中的内容
    /// 作者：毛俊峰
    /// 时间：2022.11.30
    /// </summary>
    public class EnumLabelAttribute : HeaderAttribute
    {
        public EnumLabelAttribute(string header) : base(header)
        {

        }
    }

    [CustomPropertyDrawer(typeof(EnumLabelAttribute))]
    public class EnumLabelDrawer : PropertyDrawer
    {
        private readonly List<string> m_displayNames = new List<string>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = (EnumLabelAttribute)attribute;
            var type = property.serializedObject.targetObject.GetType();
            var field = type.GetField(property.name);
            var enumtype = field.FieldType;
            foreach (var enumName in property.enumNames)
            {
                var enumfield = enumtype.GetField(enumName);
                var hds = enumfield.GetCustomAttributes(typeof(HeaderAttribute), false);
                m_displayNames.Add(hds.Length <= 0 ? enumName : ((HeaderAttribute)hds[0]).header);
            }
            EditorGUI.BeginChangeCheck();
            var value = EditorGUI.Popup(position, att.header, property.enumValueIndex, m_displayNames.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                property.enumValueIndex = value;
            }
        }
    }

}
#endif 
