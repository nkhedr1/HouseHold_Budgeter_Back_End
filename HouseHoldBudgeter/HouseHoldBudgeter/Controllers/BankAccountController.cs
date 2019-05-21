using HouseHoldBudgeter.Models;
using HouseHoldBudgeter.Models.Domain;
using HouseHoldBudgeter.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseHoldBudgeter.Controllers
{
    [RoutePrefix("api/BankAccount")]
    public class BankAccountController : ApiController
    {
        private ApplicationDbContext DbContext;

        public BankAccountController()
        {
            DbContext = new ApplicationDbContext();
        }

        [HttpPost]
        [Authorize]
        [Route("CreateBankAccount/{id:int}")]
        public IHttpActionResult CreateBankAccount(int id, BankAccountBindingModel model)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
               house => house.Id == id);

            var userId = User.Identity.GetUserId();

            if (currentHousehold == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId)
            {
                BankAccount newBankAccount;

                newBankAccount = new BankAccount();
                newBankAccount.Name = model.Name;
                newBankAccount.Description = model.Description;
                newBankAccount.DateCreated = DateTime.Today;

                currentHousehold.BankAccounts.Add(newBankAccount);
                DbContext.SaveChanges();

                return Ok("Bank Account created");
            }
            else
            {
                return BadRequest("User not owner of household");
            }
        }

        [HttpPost]
        [Authorize]
        [Route("EditBankAccount/{id:int}/{bankAccountId:int}")]
        public IHttpActionResult EditBankAccount(int id, int bankAccountId, BankAccountViewModel model)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == id);

            var currentBankAccount = currentHousehold.BankAccounts.FirstOrDefault(
               cat => cat.Id == bankAccountId);

            var userId = User.Identity.GetUserId();

            if (currentHousehold == null || currentBankAccount == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId)
            {
                currentBankAccount.Name = model.Name;
                currentBankAccount.Description = model.Description;
                currentBankAccount.DateUpdated = DateTime.Today;

                DbContext.SaveChanges();
                return Ok("Bank Account Edited");
            }
            else
            {
                return BadRequest("User not owner of household");
            }

        }

        [HttpPost]
        [Authorize]
        [Route("DeleteBankAccount/{id:int}/{bankAccountId:int}")]
        public IHttpActionResult DeleteBankAccount(int id, int bankAccountId)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == id);

            var currentBankAccount = currentHousehold.BankAccounts.FirstOrDefault(
               cat => cat.Id == bankAccountId);

            var userId = User.Identity.GetUserId();

            if (currentHousehold == null || currentBankAccount == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId)
            {

                DbContext.BankAccounts.Remove(currentBankAccount);
                DbContext.SaveChanges();
                return Ok("Bank Account Deleted");
            }
            else
            {
                return BadRequest("User not owner of household");
            }

        }

        [HttpGet]
        [Authorize]
        [Route("ViewAllHouseholdBankAccounts/{id:int}")]
        public IHttpActionResult ViewAllHouseholdBankAccounts(int id)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == id);

            var userId = User.Identity.GetUserId();

            var currentLoggedUser = DbContext.Users.FirstOrDefault(
            usr => usr.Id == userId);

            if (currentHousehold == null)
            {
                return NotFound();
            }

            if (currentHousehold.HouseholdJoinedMembers.Contains(currentLoggedUser))
            {
                var currentHouseholdBankAccounts = (from account in currentHousehold.BankAccounts
                                                  select new BankAccountViewModel
                                                  {
                                                      Id = account.Id,
                                                      Name = account.Name,
                                                      Description = account.Description,
                                                      DateCreated = account.DateCreated,
                                                      DateUpdated = account.DateUpdated,
                                                      Balance = account.Balance
                                                  }).ToList();



                return Ok(currentHouseholdBankAccounts);
            }
            else
            {
                return BadRequest("You are not a member of this household");
            }
        }
    }
}
