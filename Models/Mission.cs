using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class Mission
    {
        public int Id { get; set; }

        public string MissionText { get; set; }

        public string Picture
        {
            get
            {
                switch (MissionType)
                {
                    case "Звонок":
                        return "/images/icons/Phone.png";
                    case "Email":
                        return "/images/icons/Email.png";
                    case "Договор":
                        return "/images/icons/Note.png";
                    case "Утверждение макета":
                        return "/images/icons/Copy.png";
                    case "Создание задачи":
                        return "/images/icons/Copy.png";
                    case "Указать артиста":
                        return "/images/icons/Playboy.png";
                    case "Указать техника":
                        return "/images/icons/Custom.png";
                    case "Заявка":
                        return "/images/icons/VK.png";
                    case "Автоматическая заявка":
                        return "/images/icons/Android.png";
                    default:
                        return "/images/icons/Check Box.png";
                }
            }
        }

        public DateTime DeadLine { get; set; }

        public DateTime CreatedDate { get; set; }

        public TimeSpan LeftTime { get { return DeadLine - DateTime.Now; } } 

        public string Status { get; set; }

        public string MissionType { get; set; }

        public bool IsFailed { get; set; }

        public string EmployeerId { get; set; }
        public User Employeer { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public string GetLeftTime()
        {
            var s =$"{LeftTime.Hours} часов {LeftTime.Minutes} минут";
            return s;
        }
    }
}
