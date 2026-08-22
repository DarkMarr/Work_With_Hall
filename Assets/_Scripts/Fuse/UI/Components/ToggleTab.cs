using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.Fuse.UI
{
    public class ToggleTab : MonoBehaviour
    {
        [SerializeField]
        public Toggle Toggle;

        [SerializeField]
        TextMeshProUGUI label;

        public void Init(string name, ToggleGroup group)
        {
            base.name = name;
            label.text = name;
            Toggle.group = group;
        }
    }
}