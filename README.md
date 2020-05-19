 #### ! Проект временно заморожен
# OXG.CRM-System

CRM-система предназначенная для предприятия оказывающего развлекательные услуги.


## Начало работы

Для запуска проекта на локальном сервере у вас должен быть установлен MS SQL Server.
База данных автоматически инициализируется при первом запуске.

Для использования Бота ВК вам будет необходимо после развёртывания проекта, настроить свою группу для работы с ботом,
 [инструкция ](https://habr.com/ru/post/441720/) - пункт настройка CallBackAPI. Строка подтверждения возвращаемая приложением и токен доступа находятся в файле appsettings.json, в секции Config.
Логика бота находится в методе [Post]Callback контроллера CallbackController.

Для изменения функции автоматического создания договоров для клиента необходимо:
 * Изменить шаблон договора в соответствии с вашими требованиями [инструкция](https://habr.com/ru/post/269307/)
 * Изменить логику генерации договора в методе Create, контроллера ContractController.


## Используемые NuGet-пакеты (Список зависимостей)

* MailKit (2.5.1)
* Microsoft.AspNetCore.Identity.EntityFrameworkCore (3.1.2)
* Microsoft.AspNetCore.Mvc.NewtonsoftJson (3.1.3)
* Microsoft.EntityFrameworkCore (3.1.2)
* Microsoft.EntityFrameworkCore.SqlServer (3.1.2)
* Microsoft.VisualStudio.Web.CodeGeneration.Design (3.1.2)
* System.IO (4.3.0)
* System.IO.Packaging (4.7.0)
* System.Linq (4.3.0)
* TemplateEngine.Docx (1.1.5)
* VkNet (1.50.0)