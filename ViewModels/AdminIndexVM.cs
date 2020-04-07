using OXG.CRM_System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.ViewModels
{
    public class AdminIndexVM
    {
        public List<string> Last30Days { get; set; }

        public List<decimal> EventsSum { get; set; } //Список сумм мероприятий по дням за последние 30 дней

        public List<string> RejectCauses2 { get; set; } = TypesAndStaticValues.RejectCauses;//Причины отклонения заявок

        public List<int> RejectNum { get; set; } //Список значений отклоненных мероприятий

        public List<string> WorksName { get; set; }//Наименования услуг

        public List<int> WorksNum { get; set; } //Кол-во мероприятий на которых была заказана услуга 

        public List<string> TypesName { get; set; }//Наименование типов мероприятий

        public List<int> TypesCount { get; set; }//Кол-во мероприятий данного типа

        public List<string> ManagerName { get; set; }//Наименование типов мероприятий

        public List<int> ManagerRequestCount { get; set; }//Кол-во мероприятий данного типа
    }
}
