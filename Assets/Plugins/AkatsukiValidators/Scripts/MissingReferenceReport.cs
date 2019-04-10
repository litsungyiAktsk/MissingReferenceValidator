using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Akatsuki.Validators
{
    public class MissingReferenceReport : MonoBehaviour
    {
        Image image;
        [ReadOnly, SerializeField]
        private MessageInfo m_message;

        internal void Initialize(MessageInfo message)
        {
            m_message = message;
            m_message.GameObject = null;
        }

        internal void ResetTarget(GameObject prefab)
        {
            var target = GetTarget(prefab.transform);
            if (target != null)
            {
                m_message.GameObject = target;
            }
        }

        internal void UpdateChange(GameObject prefab)
        {
            var target = GetTarget(prefab.transform);
            if (target == null)
            {
                return;
            }

            var fieldName = m_message.MemberName;
            var type = Type.GetType(m_message.ComponentType);
            if (type == null)
            {

                Debug.LogError("Missing type information! Please retry validator again.");
                return;
            }

            var fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                return;
            }

            var sourceComponent = m_message.GameObject.GetComponent(type);
            var value = fieldInfo.GetValue(sourceComponent);
            var targetComponent = target.GetComponent(type);
            fieldInfo.SetValue(targetComponent, value);
        }

        private GameObject GetTarget(Transform root)
        {
            var rootName = root.name.Replace("(Clone)", string.Empty);
            if (m_message.FullPath.Count == 1 && rootName == m_message.FullPath.First())
            {
                return root.gameObject;
            }

            var found = false;
            foreach (var path in m_message.FullPath.Skip(1).Take(m_message.FullPath.Count - 1))
            {
                found = false;
                foreach (Transform child in root.transform)
                {
                    if (child.name == path)
                    {
                        root = child;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    break;
                }
            }

            return found ? root.gameObject : null;
        }
    }
}
