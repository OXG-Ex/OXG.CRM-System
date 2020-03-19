using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Xceed.Words.NET;

namespace OXG.CRM_System.Controllers
{
    public class ContractController : Controller
    {
        private IWebHostEnvironment appEnvironment;

        public ContractController(IWebHostEnvironment webHost)
        {
            appEnvironment = webHost;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            var path = appEnvironment.WebRootPath;
            //TODO: Создать шаблон документа и настроить в нём подстановку данных
            var file = new FileInfo(@path+ @"/files/contract.docx");
            using (var document = DocX.Load(file.FullName))
            {
                ///Заменяет текст cafedraName на "Зельеварения"
                document.ReplaceText("cafedraName", "Зельеварения");

                document.Save();
            }
            return View();
        }
    }
}