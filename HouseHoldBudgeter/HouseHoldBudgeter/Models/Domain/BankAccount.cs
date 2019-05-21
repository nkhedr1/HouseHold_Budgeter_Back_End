using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseHoldBudgeter.Models.Domain
{
    public class BankAccount
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public decimal Balance { get; set; }

        public virtual Household HouseHold { get; set; }
        public int HouseHoldId { get; set; }

        public virtual List<Transaction> Transactions { get; set; }

        public BankAccount()
        {
            Balance = 0;
            Transactions = new List<Transaction>();
        }
    }
}