using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class ClientVK
    {
        public int? Id { get; set; }

        public long VkId { get; set; }

        public string VkAdress { get; set; }

        public string VkName { get; set; } //$"{FirstName} {LastName}"

        public string Stage { get; set; }

        public string Branch { get; set; }

        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        public ClientVK()
        {

        }

        public ClientVK(VkNet.Model.User user)
        {
            VkId = user.Id;

            VkAdress = $"vk.com/id{VkId}";

            VkName = $"{user.FirstName} {user.LastName}";
        }
    }
}
