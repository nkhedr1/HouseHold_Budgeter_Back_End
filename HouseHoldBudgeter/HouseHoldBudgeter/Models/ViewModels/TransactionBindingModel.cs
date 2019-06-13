using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseHoldBudgeter.Models.ViewModels
{
    public class TransactionBindingModel
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int BankAccountId { get; set; }

        [Required]
        public DateTime Date { get; set; }


    }
}