using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using QuizGame.MyRoom.UI;
using QuizGame.UI;
using QuizGame.Scene;
using QuizGame.MyRoom.Decoration;
using QuizGame.MyRoom.FriendList;
using QuizGame.MyRoom.Trade;
using QuizGame.Material;
using QuizGame.Player;
using QuizGame.MyRoom.MyItem;
using QuizGame.Item;
using QuizGame.Item.Interfaces;
using QuizGame.Item.UI;
using QuizGame.Character;
using QuizGame.Character.Outfit.UI;
using QuizGame.Network;
using QuizGame.Utilities;

namespace QuizGame.MyRoom
{
    public class MyRoomSceneController : MonoBehaviour
    {
        [SerializeField]
        private DecorationController decorationController;

        [SerializeField]
        private FriendListController friendListController;

        private TradeController tradeController = new TradeController();
        private MyItemController myItemController = new MyItemController();

        private BaseUI currentUI;

        // Held so the Equip button can close a wardrobe it already opened.
        private WardrobeUI wardrobeUI;

        private async void Start()
        {
            UIManager.Instance.CloseAll();
            var myRoomUI = UIManager.Instance.Replace<MyRoomUI>(ref currentUI);
            myRoomUI.Init(
                onDecorateButtonClicked: () => SwitchMyRoomUIToDecorationState(myRoomUI),
                onDoneButtonClicked: () => LeaveDecorationState(myRoomUI, keepChanges: true),
                onMenuButtonClicked: HandleMenuButtonClicked,
                onItemButtonClicked: HandleItemButtonClicked,
                onEquipButtonClicked: HandleEquipButtonClicked,
                onTradeButtonClicked: HandleTradeButtonClicked,
                onFriendButtonClicked: HandleFriendButtonClicked,
                onDecorationBackButtonClicked: () => LeaveDecorationState(myRoomUI, keepChanges: false),
                onRoomStyleButtonClicked: HandleRoomStyleButtonClicked
            );
            myRoomUI.SwitchUIStage(MyRoomUI.Stage.Normal);
            InitDecorationController();

            // Last, because it waits on the network: the room is usable while the name arrives.
            var profileData = await PlayerDataManager.Instance.GetProfileData();
            if (this == null || myRoomUI == null) return;
            myRoomUI.SetProfile(profileData?.ProfileName, "Unranked"); // TODO: [Network] real rank.
        }

        private void InitDecorationController()
        {
            var playerDecorationSlots = DecorationSlotInfo.FromJson(DecorationModel.GetDataInSlotTempDataJson()); //TODO: [Network] Load installing decoration info from server
            // TestInventory: every decoration in the game is offered, not only the ones owned.
            var playerAvailableDecoration = DecorationItemResourceManager.Instance.GetDecorationAllTypes() //TODO: [Network] Load all player decoration in inventory
                .ToDictionary(pair => pair.Key, pair => pair.Value.Cast<IDecorationItem>()
                .ToArray());
            var decorationModel = new DecorationModel(playerAvailableDecoration, playerDecorationSlots);
            decorationController.Init(decorationModel);
        }

        private void SwitchMyRoomUIToDecorationState(MyRoomUI myRoomUI)
        {
            decorationController.SetAsDecorateMode(true);
            myRoomUI.SwitchUIStage(MyRoomUI.Stage.Decoration);
        }

        /// <summary>
        /// Closes decoration mode. The tick keeps what was arranged; the back arrow walks away from
        /// it. Both leave the mode, which is why they share this.
        /// </summary>
        private void LeaveDecorationState(MyRoomUI myRoomUI, bool keepChanges)
        {
            decorationController.SetAsDecorateMode(false);
            myRoomUI.SwitchUIStage(MyRoomUI.Stage.Normal);

            if (!keepChanges)
            {
                // TODO: [Network] Re-read the saved arrangement so the back arrow really undoes the
                // session's changes. Until there is somewhere to read it back from, it only closes.
                Debug.Log("[MyRoom] Left decoration mode without saving.");
                return;
            }

            var myRoomDataJson = decorationController.GetDecorationDatasAsJson();
            Debug.Log("Save data to json: " + myRoomDataJson); //TODO: [Network] Save data to database
        }

