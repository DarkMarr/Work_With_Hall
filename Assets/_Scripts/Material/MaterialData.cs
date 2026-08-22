using Newtonsoft.Json;

namespace QuizGame.Material
{
    public class MaterialData
    {
        [JsonProperty("material_id")]
        public string MaterialID;

        [JsonProperty("quantity")]
        public int Quantity;

        public MaterialData(string materialID, int quantity)
        {
            MaterialID = materialID;
            Quantity = quantity;
        }
    }
}