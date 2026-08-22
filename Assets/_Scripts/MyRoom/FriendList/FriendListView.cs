using QuizGame.UI;

namespace QuizGame.MyRoom.FriendList.UI
{
    public class FriendListView : VerticalObjectPoolingScrollView<FriendListElement>
    {
        public void SetElementsToHomeMode()
        {
            foreach (var element in elementPool)
            {
                element.ChangeState(FriendListElement.State.Home);
            }
        }

        public void SetElementsToDeleteMode()
        {
            foreach (var element in elementPool)
            {
                element.ChangeState(FriendListElement.State.Delete);
            }
        }
    }
}
