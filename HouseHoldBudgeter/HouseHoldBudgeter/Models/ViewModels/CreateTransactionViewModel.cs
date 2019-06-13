using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseHoldBudgeter.Models.ViewModels
{
    public class CreateTransactionViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public decimal Amount { get; set; }
        public bool VoidTransaction { get; set; }
        public int HouseholdId { get; set; }
        public int CategoryId { get; set; }
        public List<CategoryViewModel> Categories { get; set; }
    }
}