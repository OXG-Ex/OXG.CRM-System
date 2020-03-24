﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyDox;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using OXG.CRM_System.Models;
using TemplateEngine.Docx;

namespace OXG.CRM_System.Controllers
{
    public class ContractController : Controller
    {
        CRMDbContext db;
        IWebHostEnvironment appEnvironment;
        public ContractController(CRMDbContext context, IWebHostEnvironment _appEnvironment)
        {
            db = context;
            appEnvironment = _appEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Create(int id)
        {
            var pathDocument = appEnvironment.ContentRootPath + "\\wwwroot\\files\\template.docx";
            var docFile = new FileInfo(pathDocument);
            var eventDb = await db.Events.Include(e => e.Client).Include(e => e.Manager).Include(e => e.Missions).Include(e => e.Works).Include(e => e.Contract).Where(e => e.Id == id).FirstOrDefaultAsync();
            var workTable = new TableContent("Works");
            var i = 0;
            var filename = $"{eventDb.Id}" + ".docx";
            

            foreach (var item in eventDb.Works)
            {
                i++;
                workTable.AddRow(
                    new FieldContent("WorkIndex", $"{i}"),
                    new FieldContent("WorkName", item.Name),
                    new FieldContent("WorkNum", $"{item.Num}"),
                    new FieldContent("WorkSum", $"{item.Sum}"));
            }

            var valuesToFill = new Content(new FieldContent("docDate", DateTime.Now.ToShortDateString()),
                                           new FieldContent("docNum",$"{eventDb.Id}" ),
                                           new FieldContent("docClient", $"{eventDb.Client.Name}"),
                                           workTable);

            using (var outputDocument = new TemplateProcessor(pathDocument)
                .SetRemoveContentControls(false))
            {
                outputDocument.FillContent(valuesToFill);
                outputDocument.SaveChanges();
            }

            docFile.CopyTo(appEnvironment.ContentRootPath + $"\\wwwroot\\files\\contracts\\{filename}",true);

            var contract = new Contract() { Event = eventDb, Manager = eventDb.Manager, Name = filename, CreatedDate = DateTime.Now, Client = eventDb.Client, Path = appEnvironment.ContentRootPath + $"\\wwwroot\\files\\contracts\\{filename}" };
            await db.AddAsync(contract);
            await db.SaveChangesAsync();

            var mission = eventDb.Missions.Where(m => m.MissionType == "Создать договор").FirstOrDefault();
            mission.Status = "Закрыто";
            await db.SaveChangesAsync();
            return View();
        }
    }
}