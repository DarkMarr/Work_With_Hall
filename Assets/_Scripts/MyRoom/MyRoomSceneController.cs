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

        private void Start()
        {
            UIManager.Instance.CloseAll();
            var myRoomUI = UIManager.Instance.Replace<MyRoomUI>(ref currentUI);
            myRoomUI.Init(
                onDecorateButtonClicked: () => SwitchMyRoomUIToDecorationState(myRoomUI),
                onDoneButtonClicked: () => SwitchMyRoomUIToNormalState(myRoomUI),
                onMenuButtonClicked: HandleMenuButtonClicked,
                onItemButtonClicked: HandleItemButtonClicked,
                onEquipButtonClicked: HandleEquipButtonClicked,
                onTradeButtonClicked: HandleTradeButtonClicked,
                onFriendButtonClicked: HandleFriendButtonClicked
            );
            myRoomUI.SwitchUIStage(MyRoomUI.Stage.Normal);
            InitDecorationController();
        }

        private void InitDecorationController()
        {
            var playerDecorationSlots = DecorationSlotInfo.FromJson(DecorationModel.GetDataInSlotTempDataJson()); //TODO: [Network] Load installing decoration info from server
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

        private void SwitchMyRoomUIToNormalState(MyRoomUI myRoomUI)
        {
            decorationController.SetAsDecorateMode(false);
            myRoomUI.SwitchUIStage(MyRoomUI.Stage.Normal);

            var myRoomDataJson = decorationController.GetDecorationDatasAsJson();
            Debug.Log("Save data to json: " + myRoomDataJson); //TODO: [Network] Save data to database
        }

        private void HandleMenuButtonClicked()
        {
            SceneManager.LoadScene(SceneList.MainMenu.ToString());
        }

        private void HandleItemButtonClicked()
        {
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

        private void HandleEquipButtonClicked()
        {

        }

        private void HandleTradeButtonClicked()
        {
            var tradePanelInfos = TradeSlotInfo.FromJson(TradeModel.GetTradeSlotDataTempDataJson());
            var playerMaterials = PlayerMaterial.FromJson(TradeModel.GetMaterialsDataTempDataJson());
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
