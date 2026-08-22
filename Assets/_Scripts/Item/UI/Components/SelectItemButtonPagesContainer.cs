using System.Collections.Generic;
using QuizGame.Interfaces;
using QuizGame.UI;

namespace QuizGame.Item.UI
{
    public class SelectItemButtonPagesContainer : BasePagesContainer<SelectItemButton, IHasSprite>
    {
        protected override void OnOpenPage(List<SelectItemButton> buttons, IHasSprite[] sprites)
        {
            base.OnOpenPage(buttons, sprites);
            for (int i = 0; i < CountSpawnedObject(); i++)
            {
                var button = buttons[i];
                if (i < sprites.Length && sprites[i] != null)
                {
                    button.SetSprite(sprites[i].GetSprite());
                }
                else
                {
                    button.SetSprite(null);
                }
            }
        }
    }
}
