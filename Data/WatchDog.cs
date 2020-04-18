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
                        Text = "Сотрудник провалил дедлайн",
                        MissionId = item.Id
                    };
                    await db.Notices.AddAsync(notice);
                    await db.SaveChangesAsync();
                }
            }

        }
    }
}
