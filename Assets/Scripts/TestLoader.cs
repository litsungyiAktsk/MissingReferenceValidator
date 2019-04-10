using UnityEngine;

namespace Akatsuki
{
    public class TestLoader : MonoBehaviour
    {
        [SerializeField]
        private PrefabComponent m_goodPrefab;

        [SerializeField]
        private PrefabComponent m_missingPrefab;

        private void Awake()
        {
            Instantiate(m_goodPrefab, transform);
            Instantiate(m_missingPrefab, transform);
        }
    }
}
