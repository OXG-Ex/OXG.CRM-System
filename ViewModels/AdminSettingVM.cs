using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.ViewModels
{
    public class AdminSettingVM
    {
        [Required(ErrorMessage ="Данное поле обязательно для заполнения")]
        public string EmailLogin { get; set; }

        [Required(ErrorMessage = "Данное поле обязательно для заполнения")]
        public string EmailPassword { get; set; }

        [Required(ErrorMessage = "Данное поле обязательно для заполнения")]
        public string CompanyName { get; set; }
    }
}
