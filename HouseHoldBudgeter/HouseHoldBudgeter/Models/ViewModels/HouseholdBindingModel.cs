﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseHoldBudgeter.Models.ViewModels
{
    public class HouseholdBindingModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }


    }
}