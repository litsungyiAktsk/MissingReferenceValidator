using System.Collections.Generic;
using UnityEngine;

namespace Akatsuki.Validators
{
    [ExecuteInEditMode]
    public class MissingReferenceSelecter : MonoBehaviour
    {
#pragma warning disable 0414
        private string m_path;
#pragma warning restore 0414

        [SerializeField]
        private GameObject m_prefab;

        [SerializeField]
        private GameObject m_instance;

        [SerializeField]
        private List<MissingReferenceReport> m_reports = new List<MissingReferenceReport>();

        public void Initialize(MessageInfo message)
        {
            m_path = message.Path;
#if UNITY_EDITOR
            m_prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(m_path);
#endif
        }

        public void AddReport(MissingReferenceReport report)
        {
            m_reports.Add(report);
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (m_prefab == null || m_instance != null)
            {
                return;
            }

            m_instance = UnityEditor.PrefabUtility.InstantiateAttachedAsset(m_prefab) as GameObject;
            m_instance.transform.SetParent(transform);

            foreach (var report in m_reports)
            {
                report.ResetTarget(m_instance);
            }
        }

        private void OnDisable()
        {
            if (m_prefab == null || m_instance == null)
            {
                return;
            }

            var target = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(m_instance) as GameObject;
            foreach (var report in m_reports)
            {
                report.UpdateChange(target);
            }

            UnityEditor.EditorUtility.SetDirty(target);

            DestroyImmediate(m_instance);
            m_instance = null;
        }
#endif
    }
}
