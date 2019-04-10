using Akatsuki.Validators;
using UnityEngine;
using UnityEngine.UI;

namespace Akatsuki
{
    public class PrefabComponent : MonoBehaviour
    {
        [SerializeField]
        private Image m_nullableElement;

        [NotNull, SerializeField]
        private Image m_nonNullableElement;

        [SerializeField]
        private Image m_missingSpriteElement;
    }
}
