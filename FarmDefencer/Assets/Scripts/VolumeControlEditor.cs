using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public interface IVolumeControl
{

}

public class VolumeControl : PropertyAttribute
{
    public string Group;

    public VolumeControl(string group = "Common")
    {
        Group = group;
    }
}

public class VolumeControlEditor : EditorWindow
{
    private List<GameObject> _prefabs = new List<GameObject>();
    private Vector2 _scrollPos; // ← 스크롤 상태 저장 변수

    [MenuItem("Tools/Volume Control")]
    static void OpenWindow()
    {
        GetWindow<VolumeControlEditor>("Volume Control Editor");
    }

    private void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos); // 🔽 스크롤 시작

        Color oldColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.4f, 1f, 0.6f);

        if (GUILayout.Button("Reload", GUILayout.Height(30)))
        {
            FindPrefabsWithIVolumeControl();
        }

        GUILayout.Space(30);

        GUI.backgroundColor = oldColor; // 원상복구!

        if (_prefabs == null || _prefabs.Count == 0)
        {
            EditorGUILayout.HelpBox("There is no volume control prefabs. Please click 'Reload' button.", MessageType.Info);
            EditorGUILayout.EndScrollView(); // 🔼 스크롤 끝
            return;
        }

        ///////////////////////////////////////////////////
        /// 프리팹
        ///////////////////////////////////////////////////
        foreach (var prefab in _prefabs)
        {
            var components = prefab.GetComponentsInChildren<Component>(true);

            var hasVolumeControl = false;

            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true)); // 🔽 박스 그룹 시작

            ///////////////////////////////////////////////////
            /// 컴포넌트
            ///////////////////////////////////////////////////
            foreach (var comp in components)
            {
                if (comp == null)
                {
                    continue;
                }

                Type type = comp.GetType();

                if (typeof(IVolumeControl).IsAssignableFrom(type) == false)
                {
                    continue;
                }

                GUILayout.Label($"- Prefab: {prefab.name} / Component: {type.Name}", new GUIStyle(EditorStyles.boldLabel)
                {
                    richText = true,
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    // alignment = TextAnchor.MiddleCenter,
                });
                GUILayout.Space(10);
                EditorGUILayout.ObjectField(GUIContent.none, prefab, typeof(GameObject), true); // 라벨 없이
                GUILayout.Space(10);

                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                ///////////////////////////////////////////////////
                /// 필드
                ///////////////////////////////////////////////////
                foreach (var field in fields)
                {
                    if (Attribute.IsDefined(field, typeof(VolumeControl), true))
                    {
                        var attr = field.GetCustomAttribute<VolumeControl>(true);
                        var value = field.GetValue(comp);

                        GUILayout.BeginVertical();

                        GUILayout.Label($"    • <color=#4FD1C5>Field</color>: {field.Name} | <color=#9F7AEA>Group</color>: {attr.Group} | <color=#63B3ED>Value</color>: {value}", new GUIStyle(EditorStyles.boldLabel)
                        {
                            richText = true,
                            fontSize = 13,
                            fontStyle = FontStyle.Bold,
                            // alignment = TextAnchor.MiddleCenter,
                        });

                        GUILayout.BeginHorizontal();

                        // 🔽 필드 타입에 따라 값 조정 UI 제공
                        if (field.FieldType == typeof(float))
                        {
                            float currentValue = (float)value;
                            float newValue = EditorGUILayout.Slider(currentValue, 0f, 1f, GUILayout.Width(450));

                            if (GUILayout.Button("↺", GUILayout.Width(30)))
                            {
                                newValue = 0.5f; // 초기값으로 복원
                            }

                            if (Mathf.Approximately(currentValue, newValue) == false)
                            {
                                Undo.RecordObject(comp, "Change Volume Value");
                                field.SetValue(comp, newValue);
                                EditorUtility.SetDirty(comp);
                            }
                        }
                        else if (field.FieldType == typeof(int))
                        {
                            int currentValue = (int)value;
                            int newValue = EditorGUILayout.IntField(currentValue, GUILayout.Width(450));

                            if (GUILayout.Button("↺", GUILayout.Width(30)))
                            {
                                newValue = 0; // 초기값으로 복원
                            }

                            if (currentValue != newValue)
                            {
                                Undo.RecordObject(comp, "Change Volume Value");
                                field.SetValue(comp, newValue);
                                EditorUtility.SetDirty(comp);
                            }
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            bool currentValue = (bool)value;
                            bool newValue = EditorGUILayout.Toggle(currentValue, GUILayout.Width(450));

                            if (GUILayout.Button("↺", GUILayout.Width(30)))
                            {
                                newValue = false; // 초기값으로 복원
                            }

                            if (currentValue != newValue)
                            {
                                Undo.RecordObject(comp, "Change Volume Value");
                                field.SetValue(comp, newValue);
                                EditorUtility.SetDirty(comp);
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"<color=#63B3ED>Value</color>: {value}", new GUIStyle(EditorStyles.label)
                            {
                                richText = true
                            });
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();

                        hasVolumeControl = true;
                    }
                }

                GUILayout.Space(10);
            }

            GUILayout.Space(5);

            if (hasVolumeControl == true)
            {
                Color oldColor2 = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.12f, 0.56f, 1f);

                if (GUILayout.Button("Ping", GUILayout.Height(20)))
                {
                    EditorGUIUtility.PingObject(prefab); // 하이라키에서 강조
                    Selection.activeObject = prefab;     // 선택까지 가능
                }

                GUI.backgroundColor = oldColor2;
            }

            GUILayout.EndVertical(); // 🔼 박스 그룹 끝
            GUILayout.Space(30);
        }

        EditorGUILayout.EndScrollView(); // 🔼 스크롤 끝
    }

    private void FindPrefabsWithIVolumeControl()
    {
        _prefabs.Clear();

        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/Project Assets", "Assets/Resources" });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                continue;
            }

            var components = prefab.GetComponentsInChildren<IVolumeControl>(true);
            if (components.Length > 0 && _prefabs.Contains(prefab) == false)
            {
                _prefabs.Add(prefab);
            }
        }
    }
}
