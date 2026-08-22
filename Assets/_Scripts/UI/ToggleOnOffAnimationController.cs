using UnityEngine;
using UnityEngine.UI;

namespace QuizGame.UI
{
    [RequireComponent(typeof(Toggle), typeof(Animator))]
    public class ToggleOnOffAnimationController : MonoBehaviour
    {
        [SerializeField]
        private Animator targetAnimator;

        [SerializeField]
        private Toggle targetToggle;

        [SerializeField]
        private string onParameterName = "On";

        private void OValidate()
        {
            targetAnimator ??= GetComponent<Animator>();
            targetToggle ??= GetComponent<Toggle>();
        }

        private void Start()
        {
            targetAnimator.SetBool(onParameterName, targetToggle.isOn);
            targetToggle.onValueChanged.AddListener((isOn) =>
            {
                if (targetAnimator != null)
                {
                    targetAnimator.SetBool(onParameterName, isOn);
                }
            });
        }
    }
}
