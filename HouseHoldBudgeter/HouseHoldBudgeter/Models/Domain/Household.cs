using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseHoldBudgeter.Models.Domain
{
    public class Household
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public string CreatedById { get; set; }

        public virtual List<ApplicationUser> HouseholdJoinedMembers { get; set; }
        public virtual List<ApplicationUser> HouseholdInvitedMembers { get; set; }

        public virtual List<Category> HouseholdCategories { get; set; }

        public Household()
        {
            HouseholdJoinedMembers = new List<ApplicationUser>();
            HouseholdInvitedMembers = new List<ApplicationUser>();
            HouseholdCategories = new List<Category>();
        }
    }
}