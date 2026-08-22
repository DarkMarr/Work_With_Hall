using System;
using System.Threading.Tasks;
using QuizGame.UI;

namespace QuizGame.Authentication.UI
{
    public class DefaultTransitionUI : BaseUI
    {
        private Action onTransitionEnd;
        private Func<bool> completeCondition;

        public void Init(Func<bool> completeCondition, Action onTransitionEnd)
        {
            this.onTransitionEnd = onTransitionEnd;
            this.completeCondition = completeCondition;
        }

        private async void Start()
        {
            await WaitTaskComplete();
        }

        private async Task WaitTaskComplete()
        {
            while (!completeCondition.Invoke())
            {
                await Task.Yield();
            }
            Close();
            onTransitionEnd?.Invoke();
        }
    }
}
