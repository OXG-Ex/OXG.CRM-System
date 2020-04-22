using OXG.CRM_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.ViewModels
{
    public class NoticeVM
    {
        public string EmployeerName { get; set; }

        public string Text { get; set; }

        public string AgoTime { get; set; }

        public int NoticeNum { get; set; }

        public string EmployeerId { get; set; }

        public int MissionId { get; set; }

        public NoticeVM()
        {

        }

        public NoticeVM(Notice notice)
        {
            EmployeerName = notice.EmployeerName;
            Text = notice.Text;
            EmployeerId = notice.EmployeerId;
            MissionId = notice.MissionId;
            var timeSpan = DateTime.Now - notice.Deadline;
            if (timeSpan.TotalMinutes<120)
            {
                AgoTime = $"{(int)timeSpan.TotalMinutes} минут назад";
            }
            else
            {
                AgoTime = $"{(int)timeSpan.TotalHours} часов назад";
            }
        }
    }
}
