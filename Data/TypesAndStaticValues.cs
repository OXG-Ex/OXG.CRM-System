using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Data
{
    public static class TypesAndStaticValues
    {
        private static List<string> EventTypes = new List<string>
        {
            "День рождения (Взрослый)",
            "День рождения (Детский)",
            "Свадьба",
            "Массовое",
            "День св. Валентина",
            "23 февраля",
            "Новый год",
            "Корпоратив"
        };

        private static List<string> MissionTypes = new List<string>
        {
            "Клиент",
            "Артист",
            "Техник",
            "Договор",
            "Email",
            "Заявка",
            "Звонок"
        };

        public static List<string> RejectCauses = new List<string>
        {
            "Спам",
            "Высокая стоимость услуг",
            "Нет свободной аппаратуры",
            "Нет свободного реквизита",
            "Нет свободных артистов",
            "Недостаточный ассортимент услуг",
            "Не соответствие площадки ТБ",
            "Слишком далеко",
            "Нет разрешения от спец. служб"
        };

        public static SelectList GetEventTypes()
        {
            return new SelectList(EventTypes);
        }

        public static SelectList GetMissionTypes()
        {
            return new SelectList(MissionTypes);
        }
    }
}
