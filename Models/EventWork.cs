﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Models
{
    public class EventWork
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Num { get; set; }

        public decimal Price { get; set; }

        public decimal Sum { get => Num * Price; }

        public EventWork()
        {

        }

        public EventWork(Work work, int num)
        {
            Name = work.Name;

            Num = num;

            Price = work.Price;
        }
    }
}
