using Akatsuki.Validators;
using UnityEngine;
using UnityEngine.UI;

namespace Akatsuki
{
    public class PrefabComponent : MonoBehaviour
    {
        [SerializeField]
        private PrefabElemnet m_nullableElement;

        [NotNull, SerializeField]
        private PrefabElemnet m_nonNullableElement;

        [SerializeField]
        private PrefabElemnet m_missingElement;

        [SerializeField]
        private Image m_image;
    }
}
