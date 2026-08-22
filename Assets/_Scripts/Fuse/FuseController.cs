using QuizGame.Fuse.UI;
using QuizGame.Item.Interfaces;
using QuizGame.MyRoom.Decoration;
using QuizGame.Scene;
using QuizGame.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuizGame.Fuse
{
    public class FuseController
    {
        private BaseUI currentUI;
        private FuseProfileUI profileUI;

        private List<FuseTabModel> currentModelList;

        private FusingPlayerModel playerModel;

        public void Setup(List<FuseTabModel> fuseTabModelList, FusingPlayerModel playerModel)
        {
            UIManager.Instance.CloseAll();
            profileUI = UIManager.Instance.Create<FuseProfileUI>();
            UpdatePlayerMaterials(playerModel);

            var fuseUI = UIManager.Instance.Replace<FuseUI>(ref currentUI);
            currentModelList = fuseTabModelList;
            fuseUI.Init(fuseTabModelList);
            fuseUI.OnBackButtonClicked += () => HandleMenuButtonClicked();
            fuseUI.OnTabButtonClicked += tabModel => HandleTabClicked(tabModel);
        }

        private void UpdatePlayerMaterials(FusingPlayerModel playerModel)
        {
            this.playerModel = playerModel;
            profileUI.Setup(playerModel);
            Debug.Log("[FuseController] Player materials updated.");
            Debug.Log($"[FuseController] Current materials: {string.Join(", ", playerModel.GetMaterials().Select(m => m.ToString()))}");
        }

        private void HandleMenuButtonClicked()
        {
            SceneManager.LoadScene(SceneList.MainMenu.ToString());
        }

        private void HandleTabClicked(FuseTabModel tabModel)
        {
            Debug.Log($"[FuseController] Tab clicked");
            var fusingTabUI = UIManager.Instance.Replace<FusingTabUI>(ref currentUI);
            fusingTabUI.OnCloseButtonClicked += () => HandleBackButtonClicked();

            var fuseDetailUI = UIManager.Instance.Create<FusingDetailUI>(parentUI: fusingTabUI);
            fuseDetailUI.OnPreviewButtonClicked += decorationItem => HandlePreviewButtonClicked(decorationItem);
            fuseDetailUI.OnFuseButtonClicked += fusingItem => HandleFuseButtonClicked(fusingItem, playerModel);
            fusingTabUI.OnSelectedFuseItem += decorationItem => HandleOnSelectedFuseItem(ref fuseDetailUI, decorationItem);

            fusingTabUI.Init(tabModel);
        }

        private void HandleBackButtonClicked()
        {
            Debug.Log("[FuseController] Back button clicked.");
            Setup(currentModelList, playerModel);
        }

        private void HandleOnSelectedFuseItem(ref FusingDetailUI fuseDetailUI, IDecorationItem decorationItem)
        {
            fuseDetailUI.Setup(decorationItem);
        }

        private void HandlePreviewButtonClicked(IDecorationItem decorationItem)
        {
            Debug.Log("[FuseController] Preview button clicked.");
            var previewUI = UIManager.Instance.Create<FusingPreviewUI>();
            previewUI.OnCloseButtonClicked += () => previewUI.Close();
            previewUI.Init(decorationItem);
        }

        private void HandleFuseButtonClicked(IDecorationItem fusingItem, FusingPlayerModel playerModel)
        {
            var fuseResult = fusingItem.GetFuseResult();
            var fuseRequirements = fusingItem.GetFuseRequirementItems().ToList();

            Debug.Log("[FuseController] Fuse button clicked.");
            Debug.Log($"[FuseController] Try to fusing item: {fuseResult.ToString()} Required: {string.Join(", ", fuseRequirements.Select(m => m.ToString()))}");

            var fuseable = playerModel.IsFuseAble(fusingItem: fusingItem, out List<IQuantifiableItem> missingMaterials);

            // If not fuse able, exit
            if (!fuseable)
            {
                var popup = UIManager.Instance.Create<MessagePopupUI>();
                popup.Setup("Failed to fuse", "You do not have enough materials.", "OK", () => popup.Close());
                Debug.LogWarning($"[FuseController] Missing materials: {string.Join(", ", missingMaterials.Select(m => m.ToString()))}");
                return;
            }

            // Deduct required materials from player's inventory
            fuseRequirements.ForEach(playerModel.RemoveMaterial);

            UpdatePlayerMaterials(playerModel);
            Debug.Log($"[FuseController] Item fused: {fuseResult.ToString()}.");
        }
    }
}