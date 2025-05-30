﻿using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class CreateSquadCardPostModel
    {
        [JsonPropertyName("cardId")]
        public int CardId { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("children")]
        public int[] Children { get; set; }

        public CreateSquadCardPostModel()
        {
            Children = [];
        }
    }
}
