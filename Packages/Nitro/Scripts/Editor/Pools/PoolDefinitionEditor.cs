using UnityEngine;
using UnityEditorInternal;
using UnityEditor;
using Nitro.Pooling;

namespace NitroEditor
{
    [CustomEditor(typeof(PoolDefinition))]
    public class PoolDefinitionEditor : Editor
    {
        private ReorderableList m_list = null;
        private SerializedProperty poolData = null;

        private void OnEnable()
        {
            poolData = serializedObject.FindProperty("poolData");

            m_list = new ReorderableList(serializedObject, poolData, true, true, true, true);

            m_list.drawHeaderCallback = (Rect r) =>
            {
                EditorGUI.LabelField(r, "Object Pools");
            };

            m_list.elementHeightCallback = (int index) =>
            {
                SerializedProperty current = poolData.GetArrayElementAtIndex(index);

                SerializedProperty Prefab = current.FindPropertyRelative("Prefabs");

                SerializedProperty referenceType = current.FindPropertyRelative("referenceType");

                PoolReferenceType m_type = (PoolReferenceType)referenceType.intValue;

                if (m_type == PoolReferenceType.PREFAB)
                {
                    float prefabs_height = 0;
                    if (Prefab.isExpanded)
                        prefabs_height = EditorGUI.GetPropertyHeight(Prefab, true);
                    return current.isExpanded ? (160 + prefabs_height) : EditorGUIUtility.singleLineHeight;
                }
                else
                {
#if ADDRESSABLES_INSTALLED
                    return current.isExpanded ? 165 : EditorGUIUtility.singleLineHeight;
#else
                    float prefabs_height = 0;
                    if (Prefab.isExpanded)
                        prefabs_height = EditorGUI.GetPropertyHeight(Prefab, true);
                    return current.isExpanded ? (205 + prefabs_height) : EditorGUIUtility.singleLineHeight;
#endif
                }

            };

            m_list.drawElementCallback = (Rect r, int index, bool active, bool focused) =>
            {
                SerializedProperty current = poolData.GetArrayElementAtIndex(index);

                SerializedProperty Label = current.FindPropertyRelative("Label");
                SerializedProperty Priority = current.FindPropertyRelative("Priority");
                SerializedProperty referenceType = current.FindPropertyRelative("referenceType");
                SerializedProperty Prefab = current.FindPropertyRelative("Prefabs");
                SerializedProperty PreallocateCount = current.FindPropertyRelative("PreallocateCount");
                SerializedProperty UsePoolParent = current.FindPropertyRelative("UsePoolParent");

                Rect r_expanded = new Rect(r.x + 10, r.y, r.width - 10, EditorGUIUtility.singleLineHeight);
                current.isExpanded = EditorGUI.Foldout(r_expanded, current.isExpanded, Label.stringValue);

                if (current.isExpanded)
                {
                    // LABEL
                    Rect r_label = DrawControlRect(Label, r_expanded);

                    // PRIORITY
                    Rect r_priotity = DrawControlRect(Priority, r_label);

                    // REF. TYPE
                    Rect r_reference = DrawControlRect(referenceType, r_priotity);
                    
                    Rect r_preallocate = DrawControlRect(PreallocateCount, r_reference);

                    Rect r_poolparent = DrawControlRect(UsePoolParent, r_preallocate);

                    Rect r_type = new Rect(r_poolparent);
                    
                    PoolReferenceType m_type = (PoolReferenceType)referenceType.intValue;

                    switch (m_type)
                    {
                        case PoolReferenceType.PREFAB:

                            r_type = DrawControlRect(Prefab, r_poolparent  , 5 , EditorGUI.GetPropertyHeight(Prefab , true));

                            break;

#if ADDRESSABLES_INSTALLED
                        case PoolReferenceType.LABEL_REFERENCE:
                            SerializedProperty assetlabelReference = current.FindPropertyRelative("assetlabelReference");
                            r_type = DrawControlRect(assetlabelReference, r_poolparent);
                            break;
                        case PoolReferenceType.ASSET_REFERENCE:
                            SerializedProperty assetReference = current.FindPropertyRelative("assetReference");
                            r_type = DrawControlRect(assetReference, r_poolparent);
                            break;
#endif
                        default: goto case PoolReferenceType.PREFAB;
                    }
#if !ADDRESSABLES_INSTALLED
                    if (m_type == PoolReferenceType.ASSET_REFERENCE || m_type == PoolReferenceType.LABEL_REFERENCE)
                    {
                        Rect r_warning = new Rect(r_type);
                        r_warning = GetRectForControl(r_type, EditorGUIUtility.singleLineHeight * 2);
                        EditorGUI.HelpBox(r_warning, $"Please install 'com.unity.addressables' >= 1.8.5  to use {m_type}.", MessageType.Warning);
                    }
#endif
                    
                }

            };
        }

        private Rect DrawControlRect(SerializedProperty property , Rect baserect , float spacing = 5 , float overrideheight = -1)
        {
            float h = overrideheight > 0 ? overrideheight : EditorGUI.GetPropertyHeight(property);
            Rect r = GetRectForControl(baserect, h , spacing);
            EditorGUI.PropertyField(r, property , true);
            return r;
        }

        private Rect GetRectForControl(Rect baserect , float preferedHeight = -1 , float spacing = 5)
        {
            if (preferedHeight <= 0) preferedHeight = EditorGUIUtility.singleLineHeight;

            return new Rect(baserect.x, baserect.yMax + spacing, baserect.width, preferedHeight);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(10);
            m_list.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}