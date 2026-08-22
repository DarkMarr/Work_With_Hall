using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using QuizGame.Item;
using QuizGame.Material;
using UnityEngine;

namespace QuizGame.MyRoom.Trade
{
    public class TradeModel
    {
        private List<TradeSlotInfo> tradeSlotInfos;
        private Dictionary<string, IQuantifiableMaterial> materialByID = new Dictionary<string, IQuantifiableMaterial>();

        public TradeModel(List<TradeSlotInfo> tradeSlotInfos, List<IQuantifiableMaterial> materials)
        {
            this.tradeSlotInfos = tradeSlotInfos;
            foreach (var material in materials)
            {
                materialByID.Add(material.GetID(), material);
            }
        }

        public List<TradeSlotInfo> GetTradeSlotInfos() => tradeSlotInfos.ToList();
        public List<IQuantifiableMaterial> GetMaterials() => materialByID.Values.ToList();
        public int CountTradeSlotDatas() => tradeSlotInfos.Count;

        public List<TradeSlotData> GetAllSlotDatas() =>
            tradeSlotInfos.Select(p => new TradeSlotData(p.OfferMaterial?.GetID(), p.RequestMaterial?.GetID(), p.FulfilledPlayerName)).ToList();

        public string GetAllSlotDatasAsJson() =>
            JsonConvert.SerializeObject(GetAllSlotDatas(), Formatting.Indented);

        public List<MaterialData> GetAllMaterials() =>
            materialByID.Select(m => new MaterialData(m.Key, m.Value.GetQuantity())).ToList();

        public string GetAllMaterialsAsJson() =>
            JsonConvert.SerializeObject(GetAllMaterials(), Formatting.Indented);

        public void RemoveTradeSlotInfo(TradeSlotInfo info)
        {
            tradeSlotInfos.Remove(info);
        }

        public bool IsMaterialEnough(string materialID, int quantity)
        {
            if (materialByID.TryGetValue(materialID, out var material))
            {
                return material.GetQuantity() - quantity > 0;
            }
            else
            {
                Debug.LogError($"Material with ID {materialID} not found.");
            }
            return false;
        }

        public void AddMaterial(string materialID, int quantity)
        {
            if (materialByID.TryGetValue(materialID, out var material))
            {
                material.SetQuantity(material.GetQuantity() + quantity);
            }
            else
            {
                Debug.LogError($"Material with ID {materialID} not found.");
            }
        }

        public void RemoveMaterial(string materialID, int quantity)
        {
            if (materialByID.TryGetValue(materialID, out var material))
            {
                material.SetQuantity(material.GetQuantity() - quantity);
            }
            else
            {
                Debug.LogError($"Material with ID {materialID} not found.");
            }
        }

        public static string GetTradeSlotDataTempDataJson() => @"[
            {
                ""offer_material_id"": """",
                ""request_material_id"": """",
                ""fulfilled_player_name"": """"
            },
            {
                ""offer_material_id"": ""50004"",
                ""request_material_id"": ""50001"",
                ""fulfilled_player_name"": ""ArmkaiserCute""
            }
        ]";

        public static string GetMaterialsDataTempDataJson() => @"[
            {
                ""material_id"": ""50004"",
                ""quantity"": 10
            },
            {
                ""material_id"": ""50001"",
                ""quantity"": 5
            },
            {
                ""material_id"": ""50005"",
                ""quantity"": 8
            },
            {
                ""material_id"": ""50002"",
                ""quantity"": 0
            },
            {
                ""material_id"": ""50003"",
                ""quantity"": 20
            }
        ]";
    }
}