        /// <summary>
        /// Opens the wallpapers. The room itself is a decoration like any other, it just has no slot
        /// standing in the room to tap, so it gets its own button in the decoration bar.
        /// </summary>
        private void HandleRoomStyleButtonClicked()
        {
            var wallpapers = DecorationItemResourceManager.Instance.GetDecorationByType(DecorationType.Room);
            if (wallpapers == null || wallpapers.Length == 0)
            {
                Debug.LogWarning("[MyRoom] No room wallpapers to choose from.");
                return;
            }

            var backdrop = FindFirstObjectByType<RoomBackdrop>();
            if (backdrop == null)
            {
                Debug.LogWarning("[MyRoom] No RoomBackdrop in the scene; nothing to hang a wallpaper on.");
                return;
            }

            var selectionUI = UIManager.Instance.Create<EquipItemSelectionUI>();
            selectionUI.Init(
                defaultSelectingItemIndex: -1,
                selectionTitle: "Room",
                itemSprites: wallpapers,
                onSelectButtonClicked: () =>
                {
                    var selected = wallpapers[selectionUI.SelectingItemIndex];
                    backdrop.SetWallpaper(selected.GetRoomSprite());
                    //TODO: [Network] Save the chosen wallpaper with the rest of the room.
                }
            );
            selectionUI.VisualizeEquipText();
        }

        private void HandleMenuButtonClicked()
        {
            SceneManager.LoadScene(SceneList.MainMenu.ToString());
        }

        private void HandleItemButtonClicked()
        {
            // TestInventory: the bag shows the whole catalogue in each tab, not what the player owns.
            // TODO: [Network] Load all player trophy items/decoration from server
            var tropyItems = DecorationItemResourceManager.Instance.GetAllResources().Cast<IItem>().ToList();
            // TODO: [Network] Load all player equipment items from server
            var equipmentItems = DecorationItemResourceManager.Instance.GetAllResources().Cast<IItem>().ToList();
            // TODO: [Network] Load all player consumable items from server
            var consumableItems = CarryOnItemResourceManager.Instance.GetAllResources().Cast<IItem>().ToList();

            var myItemModel = new MyItemModel();
            myItemModel.Init(tropyItems, equipmentItems, consumableItems);
            myItemController.Init(myItemModel);
        }

        /// <summary>
        /// Opens the dressing screen. It covers only the lower half, so the avatar standing in the
        /// room stays visible and the player sees each garment on their own character as they pick
        /// it, rather than on a preview that could drift from the real thing.
        /// </summary>
        private void HandleEquipButtonClicked()
        {
            if (PlayerCharacterManager.Instance == null)
            {
                Debug.LogWarning("[MyRoom] PlayerCharacterManager not available; cannot open the wardrobe.");
                return;
            }

            if (wardrobeUI != null)
            {
                wardrobeUI.Close();
                wardrobeUI = null;
                return;
            }

            wardrobeUI = UIManager.Instance.Create<WardrobeUI>();
            wardrobeUI.Init(() =>
            {
                if (wardrobeUI == null) return;
                wardrobeUI.Close();
                wardrobeUI = null;
            });
        }

        private void HandleTradeButtonClicked()
        {
            var tradePanelInfos = TradeSlotInfo.FromJson(TradeModel.GetTradeSlotDataTempDataJson());
            // TestInventory: a full purse, so every trade on the board can actually be attempted.
            var playerMaterials = TestInventory.UnlockEverything
                ? PlayerMaterial.AllMaterialsForTesting()
                : PlayerMaterial.FromJson(TradeModel.GetMaterialsDataTempDataJson());
            var tradeModel = new TradeModel(tradePanelInfos, playerMaterials.Cast<IQuantifiableMaterial>().ToList());
            tradeController.Init(tradeModel);
        }

        private void HandleFriendButtonClicked()
        {
            var friendList = FriendData.FromJson(FriendListModel.GetFriendTempDataJson());
            var friendRequestedList = FriendData.FromJson(FriendListModel.GetFriendRequestedTempDataJson());
            var friendListModel = new FriendListModel(friendList, friendRequestedList);
            friendListController.Init(friendListModel);
        }
    }
}
