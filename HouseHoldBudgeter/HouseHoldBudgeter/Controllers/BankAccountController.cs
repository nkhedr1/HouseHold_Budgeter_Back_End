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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Authorize]
        [Route("EditBankAccount/{id:int}/{bankAccountId:int}")]
        public IHttpActionResult EditBankAccount(int id, int bankAccountId)
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
                var accountModel = new BankAccountViewModel();
                accountModel.Name = currentBankAccount.Name;
                accountModel.Description = currentBankAccount.Description;
                accountModel.Id = currentBankAccount.Id;
                accountModel.HouseholdId = currentBankAccount.HouseHoldId;
                return Ok(accountModel);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpPost]
        [Authorize]
        [Route("EditBankAccount/{id:int}/{bankAccountId:int}")]
        public IHttpActionResult EditBankAccount(int id, int bankAccountId, BankAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
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
                                                        Balance = account.Balance,
                                                        HouseholdId = id
                                                  }).ToList();



                return Ok(currentHouseholdBankAccounts);
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpGet]
        [Authorize]
        [Route("ManuallyUpdateBankAccountBalance/{id:int}/{bankAccountId:int}")]
        public IHttpActionResult ManuallyUpdateBankAccountBalance(int id, int bankAccountId)
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
                UpdateBankAccountBalance(currentBankAccount.Id);
                DbContext.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest();
            }

        }

        public void UpdateBankAccountBalance(int bankAccountId)
        {
            var currentBankAccount = DbContext.BankAccounts.FirstOrDefault(
               account => account.Id == bankAccountId);

            var activeTransactions = (from transaction in currentBankAccount.Transactions
                                      where transaction.VoidTransaction == false
                                      select transaction.Amount).ToList();

            currentBankAccount.Balance = activeTransactions.AsQueryable().Sum();
        }
    }
}
