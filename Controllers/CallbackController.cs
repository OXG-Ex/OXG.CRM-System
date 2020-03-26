using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OXG.CRM_System.Data;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace OXG.CRM_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration _configuration;

        private readonly IVkApi _vkApi;

        public CallbackController(IVkApi vkApi, IConfiguration configuration)
        {
            _vkApi = vkApi;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Callback()
        {
            return Content("Проверка страницы");
        }

        [HttpPost]
        public IActionResult Callback([FromBody] VK_Updates updates)
        {
            // Проверяем, что находится в поле "type" 
            switch (updates.Type)
            {
                // Если это уведомление для подтверждения адреса
                case "confirmation":
                    // Отправляем строку для подтверждения 
                    return Ok(_configuration["Config:Confirmation"]);
                case "message_new":
                    {
                        // Десериализация
                        var msg = Message.FromJson(new VkResponse(updates.Object));

                        var keyboard = new KeyboardBuilder()
                                            .AddButton("Подтвердить", "btnValue", KeyboardButtonColor.Positive)
                                            .SetInline(true)
                                            //.SetOneTime()
                                            .AddLine()
                                            .AddButton("Отменить", "btnValue", KeyboardButtonColor.Primary)
                                            .Build();

                        // Отправим в ответ полученный от пользователя текст
                        _vkApi.Messages.Send(new MessagesSendParams
                        {
                            RandomId = new DateTime().Millisecond,
                            PeerId = msg.PeerId.Value,
                            Message = msg.Text,
                            Keyboard = keyboard

                        });
                        break;
                    }
            }
            //  Возвращаем "ok" серверу Callback API
            return Ok("ok");
        }
    }
}