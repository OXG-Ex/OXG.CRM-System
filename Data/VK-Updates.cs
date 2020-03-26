using System;
using Newtonsoft.Json;

namespace OXG.CRM_System.Data
{
    [Serializable]
    public class VK_Updates
    {
        /// <summary>
        /// Тип события
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Объект, инициировавший событие
        /// Структура объекта зависит от типа уведомления
        /// </summary>
        [JsonProperty("object")]
        public Newtonsoft.Json.Linq.JObject Object { get; set; }

        /// <summary>
        /// ID сообщества, в котором произошло событие
        /// </summary>
        [JsonProperty("group_id")]
        public long GroupId { get; set; }
    }
}
