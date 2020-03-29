using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;
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

        private readonly CRMDbContext db;

        public CallbackController(IVkApi vkApi, IConfiguration configuration, CRMDbContext context)
        {
            _vkApi = vkApi;
            _configuration = configuration;
            db = context;
        }

        [HttpGet]
        public IActionResult Callback()
        {
            return Content("Проверка страницы");
        }

        [HttpPost]
        public async Task<IActionResult> Callback([FromBody] VK_Updates updates)
        {
            switch (updates.Type)
            {
                case "confirmation":
                    return Ok(_configuration["Config:Confirmation"]);
                case "message_new":
                    {
                        // Десериализация
                        var msg = Message.FromJson(new VkResponse(updates.Object));
                        var responseText = "";
                        var payload = "";

                        var keyboard = new KeyboardBuilder()
                                            .AddButton("Оформить заказ", "createEvent", KeyboardButtonColor.Positive)
                                            .AddButton("Получить прайс", "getPrice", KeyboardButtonColor.Default)
                                            .SetInline(false)
                                            .SetOneTime()
                                            .AddLine()
                                            .AddButton("Связаться с менеджером", "getManager", KeyboardButtonColor.Positive)
                                            .AddButton("Заказать звонок", "getCall", KeyboardButtonColor.Default)
                                            .AddLine()
                                            .AddButton("Оставить отзыв или жалобу", "createResponse", KeyboardButtonColor.Primary)
                                            .Build();
                        ///TODO: Определить ветви, этапы и общую схему взаимодействия. При отсутствии клиента добавлять его в БД
                        var user = _vkApi.Users.Get(new long[] { (long)msg.FromId }, VkNet.Enums.Filters.ProfileFields.Contacts).FirstOrDefault();

                        var userVk = await db.ClientsVK.Where(u => u.VkId == (long)msg.FromId).FirstOrDefaultAsync();
                        if (userVk == null)
                        {
                            userVk = new ClientVK(user);
                            userVk.Branch = "Our client?";
                            userVk.Stage = "Yes/No";
                            userVk.Client = new Client() { Name = "TempClient" };
                            await db.ClientsVK.AddAsync(userVk);
                            await db.SaveChangesAsync();
                            keyboard = new KeyboardBuilder().AddButton("Да", "yes", KeyboardButtonColor.Positive)
                                            .AddButton("Нет", "no", KeyboardButtonColor.Default)
                                            .SetInline(false)
                                            .SetOneTime()
                                            .Build();
                            responseText = "Добрый день, к сожалению я не нашёл записи о вас в нашей базе данных. Вы являетесь нашим клиентом?";
                        }
                        else
                        {//TODO: Добавить переход вверх по ветке. Добавить дефаулты и обработку неправильных ответов.
                            switch (userVk.Branch)
                            {
                                case "Our client?":
                                    switch (userVk.Stage)
                                    {
                                        case "Yes/No":
                                            if (msg.Text == "Да")
                                            {
                                                userVk.Stage = "Have active Event?";
                                                responseText = "У вас есть текущее мероприятие(оформленный заказ)?";
                                                keyboard = new KeyboardBuilder().AddButton("Да", "yes", KeyboardButtonColor.Positive)
                                                    .AddButton("Нет", "no", KeyboardButtonColor.Default)
                                                    .SetInline(false)
                                                    .SetOneTime()
                                                    .Build();
                                                break;
                                            }
                                            if (msg.Text == "Нет")
                                            {
                                                userVk.Branch = "Main";
                                                userVk.Stage = "0";
                                                responseText = "Что вас интерисует?";
                                                keyboard = new KeyboardBuilder()
                                                    .AddButton("Оформить заказ", "createEvent", KeyboardButtonColor.Positive)
                                                    .AddButton("Получить прайс", "getPrice", KeyboardButtonColor.Default)
                                                    .SetInline(false)
                                                    .SetOneTime()
                                                    .AddLine()
                                                    .AddButton("Заказать звонок", "getCall", KeyboardButtonColor.Default)
                                                    .AddLine()
                                                    .Build();
                                                break;
                                            }
                                            responseText = "Я вас не понимаю, пожалуйста используйте для ответов кнопки. Вы являетесь нашим клиентом ?";
                                            keyboard = new KeyboardBuilder().AddButton("Да", "yes", KeyboardButtonColor.Positive)
                                            .AddButton("Нет", "no", KeyboardButtonColor.Default)
                                            .SetInline(false)
                                            .SetOneTime()
                                            .Build();
                                            break;
                                        case "Have active Event?":
                                            if (msg.Text == "Да")
                                            {
                                                userVk.Stage = "Give me your phone";
                                                responseText = "Пожалуйста введите номер телефона который вы указывали при оформлении мероприятия";
                                                break;
                                            }
                                            if (msg.Text == "Нет")
                                            {
                                                userVk.Branch = "Main";
                                                userVk.Stage = "1";
                                                responseText = "Что вас интерисует?";
                                                keyboard = new KeyboardBuilder()
                                                    .AddButton("Оформить заказ", "createEvent", KeyboardButtonColor.Positive)
                                                    .AddButton("Получить прайс", "getPrice", KeyboardButtonColor.Default)
                                                    .SetInline(false)
                                                    .SetOneTime()
                                                    .AddLine()
                                                    .AddButton("Связаться с менеджером", "getManager", KeyboardButtonColor.Positive)
                                                    .AddButton("Заказать звонок", "getCall", KeyboardButtonColor.Default)
                                                    .AddLine()
                                                    .AddButton("Оставить отзыв или жалобу", "createResponse", KeyboardButtonColor.Primary)
                                                    .Build();
                                                break;
                                            }
                                            if (msg.Text == "Назад")
                                            {
                                                userVk.Branch = "Our client?";
                                                userVk.Stage = "Yes/No";
                                                responseText = "Вы являетесь нашим клиентом";
                                                keyboard = new KeyboardBuilder().AddButton("Да", "yes", KeyboardButtonColor.Positive)
                                                    .AddButton("Нет", "no", KeyboardButtonColor.Default)
                                                    .SetInline(false)
                                                    .SetOneTime()
                                                    .AddLine()
                                                    .AddButton("Назад", "back", KeyboardButtonColor.Primary)
                                                    .Build();
                                                break;
                                            }
                                            responseText = "Я вас не понимаю, пожалуйста используйте для ответов кнопки. У вас есть активное мероприятие?";
                                            keyboard = new KeyboardBuilder().AddButton("Да", "yes", KeyboardButtonColor.Positive)
                                            .AddButton("Нет", "no", KeyboardButtonColor.Default)
                                            .SetInline(false)
                                            .SetOneTime()
                                            .Build();
                                            break;
                                        case "Give me your phone":
                                            var tempClient = await db.Clients.Include(c => c.Manager).Include(c => c.Events).Where(c => c.Phone == msg.Text).FirstOrDefaultAsync();
                                            if (tempClient == null)
                                            {
                                                responseText = "Ошибка: такого номера нет в базе данных, убедитесь в правильности введённого номера и попробуйте ещё раз";
                                                keyboard = new KeyboardBuilder().Clear().Build();
                                            }
                                            else
                                            {
                                                tempClient.ClientVK = userVk;
                                                await db.SaveChangesAsync();
                                                var evnt = tempClient.Events.Where(e => e.Status != "Закрыто").FirstOrDefault();
                                                if (evnt == null)
                                                {
                                                    responseText = $"Вы есть в списке наших клиентов, но у вас не указано ни одного действующего мероприятия. В случае если у вас есть действующее мероприятие свяжитесь с ваши менеджером по телефону {tempClient.Manager.PhoneNumber}";
                                                }
                                                else
                                                {
                                                    responseText = $"Ваше мероприятие: {evnt.Name} Дата проведения: {evnt.DeadLine.ToShortDateString()}";
                                                }
                                                userVk.Branch = "Main";
                                                userVk.Stage = "0";
                                                keyboard = new KeyboardBuilder()
                                                    .AddButton("Оформить заказ", "createEvent", KeyboardButtonColor.Positive)
                                                    .AddButton("Получить прайс", "getPrice", KeyboardButtonColor.Default)
                                                    .SetInline(false)
                                                    .SetOneTime()
                                                    .AddLine()
                                                    .AddButton("Связаться с менеджером", "getManager", KeyboardButtonColor.Positive)
                                                    .AddButton("Заказать звонок", "getCall", KeyboardButtonColor.Default)
                                                    .AddLine()
                                                    .AddButton("Оставить отзыв или жалобу", "createResponse", KeyboardButtonColor.Primary)
                                                    .Build();
                                            }
                                            break;
                                        default:
                                            responseText = "Вы являетесь нашим клиентом ?";
                                            keyboard = new KeyboardBuilder().AddButton("Да", "yes", KeyboardButtonColor.Positive)
                                            .AddButton("Нет", "no", KeyboardButtonColor.Default)
                                            .SetInline(false)
                                            .SetOneTime()
                                            .Build();
                                            break;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        //switch (msg.Text)
                        //{
                        //    case "Получить прайс":
                        //        var works = db.Works.Where(w => w.Price != 0);
                        //        responseText = "";
                        //        foreach (var item in works)
                        //        {
                        //            responseText += $"{item.Name} - стоимость: {item.Price} руб. \n";
                        //        }
                        //        responseText += "-------------------------------------- \n";
                        //        responseText += "Для получения описания введите название услуги \n";
                        //        break;
                        //    case "Оформить заказ":
                        //        responseText = "Для оформления заказа напишите: \"Заказ15756: [описание вашего мероприятия и ваши пожелания]\"";
                        //        break;
                        //    case "Заказать звонок":
                        //        responseText = "Для заказа звонка напишите: \"Звонок7492: [ваш номер телефона, информация для менеджера]\"";
                        //        break;
                        //    case "Связаться с менеджером":
                        //        break;
                        //    case "Оставить отзыв или жалобу":
                        //        responseText = "Чтобы оставить отзыв/жалобу напишите: \"Отзыв331578: [текст отзыва/жалобы]\"";
                        //        break;
                        //    default:
                        //        if ((db.Works.Select(e => e.Name)).Contains(msg.Text))
                        //        {
                        //            var work = await db.Works.Where(w => w.Name == msg.Text).FirstOrDefaultAsync();
                        //            if (work != null)
                        //            {
                        //                responseText = $"{work.Name} \n--------------------------------------\n {work.Description}";
                        //            }
                        //            else
                        //            {
                        //                responseText = "Ошибка: проверьте правильность названия";
                        //            }
                        //        }
                        //        break;
                        //}

                        await db.SaveChangesAsync();
                        _vkApi.Messages.Send(new MessagesSendParams
                        {
                            RandomId = new DateTime().Millisecond,
                            PeerId = msg.PeerId.Value,
                            Message = responseText,
                            Keyboard = keyboard,
                            Payload = payload
                        });
                        break;
                    }
            }
            return Ok("ok");
        }
    }
}