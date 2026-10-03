using QuizGame.Craft.UI;
using QuizGame.Item.Interfaces;
using QuizGame.MyRoom.Decoration;
using QuizGame.Scene;
using QuizGame.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuizGame.Craft
{
    public class CraftController
    {
        private BaseUI currentUI;
        private CraftProfileUI profileUI;

        private List<CraftTabModel> currentModelList;

        private CraftingPlayerModel playerModel;

        public void Setup(List<CraftTabModel> craftTabModelList, CraftingPlayerModel playerModel)
        {
            UIManager.Instance.CloseAll();
            profileUI = UIManager.Instance.Create<CraftProfileUI>();
            UpdatePlayerMaterials(playerModel);

            var craftUI = UIManager.Instance.Replace<CraftUI>(ref currentUI);
            currentModelList = craftTabModelList;
            craftUI.Init(craftTabModelList);
            craftUI.OnBackButtonClicked += () => HandleMenuButtonClicked();
            craftUI.OnTabButtonClicked += tabModel => HandleTabClicked(tabModel);
        }

        private void UpdatePlayerMaterials(CraftingPlayerModel playerModel)
        {
            this.playerModel = playerModel;
            profileUI.Setup(playerModel);
            Debug.Log("[CraftController] Player materials updated.");
            Debug.Log($"[CraftController] Current materials: {string.Join(", ", playerModel.GetMaterials().Select(m => m.ToString()))}");
        }

        private void HandleMenuButtonClicked()
        {
            SceneManager.LoadScene(SceneList.MainMenu.ToString());
        }

        private void HandleTabClicked(CraftTabModel tabModel)
        {
            Debug.Log($"[CraftController] Tab clicked");
            var craftingTabUI = UIManager.Instance.Replace<CraftingTabUI>(ref currentUI);
            craftingTabUI.OnCloseButtonClicked += () => HandleBackButtonClicked();

            var craftDetailUI = UIManager.Instance.Create<CraftingDetailUI>(parentUI: craftingTabUI);
            craftDetailUI.OnPreviewButtonClicked += decorationItem => HandlePreviewButtonClicked(decorationItem);
            craftDetailUI.OnCraftButtonClicked += craftingItem => HandleCraftButtonClicked(craftingItem, playerModel);
            craftingTabUI.OnSelectedCraftItem += decorationItem => HandleOnSelectedCraftItem(ref craftDetailUI, decorationItem);

            craftingTabUI.Init(tabModel);
        }

        private void HandleBackButtonClicked()
        {
            Debug.Log("[CraftController] Back button clicked.");
            Setup(currentModelList, playerModel);
        }

        private void HandleOnSelectedCraftItem(ref CraftingDetailUI craftDetailUI, IDecorationItem decorationItem)
        {
            craftDetailUI.Setup(decorationItem);
        }

        private void HandlePreviewButtonClicked(IDecorationItem decorationItem)
        {
            Debug.Log("[CraftController] Preview button clicked.");
            var previewUI = UIManager.Instance.Create<CraftingPreviewUI>();
            previewUI.OnCloseButtonClicked += () => previewUI.Close();
            previewUI.Init(decorationItem);
        }

        private void HandleCraftButtonClicked(IDecorationItem craftingItem, CraftingPlayerModel playerModel)
        {
            var craftResult = craftingItem.GetCraftResult();
            var craftRequirements = craftingItem.GetCraftRequirementItems().ToList();

            Debug.Log("[CraftController] Craft button clicked.");
            Debug.Log($"[CraftController] Try to crafting item: {craftResult.ToString()} Required: {string.Join(", ", craftRequirements.Select(m => m.ToString()))}");

            var craftable = playerModel.IsCraftAble(craftingItem: craftingItem, out List<IQuantifiableItem> missingMaterials);

            // If not craft able, exit
            if (!craftable)
            {
                var popup = UIManager.Instance.Create<MessagePopupUI>();
                popup.Setup("Failed to craft", "You do not have enough materials.", "OK", () => popup.Close(), tone: PopupTone.Alert);
                Debug.LogWarning($"[CraftController] Missing materials: {string.Join(", ", missingMaterials.Select(m => m.ToString()))}");
                return;
            }

            // Deduct required materials from player's inventory
            craftRequirements.ForEach(playerModel.RemoveMaterial);

            UpdatePlayerMaterials(playerModel);
            Debug.Log($"[CraftController] Item crafted: {craftResult.ToString()}.");
        }
    }
}