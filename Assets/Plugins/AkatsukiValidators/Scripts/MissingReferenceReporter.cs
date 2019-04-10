using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Akatsuki.Validators
{
    public enum InvalidType
    {
        Null,
        Miss,
    }

    [Serializable]
    public class MessageInfo
    {
        public InvalidType Type;
        public string MemberName;
        public string GameObjectName;
        public string ComponentType;
        public GameObject GameObject;
        public List<string> FullPath;
        public string Path;
    }

    public class MissingReferenceReporter : MonoBehaviour
    {
        [NotNull, SerializeField]
        private MissingReferenceSelecter m_prefab;

        [NotNull, SerializeField]
        private Transform m_root;

        private Dictionary<string, MissingReferenceSelecter> m_reports = new Dictionary<string, MissingReferenceSelecter>();

        public void SetReport(IList<MessageInfo> messages)
        {
            Clear();

            foreach (var message in messages)
            {
                MissingReferenceSelecter selecter = null;
                MissingReferenceReport report = null;
                var gameObjectName = message.FullPath.First();
                var key = message.Path;
                if (!m_reports.TryGetValue(key, out selecter))
                {
                    selecter = Instantiate(m_prefab, m_root);
                    selecter.name = gameObjectName;
                    selecter.gameObject.SetActive(false);
                    selecter.Initialize(message);
                    m_reports.Add(key, selecter);
                }

                report = selecter.gameObject.AddComponent<MissingReferenceReport>();
                selecter.AddReport(report);
                report.Initialize(message);
            }
        }

        private void Clear()
        {
            foreach (var report in m_reports)
            {
                Destroy(report.Value);
            }

            m_reports.Clear();
        }
    }
}
