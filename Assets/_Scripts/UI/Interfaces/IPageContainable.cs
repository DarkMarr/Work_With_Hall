using UnityEngine;

namespace QuizGame.UI.Interfaces
{
    public interface IPageContainable
    {
        void OpenNextPage();
        void OpenPreviousPage();
        void OpenPage(int pageIndex);
        int PageCount();  
    }
}
