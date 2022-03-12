using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AillieoUtils.EasyLAN.Sample.Editor
{
    [CustomEditor(typeof(ResourcesManager))]
    public class ResourcesManagerEditor : UnityEditor.Editor
    {
        [CustomPropertyDrawer(typeof(ResourcesManager.ResourceEntry))]
        public class ResourceEntryDrawer : PropertyDrawer
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                Rect left = new Rect(position.position, new Vector2(position.width / 2, position.height));
                Rect right = new Rect(new Vector2(position.x + position.width / 2, position.y), new Vector2(position.width / 2, position.height));
                EditorGUI.PropertyField(left, property.FindPropertyRelative("key"), GUIContent.none);
                EditorGUI.PropertyField(right, property.FindPropertyRelative("value"), GUIContent.none);
            }
        }


        ReorderableList resourceList;

        private void OnEnable()
        {
            SerializedProperty resources = serializedObject.FindProperty("resources");
            resourceList = new ReorderableList(resources.serializedObject, resources);
            resourceList.drawElementCallback = (rect, index, active, focused) =>
                EditorGUI.PropertyField(rect, resources.GetArrayElementAtIndex(index));
        }

        public override void OnInspectorGUI()
        {
            resourceList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
