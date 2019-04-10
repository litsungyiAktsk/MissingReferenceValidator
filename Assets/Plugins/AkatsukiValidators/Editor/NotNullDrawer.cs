using UnityEditor;
using UnityEngine;

namespace Akatsuki.Validators
{
    [CustomPropertyDrawer(typeof(NotNullAttribute))]
    public class NotNullDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
            {

                var oldColor = GUI.contentColor;
                label.tooltip = $"`{label.text}` cannot be null!";
                GUI.contentColor = Color.red;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.contentColor = oldColor;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }
}
