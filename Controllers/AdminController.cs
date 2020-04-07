﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OXG.CRM_System.Data;
using OXG.CRM_System.Models;
using OXG.CRM_System.ViewModels;

namespace OXG.CRM_System.Controllers
{
    [Authorize(Roles ="Администратор")]
    public class AdminController : Controller
    {
        CRMDbContext db;
        IWebHostEnvironment appEnvironment;
        public AdminController(CRMDbContext context, IWebHostEnvironment _appEnvironment)
        {
            db = context;
            appEnvironment = _appEnvironment;
        }


        public async Task<IActionResult> Index()
        {
            var data = new AdminIndexVM();
            data.Last30Days = new List<string>();
            data.EventsSum = new List<decimal>();
            data.RejectNum = new List<int>();
            data.WorksName = new List<string>();
            data.WorksNum = new List<int>();
            data.TypesName = new List<string>();
            data.TypesCount = new List<int>();
            data.ManagerName = new List<string>();
            data.ManagerRequestCount = new List<int>();
            for (int i = 0; i < 30; i++)
            {
                data.Last30Days.Add(DateTime.Now.AddDays(-i).ToShortDateString());
                data.EventsSum.Add(await db.Events.Where(e => e.CreatedDate.DayOfYear == DateTime.Now.AddDays(-i).DayOfYear).SumAsync(e => e.TotalPrice));
            
            }
            
            data.RejectNum.Add(await db.Missions.Where(m => m.Status =="Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Спам")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Высокая стоимость услуг")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Нет свободной аппаратуры")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Нет свободного реквизита")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Нет свободных артистов")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Недостаточный ассортимент услуг")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Не соответствие площадки ТБ")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Слишком далеко")).CountAsync());
            data.RejectNum.Add(await db.Missions.Where(m => m.Status == "Закрыто" && m.MissionType == "Заявка" && m.MissionText.Contains("Нет разрешения от спец. служб")).CountAsync());


            foreach (var work in db.Works)
            {
                data.WorksName.Add(work.Name);
            }
            for (int i = 0; i < data.WorksName.Count(); i++)
            {
                data.WorksNum.Add(await db.Events.Include(e => e.Works).Where(e => e.Works.Where(w => w.Name == data.WorksName[i]).Count() != 0).CountAsync());
            }

            data.TypesName = TypesAndStaticValues.GetEventTypesList();
            for (int i = 0; i < data.TypesName.Count(); i++)
            {
                data.TypesCount.Add(await db.Events.Where(e => e.EventType == data.TypesName[i]).CountAsync());
            }

            
            foreach (var item in db.Managers)
            {
                data.ManagerName.Add(item.Name);
                data.ManagerRequestCount.Add(item.MissionFromRequestNum);
            }

            data.EventsSum.Reverse();
            data.Last30Days.Reverse();
            return View(data);
        }
    }
}