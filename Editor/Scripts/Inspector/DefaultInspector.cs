using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Trackman.Editor.Inspector
{
    [CustomEditor(typeof(Object), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class DefaultInspector : UnityEditor.Editor
    {
        #region Methods
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement container = new();

            SerializedProperty property = serializedObject.GetIterator();
            if (property.NextVisible(true))
            {
                do
                {
                    PropertyField field = new(property) { name = $"{property.type}:{property.propertyPath}" };
                    if (property.propertyPath == "m_Script" && serializedObject.targetObject != null) field.SetEnabled(false);
                    container.Add(field);
                }
                while (property.NextVisible(false));
            }

            return container;
        }
        #endregion
    }
}