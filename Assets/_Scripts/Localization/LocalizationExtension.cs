using TMPro;
using UnityEngine;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedObjects;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;

namespace QuizGame.Localization
{
    public static class LocalizationExtension
    {
        /// <summary>
        /// Set arguments to GameObjectLocalizer/TrackedUGuiGraphic/LocalizedStringProperty.
        /// If you want to update text every frame, please cache LocalizedStringProperty and called SetLocalizedArguments from it. 
        /// **Game Object Localizer on target text is needed**
        /// </summary>
        /// <param name="text"></param>
        /// <param name="arguments"></param>
        public static void SetLocalizedArguments(this TextMeshProUGUI text, params object[] arguments)
        {
            var trackedText = text.GetTrackedLocalizedText();
            trackedText.SetLocalizedArguments(arguments);
        }

        /// <summary>
        /// Set arguments to GameObjectLocalizer/TrackedUGuiGraphic.
        /// If you want to update text every frame, please cache LocalizedStringProperty and called SetLocalizedArguments from it. 
        /// **Game Object Localizer on target text is needed**
        /// </summary>
        /// <param name="text"></param>
        /// <param name="arguments"></param>
        public static void SetLocalizedArguments(this LocalizedStringProperty trackedText, params object[] arguments)
        {
            if (trackedText == null || trackedText.LocalizedString == null)
            {
                Debug.LogError("[Localization] TrackedText is null, please check if GameObjectLocalizer and TrackedUGuiGraphic are set up correctly.");
                return;
            }
            var isRefreshNeeded = trackedText.LocalizedString.Arguments != null;
            trackedText.LocalizedString.Arguments = arguments;
            if (isRefreshNeeded) 
            {
                //If it isn't the first time setting arguments will not refresh the text, so we need to call RefreshString manually.
                trackedText.LocalizedString.RefreshString();
            }
        }

        public static LocalizedStringProperty GetTrackedLocalizedText(this TextMeshProUGUI text)
        {
            var localizer = text.GetComponent<GameObjectLocalizer>();
            if (localizer == null)
            {
                Debug.LogError($"[Localization] {text} doesn't contain GameObjectLocalizer.");
                return null;
            }
            var trackedUGUI = localizer.GetTrackedObject<TrackedUGuiGraphic>(text);
            return trackedUGUI.GetTrackedProperty<LocalizedStringProperty>("m_text");
        }
    }
}
