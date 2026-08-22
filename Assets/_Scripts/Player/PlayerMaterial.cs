using System.Collections.Generic;
using Newtonsoft.Json;
using QuizGame.Material;
using UnityEngine;

namespace QuizGame.Player
{
    public class PlayerMaterial : IQuantifiableMaterial
    {
        private IMaterial material;
        private int quantity;

        public PlayerMaterial(IMaterial material, int quantity)
        {
            this.material = material;
            this.quantity = quantity;
        }

        public static List<PlayerMaterial> FromJson(string json)
        {
            var materialDatas = JsonConvert.DeserializeObject<List<MaterialData>>(json);
            var playerMaterials = new List<PlayerMaterial>();
            foreach (var data in materialDatas)
            {
                var material = MaterialResourceManager.Instance.GetResource(data.MaterialID);
                playerMaterials.Add(new PlayerMaterial(material, data.Quantity));
            }
            return playerMaterials;
        }

        public IMaterial GetItemInfo() => material;
        public int GetQuantity() => quantity;
        public string GetID() => material.GetID();
        public override string ToString() => $"{GetID()} x{quantity}";
        public Sprite GetSprite() => material.GetSprite();
        public void SetQuantity(int quantity)
        {
            this.quantity = quantity;
        }

        public ItemType GetItemType() => ItemType.Material;
        public ItemTier GetItemTier() => ItemTier.NoTier;
        public string GetName() => material.GetName();

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
