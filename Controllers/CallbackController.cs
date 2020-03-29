﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                        var responseText = "default";
                        var payload = "";

                        var keyboard = new KeyboardBuilder()
                                            .Clear()
                                            .Build();

                        var kbdYesNo = new KeyboardBuilder().AddButton("Да", "yes", KeyboardButtonColor.Positive)
                                                    .AddButton("Нет", "no", KeyboardButtonColor.Default)
                                                    .SetInline(false)
                                                    .SetOneTime()
                                                    .Build();
                        var kbdForClient = new KeyboardBuilder()
                                            .AddButton("Оформить заказ", "createEvent", KeyboardButtonColor.Positive)
                                            .AddButton("Получить прайс", "getPrice", KeyboardButtonColor.Default)
                                            .SetInline(false)
                                            .SetOneTime()
                                            .AddLine()
                                            .AddButton("Связаться с менеджером", "getManager", KeyboardButtonColor.Positive)
                                            .AddButton("Заказать звонок", "getCall", KeyboardButtonColor.Default)
                                            .AddLine()
                                            .AddButton("Оставить отзыв", "createResponse", KeyboardButtonColor.Primary)
                                            .Build();
                        var kbdNoClient = new KeyboardBuilder()
                                                    .AddButton("Оформить заказ", "createEvent", KeyboardButtonColor.Positive)
                                                    .AddButton("Получить прайс", "getPrice", KeyboardButtonColor.Default)
                                                    .SetInline(false)
                                                    .SetOneTime()
                                                    .AddLine()
                                                    .AddButton("Заказать звонок", "getCall", KeyboardButtonColor.Default)
                                                    .AddLine()
                                                    .Build();
                        ///TODO: Определить ветви, этапы и общую схему взаимодействия. При отсутствии клиента добавлять его в БД
                        var user = _vkApi.Users.Get(new long[] { (long)msg.FromId }, VkNet.Enums.Filters.ProfileFields.Contacts).FirstOrDefault();

                        var userVk = await db.ClientsVK.Include(u => u.Client).Include(u => u.Client.Manager).Where(u => u.VkId == (long)msg.FromId).FirstOrDefaultAsync();
                        if (userVk == null)
                        {
                            userVk = new ClientVK(user)
                            {
                                Branch = "Our client?",
                                Stage = "Yes/No",
                               // Client = await db.Clients.Where(c => c.Name == "Temp").FirstAsync()
                            };
                            await db.ClientsVK.AddAsync(userVk);
                            await db.SaveChangesAsync();
                            keyboard = kbdYesNo;
                            responseText = "Добрый день, к сожалению я не нашёл записи о вас в нашей базе данных. Вы являетесь нашим клиентом?";
                        }
                        else
                        {//TODO: Добавить переход вверх по ветке. Добавить дефаулты и обработку неправильных ответов.(частично сделано). 
                            //TODO:Добавить контроль кол-ва вк-аккаунтов у пользователя, обработку новых ивентов, отзывов. приветствие по имени.
                            if (msg.Text == "/RETURN/")
                            {
                                userVk.Branch = "Our client?";
                                userVk.Stage = "Yes/No";
                                keyboard = kbdYesNo;
                                responseText = "Вы являетесь нашим клиентом?";
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
                            switch (userVk.Branch)
                            {
                                case "Our client?":
                                    switch (userVk.Stage)
                                    {
                                        case "Yes/No":
                                            if (msg.Text == "Да")
                                            {
                                                userVk.Stage = "Give me your phone";
                                                responseText = "Пожалуйста введите номер телефона который вы указывали при оформлении мероприятия";
                                                keyboard = new KeyboardBuilder().Clear().Build();
                                                break;
                                            }
                                            if (msg.Text == "Нет")
                                            {
                                                userVk.Branch = "Main";
                                                userVk.Stage = "1";
                                                responseText = "Что вас интерисует?";
                                                keyboard = kbdNoClient;
                                                break;
                                            }
                                            responseText = "Я вас не понимаю, пожалуйста используйте для ответов кнопки. Вы являетесь нашим клиентом ?";
                                            keyboard = kbdYesNo;
                                            break;
                                        case "Give me your phone":
                                            var tempClient = await db.Clients.Include(c => c.Manager).Include(c => c.ClientVK).Include(c => c.Events).Where(c => c.Phone == msg.Text).FirstOrDefaultAsync();
                                            if (tempClient == null)
                                            {
                                                responseText = "Ошибка: такого номера нет в базе данных, убедитесь в правильности введённого номера и попробуйте ещё раз";
                                                keyboard = new KeyboardBuilder().Clear().Build();
                                            }
                                            else
                                            {
                                                tempClient.ClientVK.Add(userVk);
                                                await db.SaveChangesAsync();
                                                var evnt = tempClient.Events.Where(e => e.Status != "Закрыто").FirstOrDefault();
                                                if (evnt == null)
                                                {
                                                    responseText = $"Вы есть в списке наших клиентов, но у вас не указано ни одного текущего мероприятия. В случае если у вас есть действующее мероприятие свяжитесь с ваши менеджером по телефону {tempClient.Manager.PhoneNumber}";
                                                }
                                                else
                                                {
                                                    responseText = $"Ваше мероприятие: {evnt.Name} Дата проведения: {evnt.DeadLine.ToShortDateString()}";
                                                }
                                                userVk.Branch = "Main";
                                                userVk.Stage = "0";
                                                keyboard = kbdForClient;
                                            }
                                            break;
                                        default:
                                            responseText = "Вы являетесь нашим клиентом ?";
                                            keyboard = kbdYesNo;
                                            break;
                                    }
                                    break;
                                case "Main":
                                    switch (userVk.Stage)
                                    {
                                        case "1":
                                            switch (msg.Text)
                                            {
                                                case "Получить прайс":
                                                    var works = db.Works.Where(w => w.Price != 0);
                                                    responseText = "";
                                                    var builder = new KeyboardBuilder();
                                                    foreach (var item in works)
                                                    {
                                                        responseText += $"{item.Name} - стоимость: {item.Price} руб. \n";
                                                    }
                                                    keyboard = new KeyboardBuilder().Clear().Build();
                                                    responseText += "-------------------------------------- \n";
                                                    responseText += "Для получения описания введите название услуги \n";
                                                    break;
                                                case "Оформить заказ":
                                                    responseText = "Введите планируемую дату и время вашего мероприятия";
                                                    userVk.Branch = "newEvent";
                                                    userVk.Stage = "1";
                                                    break;
                                                case "Заказать звонок":
                                                    responseText = "Введите ваш номер телефона: ";
                                                    userVk.Branch = "CallMe";
                                                    userVk.Stage = "1";
                                                    break;
                                                default:
                                                    if ((db.Works.Select(e => e.Name)).Contains(msg.Text))
                                                    {
                                                        var work = await db.Works.Where(w => w.Name == msg.Text).FirstOrDefaultAsync();
                                                        if (work != null)
                                                        {
                                                            responseText = $"{work.Name} \n--------------------------------------\n {work.Description}";
                                                        }
                                                        else
                                                        {
                                                            responseText = "Ошибка: проверьте правильность названия";
                                                        }
                                                    }
                                                    break;
                                            }
                                            break;
                                        case "0":
                                            switch (msg.Text)
                                            {
                                                case "Получить прайс":
                                                    var works = db.Works.Where(w => w.Price != 0);
                                                    responseText = "";
                                                    var builder = new KeyboardBuilder();
                                                    var i = 0;
                                                    foreach (var item in works)
                                                    {
                                                        responseText += $"{item.Name} - стоимость: {item.Price} руб. \n";
                                                    }
                                                    keyboard = new KeyboardBuilder().Clear().Build();
                                                    responseText += "-------------------------------------- \n";
                                                    responseText += "Для получения описания введите название услуги \n";
                                                    break;
                                                case "Оформить заказ":
                                                    responseText = "Введите планируемую дату и время вашего мероприятия";
                                                    userVk.Branch = "newEvent";
                                                    userVk.Stage = "1";
                                                    break;
                                                case "Заказать звонок":
                                                    responseText = "Введите ваш номер телефона: ";
                                                    userVk.Branch = "CallMe";
                                                    userVk.Stage = "1";
                                                    break;
                                                case "Оставить отзыв":
                                                    responseText = "Введите текст отзыва: ";
                                                    userVk.Branch = "CreateResponse";
                                                    userVk.Stage = "1";
                                                    break;
                                                case "Связаться с менеджером":
                                                    responseText = $"Контакты вашего менеджера:\n Электронная почта:{userVk.Client.Manager.Email}\n Телефонный номер:{userVk.Client.Manager.PhoneNumber}\n ВК:{userVk.Client.Manager.VkAdress}\n Телеграмм:{userVk.Client.Manager.TgAdress}";
                                                    break;
                                                default:
                                                    if ((db.Works.Select(e => e.Name)).Contains(msg.Text))
                                                    {
                                                        var work = await db.Works.Where(w => w.Name == msg.Text).FirstOrDefaultAsync();
                                                        if (work != null)
                                                        {
                                                            responseText = $"{work.Name} \n--------------------------------------\n {work.Description}";
                                                        }
                                                        else
                                                        {
                                                            responseText = "Ошибка: проверьте правильность названия";
                                                        }
                                                    }
                                                    break;
                                            }
                                            break;
                                    }
                                    break;
                                case "CallMe":
                                    switch (userVk.Stage)
                                    {
                                        case "1":
                                            if (Regex.IsMatch(msg.Text, @"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{7,10}$", RegexOptions.IgnoreCase))
                                            {
                                                var mission = new Mission() { CreatedDate = DateTime.Now, DeadLine = DateTime.Now.AddHours(3), MissionType ="Звонок", MissionText = $"Позвонить клиенту для разрешения вопроса. Номер телефона: {msg.Text}", Status =$"{userVk.VkId}", Event = new Event() {Name = "TempEvent"} };
                                                userVk.Stage = "2";
                                                responseText = "Отлично! Теперь опишите суть своего вопроса";
                                                await db.Missions.AddAsync(mission);
                                                await db.SaveChangesAsync();
                                                
                                            }
                                            else
                                            {
                                                responseText = "Введённый номер не соответствует формату телефонных номеров, проверьте правильность введённого номера";
                                            }
                                            break;
                                        case "2":
                                            var missiondb = await db.Missions.Where(m => m.Status == $"{userVk.VkId}").FirstOrDefaultAsync();
                                            var min = 9999999;
                                            var ident = "";
                                            foreach (var item in db.Managers.Include(e => e.Missions))
                                            {
                                                if (item.Missions.Count() < min)
                                                {
                                                   ident = item.Id;
                                                }
                                            }
                                            var manager = await db.Managers.Where(m => m.Id == ident).FirstOrDefaultAsync();
                                            missiondb.MissionText += $", сообщение клиента: {msg.Text}";
                                            missiondb.Status = "Создано";
                                            manager.Missions.Add(missiondb);
                                            await db.SaveChangesAsync();
                                            responseText = "Менеджер свяжется с вами в ближайшее время";
                                            userVk.Branch = "Main";
                                            userVk.Stage = "1";
                                            keyboard = kbdNoClient;
                                            break;
                                    }
                                    break;
                                case "newEvent":
                                    switch (userVk.Stage)
                                    {
                                        case "1":
                                            var mission = new Mission() { CreatedDate = DateTime.Now, DeadLine = DateTime.Now.AddHours(24), Event = new Event() { Name = "TempEvent" }, MissionType = "Звонок", Status = $"{userVk.VkId}", MissionText =$"Позвонить клиенту для согласования нового мероприятия. Дата и время: {msg.Text}" };
                                            await db.Missions.AddAsync(mission);
                                            await db.SaveChangesAsync();
                                            userVk.Stage = "2";
                                            responseText = "Введите описание вашего мероприятия и номер телефона для связи, иначе связь будет осуществляться через личные сообщения ВК";
                                            break;
                                        case "2":
                                            var missionDb = await db.Missions.Where(m => m.Status == $"{userVk.VkId}").FirstOrDefaultAsync();
                                            missionDb.MissionText += $", Описание(со слов клиента): {msg.Text}. Заявка оставлена через Бота ВК. Клиент: {userVk.VkAdress}";
                                            responseText = "Отлично, в ближайшее время с вами свяжется менеджер. Всего доброго))";
                                            await db.SaveChangesAsync();
                                            userVk.Branch = "Main";
                                            userVk.Stage = "1";
                                            keyboard = kbdNoClient;
                                            break;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        

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