using Newtonsoft.Json;
using QuizGame.Item.Interfaces;
using QuizGame.Material;
using QuizGame.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuizGame.Fuse
{
    public class FusingPlayerModel
    {
        private Dictionary<string, IQuantifiableMaterial> playerMaterialByID = new Dictionary<string, IQuantifiableMaterial>();

        public FusingPlayerModel(List<PlayerMaterial> playerMaterials)
        {
            foreach (var material in playerMaterials)
            {
                playerMaterialByID.Add(material.GetID(), material);
            }
        }

        public IQuantifiableMaterial GetMaterialByID(string materialID) =>
            playerMaterialByID.GetValueOrDefault(materialID);

        public List<IQuantifiableMaterial> GetMaterials() => playerMaterialByID.Values.ToList();

        public List<MaterialData> GetAllMaterials() =>
            playerMaterialByID.Select(m => new MaterialData(m.Key, m.Value.GetQuantity())).ToList();

        public string GetAllMaterialsAsJson() =>
            JsonConvert.SerializeObject(GetAllMaterials(), Formatting.Indented);

        public bool IsFuseAble(IFuseable fusingItem, out List<IQuantifiableItem> missingPlayerMaterials)
        {
            var fuseRequirements = fusingItem.GetFuseRequirementItems().ToList();
            var playerRequirementMaterials = GetFuseRequirementInPlayer(fuseRequirements);
            var missingRequirementMaterials = new List<IQuantifiableItem>();

            var fuseable = true;
            missingPlayerMaterials = missingRequirementMaterials;

            // Check if player has all required materials and quantities
            for (int i = 0; i < playerRequirementMaterials.Count; i++)
            {
                if (playerRequirementMaterials[i] == null)
                {
                    Debug.LogWarning($"[FuseController] Player does not have required material: {fuseRequirements[i].ToString()}");
                    missingRequirementMaterials.Add(fuseRequirements[i]);
                    fuseable = false;
                    break;
                }
                else if (playerRequirementMaterials[i].GetQuantity() < fuseRequirements[i].GetQuantity())
                {
                    Debug.LogWarning($"[FuseController] Not enough quantity for material: {fuseRequirements[i].ToString()}");
                    missingRequirementMaterials.Add(fuseRequirements[i]);
                    fuseable = false;
                    break;
                }
            }

            return fuseable;
        }

        private List<IQuantifiableMaterial> GetFuseRequirementInPlayer(List<IQuantifiableItem> fuseRequirements)
        {
            var playerRequirementMaterials = new List<IQuantifiableMaterial>();

            foreach (var requirement in fuseRequirements)
            {
                var material = playerMaterialByID.GetValueOrDefault(requirement.GetID());
                if (material == null || material.GetQuantity() < requirement.GetQuantity())
                {
                    playerRequirementMaterials.Add(material);
                }
            }

            return playerRequirementMaterials;
        }

        public void AddMaterial(IQuantifiableItem quantifiableItem)
        {
            AddMaterial(quantifiableItem.GetID(), quantifiableItem.GetQuantity());
        }

        public void AddMaterial(string materialID, int quantity)
        {
            if (playerMaterialByID.TryGetValue(materialID, out var material))
            {
                material.SetQuantity(material.GetQuantity() + quantity);
            }
            else
            {
                Debug.LogError($"Material with ID {materialID} not found.");
            }
        }

        public void RemoveMaterial(IQuantifiableItem quantifiableItem)
        {
            RemoveMaterial(quantifiableItem.GetID(), quantifiableItem.GetQuantity());
        }

        public void RemoveMaterial(string materialID, int quantity)
        {
            if (playerMaterialByID.TryGetValue(materialID, out var material))
            {
                material.SetQuantity(material.GetQuantity() - quantity);
            }
            else
            {
                Debug.LogError($"Material with ID {materialID} not found.");
            }
        }
    }
}