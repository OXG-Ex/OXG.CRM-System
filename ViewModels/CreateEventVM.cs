using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.ViewModels
{
    public class CreateEventVM
    {
        [Required(ErrorMessage = "Введите название мероприятия")]
        public string EventName { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required(ErrorMessage = "Укажите дату мероприятия")]
        public DateTime DeadLine { get; set; }

        [Required(ErrorMessage = "Введите описание мероприятия")]
        public string EventDescription { get; set; }

        public string EventStatus { get; set; }

        [Required(ErrorMessage = "Введите адрес проведения мероприятия")]
        public string EventLocation { get; set; }

        [Required(ErrorMessage = "Выберите тип мероприятия")]
        public string EventType { get; set; }

        [Required(ErrorMessage = "Введите имя клиента")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "Введите номер телефона клиента")]
        [Phone]
        public string ClientPhone { get; set; }

        public string ClientEmail { get; set; }

        public bool FromRequest { get; set; }

        [Required(ErrorMessage = "Создайте краткое описание клиента")]
        public string ClientDescription { get; set; }
    }
}
