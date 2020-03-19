using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Data
{
    public static class TypesAndStaticValues
    {
        public static List<string> EventTypes = new List<string>
        {
            "Огненное шоу",
            "Пиротехническое шоу",
            "Светодиодное шоу",
            "Шоу мыльных пузырей",
            "Холодный огонь",
            "Шоу Тесла",
            "Аниматор",
            "Ведущий",
            "Ходулисты",
            "Фрик шоу",
            "Молекулярное шоу",
            "Танцовщицы"
        };

        public static List<string> MissionTypes = new List<string>
        {
            "Клиент",
            "Артист",
            "Техник",
            "Договор"
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
