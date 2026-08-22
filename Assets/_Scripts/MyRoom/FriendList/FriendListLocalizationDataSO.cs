using UnityEngine;
using UnityEngine.Localization;

namespace QuizGame.MyRoom.FriendList
{
    [CreateAssetMenu(fileName = "NewFriendListLocalizationData", menuName = "QuizGame/LocalizationData/FriendList", order = 5)]
    public class FriendListLocalizationDataSO : ScriptableObject
    {
        public LocalizedString EnterUIDLocalized;
        public LocalizedString ShowNameLocalized;
        public LocalizedString AddLocalized;
        public LocalizedString CannotFindLocalized;
        public LocalizedString BackLocalized;
        public LocalizedString OkayLocalized;
        public LocalizedString RemoveLocalized;
        public LocalizedString RequestSentLocalized;
    }
}
