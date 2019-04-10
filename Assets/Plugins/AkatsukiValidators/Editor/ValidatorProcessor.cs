using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Akatsuki.Validators
{
    public static class ValidatorProcessor
    {
        public static string SceneName => "MissingReferenceReport";
        public static string ScenePath => "Assets/Scenes/MissingReferenceReport.unity";

        private const string VALIDATOR_PROGRESSBAR_ENABLED = "Akatsuki/Validator/Progress Bar/Enabled";
        private const string VALIDATOR_PROGRESSBAR_DISABLED = "Akatsuki/Validator/Progress Bar/Disabled";

        private const string VALIDATOR_SHOW_REPORT_SCENE = "Akatsuki/Validator/Repore Scene/Show";
        private const string VALIDATOR_HIDE_REPORT_SCENE = "Akatsuki/Validator/Repore Scene/Hide";

        private class PrefabInfo
        {
            public string Path;
            public GameObject GameObject;
        }

        private class GameObjectInfo
        {
            public string Path;
            public GameObject GameObject;
        }

        static ValidatorProcessor()
        {
            IsProgressBarEnabled = !Application.isBatchMode;
            IsReportSceneShow = !Application.isBatchMode;
        }

        #region Toggle

        private static bool IsProgressBarEnabled
        {
            get;
            set;
        }

        [MenuItem(VALIDATOR_PROGRESSBAR_ENABLED)]
        public static void EnableProgressBar()
        {
            IsProgressBarEnabled = true;
        }

        [MenuItem(VALIDATOR_PROGRESSBAR_ENABLED, true)]
        public static bool CheckProgressBarEnabled()
        {
            Menu.SetChecked(VALIDATOR_PROGRESSBAR_ENABLED, IsProgressBarEnabled);
            return !IsProgressBarEnabled;
        }

        [MenuItem(VALIDATOR_PROGRESSBAR_DISABLED)]
        public static void DisableProgressBar()
        {
            IsProgressBarEnabled = false;
        }

        [MenuItem(VALIDATOR_PROGRESSBAR_DISABLED, true)]
        public static bool CheckProgressBarShow()
        {
            Menu.SetChecked(VALIDATOR_PROGRESSBAR_DISABLED, !IsProgressBarEnabled);
            return IsProgressBarEnabled;
        }

        private static bool IsReportSceneShow
        {
            get;
            set;
        }

        [MenuItem(VALIDATOR_SHOW_REPORT_SCENE)]
        public static void CheckReportSceneShow()
        {
            IsReportSceneShow = true;
        }

        [MenuItem(VALIDATOR_SHOW_REPORT_SCENE, true)]
        public static bool CheckProgressBarHide()
        {
            Menu.SetChecked(VALIDATOR_SHOW_REPORT_SCENE, IsReportSceneShow);
            return !IsReportSceneShow;
        }

        [MenuItem(VALIDATOR_HIDE_REPORT_SCENE)]
        public static void HideReportScene()
        {
            IsReportSceneShow = false;
        }

        [MenuItem(VALIDATOR_HIDE_REPORT_SCENE, true)]
        public static bool CheckReportSceneHide()
        {
            Menu.SetChecked(VALIDATOR_HIDE_REPORT_SCENE, !IsReportSceneShow);
            return IsReportSceneShow;
        }

        #endregion

        #region ProgressBar

        private class ProgressBarHelper : IDisposable
        {
            private bool Enabled
            {
                get;
            }

            public ProgressBarHelper(bool enabled, string title, string text)
            {
                Enabled = enabled;

                DisplayProgression(title, text, 0f);
            }

            public void DisplayProgression(string title, string text, float progression)
            {
                if (!Enabled)
                {
                    return;
                }

                EditorUtility.DisplayProgressBar(title, text, progression);
            }

            public void Dispose()
            {
                if (!Enabled)
                {
                    return;
                }

                EditorUtility.ClearProgressBar();
            }
        }

        #endregion

        #region Check

        [MenuItem("Akatsuki/Validator/Check All Enabled Scenes And Prefabs")]
        public static void CheckAllEnabledScenesAndPrefabs()
        {
            var errorReports = new List<MessageInfo>();

            DoCheckAllEnabledScenes(errorReports);
            DoCheckAllPrefabs(errorReports);

            ShowReport(errorReports);
            ShowErrors(errorReports);
            if (errorReports.Any())
            {
                throw new Exception("[*] Null or Missing Reference check failed!");
            }
        }

        [MenuItem("Akatsuki/Validator/Check All Enabled Scenes")]
        private static void CheckAllEnabledScenes()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!CheckSceneSaved(activeScene))
            {
                return;
            }

            var errorReports = new List<MessageInfo>();
            DoCheckAllEnabledScenes(errorReports);
            ShowErrors(errorReports);
        }

        private static void DoCheckAllEnabledScenes(IList<MessageInfo> errorReports)
        {
            using (var progression = new ProgressBarHelper(IsProgressBarEnabled, "Check Scenes", "Please wait, this may take a few minutes."))
            {
                var scenes = GetAllEnabledScenes();
                var count = 0;
                var total = scenes.Count();
                foreach (var scene in scenes)
                {
                    var progress = count / (float)total;
                    var message = $"Checking scene: {++count}/{total} (Errors: {errorReports.Count()})";
                    progression.DisplayProgression(message, scene.path, progress);

                    var gameObjects = GetAllGameObjectsFromScene(scene.path);
                    CheckGameObjects(gameObjects, scene.path, errorReports);
                }
            }
        }

        [MenuItem("Akatsuki/Validator/Check All Prefabs")]
        private static void CheckAllPrefabs()
        {
            var errorReports = new List<MessageInfo>();

            DoCheckAllPrefabs(errorReports);

            ShowReport(errorReports);
            ShowErrors(errorReports);
        }

        private static void DoCheckAllPrefabs(IList<MessageInfo> errorReports)
        {
            using (var progression = new ProgressBarHelper(IsProgressBarEnabled, "Check Prefabs", "Please wait, this may take a few minutes."))
            {
                var prefabs = GetAllPrefabs();
                var count = 0;
                var total = prefabs.Count();
                var time = DateTime.Now;
                foreach (var prefab in prefabs)
                {
                    var progress = count / (float)total;
                    var message = $"Checking prefab: {++count}/{total} (Errors: {errorReports.Count()})";
                    progression.DisplayProgression(message, prefab.Path, progress);

                    var gameObjects = GetAllGameObjectsFromPrefab(prefab.GameObject);
                    CheckGameObjects(gameObjects, prefab.Path, errorReports);
                }
            }
        }

        [MenuItem("Akatsuki/Validator/Check Selected Prefabs")]
        private static void CheckSelectedPrefabs()
        {
            var errorReports = new List<MessageInfo>();

            DoCheckSelectedPrefabs(errorReports);

            ShowReport(errorReports);
            ShowErrors(errorReports);
        }

        private static void DoCheckSelectedPrefabs(IList<MessageInfo> errorReports)
        {
            using (var progression = new ProgressBarHelper(IsProgressBarEnabled, "Check Prefabs", "Please wait, this may take a few minutes."))
            {
                var prefabs = GetSelectedPrefabs();
                var count = 0;
                var total = prefabs.Count();
                var time = DateTime.Now;
                foreach (var prefab in prefabs)
                {
                    var progress = count / (float)total;
                    var message = $"Checking prefab: {++count}/{total} (Errors: {errorReports.Count()})";
                    progression.DisplayProgression(message, prefab.Path, progress);

                    var gameObjects = GetAllGameObjectsFromPrefab(prefab.GameObject);
                    CheckGameObjects(gameObjects, prefab.Path, errorReports);
                }
            }
        }

        private static void CheckGameObjects(IList<GameObjectInfo> infos, string path, IList<MessageInfo> errorReports)
        {
            foreach (var info in infos)
            {
                foreach (var component in GetAllComponents(info.GameObject))
                {
                    if (component == null)
                    {
                        continue;
                    }

                    CheckNullValueOrMissingReference(component, path, info.Path, errorReports);
                }
            }
        }

        private static bool CheckNullValueOrMissingReference(Component component, string path, string fullPath, IList<MessageInfo> errorReports)
        {
            var result = true;
            var serialObject = new SerializedObject(component);
            var type = component.GetType();
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
                var fieldValue = fieldInfo.GetValue(component);
                if (IsMissingReference(serialObject, fieldInfo, fieldValue))
                {
                    errorReports.Add(new MessageInfo()
                    {
                        Type = InvalidType.Miss,
                        MemberName = fieldInfo.Name,
                        ComponentType = component.GetType().AssemblyQualifiedName,
                        GameObjectName = component.gameObject.name,
                        GameObject = component.gameObject,
                        FullPath = fullPath.Split('/').ToList(),
                        Path = path,
                    });
                    result = false;
                }

                if (IsNotNullValidationFailed(fieldInfo, fieldValue))
                {
                    errorReports.Add(new MessageInfo()
                    {
                        Type = InvalidType.Null,
                        MemberName = fieldInfo.Name,
                        ComponentType = component.GetType().AssemblyQualifiedName,
                        GameObjectName = component.gameObject.name,
                        GameObject = component.gameObject,
                        FullPath = fullPath.Split('/').ToList(),
                        Path = path,
                    });
                    result = false;
                }
            }

            return result;
        }

        private static bool IsMissingReference(SerializedObject serialObject, FieldInfo fieldInfo, object fieldValue)
        {
            var serialProperty = serialObject.FindProperty(fieldInfo.Name);
            if (serialProperty != null && serialProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                try
                {
                    if ((fieldValue == null || fieldValue.Equals(null)) && serialProperty.objectReferenceInstanceIDValue != 0)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    Debug.Log(string.Format("targetObject {0}, fieldInfo {1}", serialObject.targetObject.name, fieldInfo.Name));
                    return true;
                }
            }

            return false;
        }

        private static bool IsNotNullValidationFailed(FieldInfo fieldInfo, object fieldValue)
        {
            var attributes = fieldInfo.GetCustomAttributes(typeof(NotNullAttribute), false);
            return attributes.Any() && fieldValue == null;
        }

        #endregion

        #region Get GameObjects and Components

        private static IEnumerable<EditorBuildSettingsScene> GetAllEnabledScenes()
        {
            return EditorBuildSettings.scenes.Where(s => s.enabled);
        }

        private static IEnumerable<PrefabInfo> GetAllPrefabs()
        {
            return AssetDatabase.FindAssets("t:prefab")
                .Select(guid =>
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    return new PrefabInfo
                    {
                        Path = path,
                        GameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path)
                    };
                })
                .Where(a => a.GameObject != null);
        }

        private static IEnumerable<PrefabInfo> GetSelectedPrefabs()
        {
            var selectedObjects = Selection.objects;
            return selectedObjects.Select(selected =>
                {
                    var path = AssetDatabase.GetAssetPath(selected);
                    return new PrefabInfo
                    {
                        Path = path,
                        GameObject = selected as GameObject
                    };
                }).Where(a => IsPrefab(a.GameObject));
        }

        private static bool IsPrefab(GameObject target)
        {
            return PrefabUtility.GetCorrespondingObjectFromSource(target) == null &&
                PrefabUtility.GetPrefabObject(target) != null;
        }

        private static IEnumerable<Component> GetAllComponents(GameObject gameObject)
        {
            return gameObject.GetComponents<Component>().AsEnumerable();
        }

        private static IList<GameObjectInfo> GetAllGameObjectsFromScene(string path)
        {
            EditorSceneManager.OpenScene(path);
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();
            var gameObjects = new List<GameObjectInfo>();
            foreach (var rootGameObject in rootGameObjects)
            {
                var info = new GameObjectInfo()
                {
                    GameObject = rootGameObject,
                    Path = $"{rootGameObject.name}"
                };
                gameObjects.Add(info);
                var subGameObjects = GetAllChildren(info);
                gameObjects.AddRange(subGameObjects);
            }

            return gameObjects;
        }

        private static IList<GameObjectInfo> GetAllGameObjectsFromPrefab(GameObject prefab)
        {
            var info = new GameObjectInfo()
            {
                GameObject = prefab,
                Path = $"{prefab.name}"
            };
            var gameObjects = new List<GameObjectInfo>()
            {
                info
            };
            var subGameObjects = GetAllChildren(info);
            gameObjects.AddRange(subGameObjects);

            return gameObjects;
        }

        private static IList<GameObjectInfo> GetAllChildren(GameObjectInfo root)
        {
            var gameObjects = new List<GameObjectInfo>();
            var count = root.GameObject.transform.childCount;
            for (int i = 0; i < count; ++i)
            {
                var child = root.GameObject.transform.GetChild(i);
                var info = new GameObjectInfo()
                {
                    GameObject = child.gameObject,
                    Path = $"{root.Path}/{child.gameObject.name}"
                };
                gameObjects.Add(info);

                var subGameObjects = GetAllChildren(info);
                gameObjects.AddRange(subGameObjects);
            }

            return gameObjects;
        }

        #endregion

        private static void ShowReport(IList<MessageInfo> errorReport)
        {
            if (!errorReport.Any())
            {
                return;
            }

            if (!IsReportSceneShow)
            {
                return;
            }

            if (!CheckAndOpenReportScene())
            {
                return;
            }

            var report = UnityEngine.Object.FindObjectOfType<MissingReferenceReporter>();
            report?.SetReport(errorReport);

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void ShowErrors(IList<MessageInfo> errorReport)
        {
            if (!errorReport.Any())
            {
                Debug.Log("LGTM, no errors!");
                return;
            }

            foreach (var messageInfo in errorReport)
            {
                var message = $"[{messageInfo.Type.ToString()}] {messageInfo.FullPath}/{messageInfo.MemberName} in {messageInfo.GameObjectName} at {messageInfo.Path}";
                Debug.LogError(message, messageInfo.GameObject);
            }
        }

        private static bool CheckSceneSaved(Scene scene)
        {
            if (scene.isDirty)
            {
                Debug.LogError("[*] Please save active scene first!");
                return false;
            }

            return true;
        }

        private static bool CheckAndOpenReportScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == SceneName)
            {
                return true;
            }

            if (CheckSceneSaved(scene))
            {
                scene = EditorSceneManager.OpenScene(ScenePath);
                return true;
            }

            return false;
        }
    }
}
