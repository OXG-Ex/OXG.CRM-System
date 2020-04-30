using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Data
{
    public static class WatchDog
    {
        public static async Task FindDeadlineAsync(CRMDbContext db)
        {

            var tasks = db.Missions.Include(m => m.Employeer);

            foreach (var item in tasks)
            {
                if (item.LeftTime.TotalSeconds < 0 && item.Status != "Закрыто" && item.IsFailed == false)
                {
                    item.IsFailed = true;
                    var notice = new Notice
                    {
                        EmployeerId = item.EmployeerId,
                        EmployeerName = item.Employeer.Name,
                        Text = "Провален дедлайн",
                        MissionId = item.Id,
                        IsViewed = false,
                        Deadline = item.DeadLine
                    };
                    await db.Notices.AddAsync(notice);

                }
            }

        }

        public static async Task FindNewRequestAsync(CRMDbContext db)
        {
            var events = db.Events.Include(e => e.Client);
            foreach (var item in events)
            {
                if (item.CreatedDate.Date == DateTime.Now.Date.AddDays(14) && db.Missions.Where(m => m.MissionText == $"Предложить клиенту проведение мероприятия. Заявка создана автоматически на основе уже проводившегося {item.DeadLine.ToShortDateString()} мероприятия <a href=/events/view?id={item.Id}>{item.Name}</a>").FirstOrDefault() == null)
                {
                    var req = new Mission() { Employeer = db.GetMustFreedomManager(), MissionType = "Автоматическая заявка", Status = "Создано", MissionText = $"Предложить клиенту проведение мероприятия. Заявка создана автоматически на основе уже проводившегося {item.DeadLine.ToShortDateString()} мероприятия <a href=/events/view?id={item.Id}>{item.Name}</a>", CreatedDate = DateTime.Now, Event = await db.Events.FirstOrDefaultAsync(), DeadLine = DateTime.Now.AddDays(3) };
                    await db.Missions.AddAsync(req);
                }
            }
            await db.SaveChangesAsync();
        }
    }
}
