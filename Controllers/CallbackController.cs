using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace OXG.CRM_System.Controllers
{
    /// <summary>
    /// Контроллер Бота ВК, реагирует на присылаемые JSON объекты по адресу api/callback 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration _configuration;

        private readonly IVkApi _vkApi;//Поле для работы с АПИ ВКонтакта

        private readonly CRMDbContext db;

        //Внедрение зависимостей
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

        /// <summary>
        /// Основной метод работающий с клиентом
        /// </summary>
        /// <param name="updates"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Callback([FromBody] VK_Updates updates)
        {
            switch (updates.Type)
            {
                ///Если вервер запрашивает подтверждение
                case "confirmation":
                    //Возвращается строка указанная в appsettings.json/Config/Confirmation
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
                                            //.SetOneTime()
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
                                                    //.SetOneTime()
                                                    .AddLine()
                                                    .AddButton("Заказать звонок", "getCall", KeyboardButtonColor.Default)
                                                    .AddLine()
                                                    .Build();
                        var kbdReturn = new KeyboardBuilder()
                                                    .AddButton("/RETURN/", "return", KeyboardButtonColor.Default)
                                                    .SetInline(false)
                                                    .Build();
                        //получение пользователя от ВК по id
                        var user = _vkApi.Users.Get(new long[] { (long)msg.FromId }, VkNet.Enums.Filters.ProfileFields.Contacts).FirstOrDefault();
                        //Поиск клиента в БД
                        var userVk = await db.ClientsVK.Include(u => u.Client).Include(u => u.Client.Manager).Where(u => u.VkId == msg.FromId).FirstOrDefaultAsync();
                        //Если не существует такого клиента в БД
                        if (userVk == null)
                        {
                            //Создание нового ВК-клиента
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
                         ////!!!!!!!!!ПРОКОММЕНТИРОВАТЬ ВЕСЬ КОЛБЭК КОНТРОЛЛЕР!!!!!!!!!!!!!!!!!

                            ///Сброс всех ветвей и стадий, для тестирования
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

                            //переключение по веткам
                            switch (userVk.Branch)
                            {
                                //Ветка выяснения, является ли пользователь клиентом агентства
                                case "Our client?":
                                    switch (userVk.Stage)
                                    {
                                        case "Yes/No":
                                            if (msg.Text == "Да")
                                            {
                                                userVk.Stage = "Give me your phone";
                                                responseText = "Пожалуйста введите номер телефона который вы указывали при оформлении мероприятия";
                                                keyboard = kbdReturn;
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
                                        //Поиск клиента агенства по номеру телефона
                                        case "Give me your phone":
                                            var tempClient = await db.Clients.Include(c => c.Manager).Include(c => c.ClientVK).Include(c => c.Events).Where(c => c.Phone == msg.Text).FirstOrDefaultAsync();
                                            if (tempClient == null)
                                            {
                                                responseText = "Ошибка: такого номера нет в базе данных, убедитесь в правильности введённого номера и попробуйте ещё раз";
                                                keyboard = kbdReturn;
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
                                                    responseText = $"Здравствуйте, {tempClient.Name}, Ваше ближайшее мероприятие: {evnt.Name}, дата проведения: {evnt.DeadLine.ToShortDateString()}";
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

                                //Ветвь основного меню
                                case "Main":
                                    switch (userVk.Stage)
                                    {
                                        //Главный функционал для пользователей не являющихся клиентами 
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

                                        // Главный функционал для клиентов
                                        case "0":
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

                                                case "Оставить отзыв":
                                                    responseText = "Введите текст отзыва: ";
                                                    userVk.Branch = "CreateResponse";
                                                    userVk.Stage = "1";
                                                    break;

                                                case "Связаться с менеджером":
                                                    responseText = $"Контакты вашего менеджера:\n Электронная почта:{userVk.Client.Manager.Email}\n Телефонный номер:{userVk.Client.Manager.PhoneNumber}\n ВК:{userVk.Client.Manager.VkAdress}\n Телеграмм:{userVk.Client.Manager.TgAdress}";
                                                    keyboard = kbdForClient;
                                                    break;
                                                //Возвращает информацию об услуге если она есть в БД
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

                                //Ветвь заказа звонка
                                case "CallMe":
                                    switch (userVk.Stage)
                                    {
                                        //Получение номера телефона
                                        case "1":
                                            if (Regex.IsMatch(msg.Text, @"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{7,10}$", RegexOptions.IgnoreCase))
                                            {
                                                var mission = new Mission() { CreatedDate = DateTime.Now, DeadLine = DateTime.Now.AddHours(3), MissionType = "Заявка", MissionText = $"Заявка на звонок. Номер телефона: {msg.Text}", Status = $"{userVk.VkId}", Event = await db.Events.Where(e => e.Name == "TempEvent").FirstOrDefaultAsync() };
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
                                        //Создание заявки для менеджера
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
                                            missiondb.MissionText += $", сообщение клиента: {msg.Text}. Заявка оставлена через Бота ВК. Клиент: <a href=https://{userVk.VkAdress}>{userVk.VkName}</a>";
                                            missiondb.Status = "Создано";
                                            manager.Missions.Add(missiondb);
                                            await db.SaveChangesAsync();
                                            responseText = "Отлично, в ближайшее время с вами свяжется менеджер. Всего доброго))";
                                            userVk.Branch = "Main";
                                            userVk.Stage = "1";
                                            keyboard = kbdNoClient;
                                            break;
                                    }
                                    break;

                                //Ветвь заявки на новое мероприятие
                                case "newEvent":
                                    switch (userVk.Stage)
                                    {
                                        case "1":
                                            var mission = new Mission()
                                            {
                                                CreatedDate = DateTime.Now,
                                                DeadLine = DateTime.Now.AddHours(24),
                                                Event = await db.Events.Where(e => e.Name == "TempEvent").FirstOrDefaultAsync(), //Установка временного мероприятия дабы избежать исключения
                                                MissionType = "Заявка",
                                                Status = $"{userVk.VkId}",
                                                MissionText = $"Заявка на проведение мероприятия. Дата и время: {msg.Text}"
                                            };
                                            await db.Missions.AddAsync(mission);
                                            await db.SaveChangesAsync();
                                            userVk.Stage = "2";
                                            responseText = "Введите описание вашего мероприятия и номер телефона для связи, иначе связь будет осуществляться через личные сообщения ВК";
                                            break;
                                        //Сохранение описание и контактов клиента, установка менеджера с минимальным числом заданий
                                        case "2":
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
                                            var missionDb = await db.Missions.Where(m => m.Status == $"{userVk.VkId}" && m.EmployeerId == null).FirstOrDefaultAsync();
                                            missionDb.MissionText += $", Описание(со слов клиента): {msg.Text}. Заявка оставлена через Бота ВК. Клиент: <a href=https://{userVk.VkAdress}>{userVk.VkName}</a>";
                                            missionDb.Status = "Создано";
                                            responseText = "Отлично, в ближайшее время с вами свяжется менеджер. Всего доброго))";
                                            missionDb.Employeer = manager;
                                            await db.SaveChangesAsync();
                                            userVk.Branch = "Main";

                                            if (userVk.Client != null)
                                            {
                                                keyboard = kbdForClient;
                                                userVk.Stage = "0";
                                            }
                                            else
                                            {
                                                keyboard = kbdNoClient;
                                                userVk.Stage = "1";
                                            }
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