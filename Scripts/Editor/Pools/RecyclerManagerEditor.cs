//using UnityEngine;
//using UnityEditor;
//using Nitro.Pooling;

//namespace NitroEditor.Pooling
//{
//    [CustomEditor(typeof(RecyclerManager))]
//    public class RecyclerManagerEditor : Editor
//    {
//        /// <summary>
//        /// Recyclebin List
//        /// </summary>
//        private SerializedProperty pools = default;

//        private Vector2 scroll = Vector2.zero;

//        private string searchFilter = default;

//        private void OnEnable()
//        {   
//            pools = serializedObject.FindProperty("Pools");
//        }

//        void RemoveButton(int index)
//        {
//            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
//            if (GUILayout.Button(new GUIContent("x")))
//            {
//                pools.DeleteArrayElementAtIndex(index);
//            }
//            EditorGUI.EndDisabledGroup();
//        }

//        public override void OnInspectorGUI()
//        {
//            EditorGUILayout.LabelField("Manage your Object Pools here", new GUIStyle(EditorStyles.label)
//            {
//                alignment = TextAnchor.MiddleCenter,
//                fontSize = 10,
//                fontStyle = FontStyle.Normal
//            }, GUILayout.Height(16));

//            EditorGUILayout.Space();

//            serializedObject.Update();

//            searchFilter = EditorGUILayout.TextField(string.Empty, searchFilter, GUI.skin.FindStyle("SearchTextField"));

//            GUILayout.Space(5);

//            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(320));

//            if (pools.arraySize > 0)
//                for (int i = 0; i < pools.arraySize; i++)
//                {
//                    SerializedProperty current = pools.GetArrayElementAtIndex(i);

//                    SerializedProperty _label = current.FindPropertyRelative("label");

//                    if (!string.IsNullOrEmpty(searchFilter) && !_label.stringValue.ToLower().Contains(searchFilter.ToLower())) continue;

//                    bool isvalid = !string.IsNullOrEmpty(_label.stringValue);

//                    GUILayout.BeginVertical(GUI.skin.box);

//                    EditorGUILayout.BeginHorizontal();

//                    GUILayout.Space(10);

//                    current.isExpanded = EditorGUILayout.Foldout(current.isExpanded, isvalid ? _label.stringValue : "(No label)");

//                    GUILayout.FlexibleSpace();

//                    RemoveButton(i);

//                    EditorGUILayout.EndHorizontal();

//                    if (i < pools.arraySize && i >= 0)
//                    {
//                        SerializedProperty _maxitems = current.FindPropertyRelative("MaxItems");

//                        SerializedProperty _prefab = current.FindPropertyRelative("Prefab");

//                        SerializedProperty _preloadcount = current.FindPropertyRelative("PreAllocateCount");

//                        SerializedProperty _PoolParent = current.FindPropertyRelative("PoolParent");


//                        if (current.isExpanded)
//                        {

//                            EditorGUILayout.Space();

//                            _label.stringValue = EditorGUILayout.TextField("Label", _label.stringValue);

//                            _maxitems.intValue = EditorGUILayout.IntField("Max Items", Mathf.Clamp(_maxitems.intValue, 1, int.MaxValue));

//                            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

//                            _preloadcount.intValue = EditorGUILayout.IntField("Pre-Allocate Count",
//                                Mathf.Clamp(_preloadcount.intValue, 0, _maxitems.intValue));

//                            _prefab.objectReferenceValue = EditorGUILayout.ObjectField("Prefab", _prefab.objectReferenceValue,
//                               typeof(GameObject), false);

//                            _PoolParent.objectReferenceValue = EditorGUILayout.ObjectField("Prefab Parent", _PoolParent.objectReferenceValue,
//                                typeof(Transform), true);

//                            EditorGUI.EndDisabledGroup();
//                        }

//                    }

//                    GUILayout.Space(2);
//                    GUILayout.EndVertical();
//                }
//            else
//            {
//                EditorGUILayout.BeginHorizontal();

//                GUILayout.FlexibleSpace();

//                EditorGUILayout.BeginVertical();

//                GUILayout.FlexibleSpace();

//                GUILayout.Label("No object pools found.");

//                GUILayout.FlexibleSpace();

//                EditorGUILayout.EndVertical();

//                GUILayout.FlexibleSpace();

//                EditorGUILayout.EndHorizontal();
//            }

//            EditorGUILayout.EndScrollView();


//            if (!Application.isPlaying)
//            {
//                GUILayout.Space(2);

//                GUILayout.BeginHorizontal();

//                GUILayout.FlexibleSpace();

//                if (GUILayout.Button("Add new", GUI.skin.FindStyle("LargeButton"), GUILayout.Height(25f), GUILayout.Width(150)))
//                {
//                    int index = pools.arraySize > 0 ? pools.arraySize : 0;
//                    pools.InsertArrayElementAtIndex(index);
//                    SerializedProperty prop = pools.GetArrayElementAtIndex(index);
//                    SerializedProperty label = prop.FindPropertyRelative("label");
//                    label.stringValue = string.Empty;
//                }

//                GUILayout.FlexibleSpace();

//                GUILayout.EndHorizontal();
//                GUILayout.Space(5f);
//            }


//            serializedObject.ApplyModifiedProperties();

//        }
//    }
//}
