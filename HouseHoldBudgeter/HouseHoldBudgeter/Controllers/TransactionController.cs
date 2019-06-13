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
    [RoutePrefix("api/Transaction")]
    public class TransactionController : ApiController
    {
        private ApplicationDbContext DbContext;

        public TransactionController()
        {
            DbContext = new ApplicationDbContext();
        }

        [HttpGet]
        [Authorize]
        [Route("CreateTransaction/{id:int}/{bankAccountId:int}")]
        public IHttpActionResult CreateTransaction(int id, int bankAccountId)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == id);

            var currentBankAccount = currentHousehold.BankAccounts.FirstOrDefault(
               cat => cat.Id == bankAccountId);

            var currentHouseholdCategories = (from cat in currentHousehold.HouseholdCategories
                                              select new CategoryViewModel
                                                {
                                                    Id = cat.Id,
                                                    Name = cat.Name,
                                                    Description = cat.Description,
                                                    HouseholdId = cat.HouseholdId,
                                                    DateCreated = cat.DateCreated,
                                                    DateUpdated = cat.DateUpdated
                                                }).ToList();

            

            var userId = User.Identity.GetUserId();

            if (currentBankAccount == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId)
            {
                CreateTransactionViewModel newTransactionModel;

                newTransactionModel = new CreateTransactionViewModel();
                newTransactionModel.Categories = currentHouseholdCategories;
                newTransactionModel.HouseholdId = id;
                return Ok(newTransactionModel);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpPost]
        [Authorize]
        [Route("CreateTransaction/{id:int}/{bankAccountId:int}")]
        public IHttpActionResult CreateTransaction(int id, int bankAccountId, CreateTransactionViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentHousehold = DbContext.Households.FirstOrDefault(
            house => house.Id == id);

            var currentBankAccount = currentHousehold.BankAccounts.FirstOrDefault(
               account => account.Id == bankAccountId);

            var currentCategory = currentHousehold.HouseholdCategories.FirstOrDefault(
              cat => cat.Id == model.CategoryId);

            var userId = User.Identity.GetUserId();

            var currentLoggedUser = DbContext.Users.FirstOrDefault(
               user => user.Id == userId);

            if (currentHousehold == null || currentBankAccount == null || currentCategory == null)
            {
                return NotFound();
            }

            if (currentHousehold.HouseholdJoinedMembers.Contains(currentLoggedUser))
            {
                Transaction newTransaction;

                newTransaction = new Transaction();
                newTransaction.Title = model.Title;
                newTransaction.Description = model.Description;
                newTransaction.DateCreated = DateTime.Today;
                newTransaction.Date = model.Date;
                newTransaction.BankAccountId = bankAccountId;
                newTransaction.CategoryId = model.CategoryId;
                newTransaction.Amount = model.Amount;
                newTransaction.CreatedById = userId;

                currentBankAccount.Transactions.Add(newTransaction);
                UpdateBankAccountBalance(currentBankAccount.Id);
                DbContext.SaveChanges();

                return Ok();
            }
            else
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Authorize]
        [Route("EditTransaction/{householdId:int}/{id:int}")]
        public IHttpActionResult EditTransaction(int id, int householdId)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == householdId);

            var currentTransaction = DbContext.Transactions.FirstOrDefault(
               trans => trans.Id == id);

            var currentHouseholdCategories = (from cat in currentHousehold.HouseholdCategories
                                              select new CategoryViewModel
                                              {
                                                  Id = cat.Id,
                                                  Name = cat.Name,
                                                  Description = cat.Description,
                                                  HouseholdId = cat.HouseholdId,
                                                  DateCreated = cat.DateCreated,
                                                  DateUpdated = cat.DateUpdated
                                              }).ToList();

            var userId = User.Identity.GetUserId();

            if (currentTransaction == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId)
            {
                var transactionModel = new TransactionViewModel();
                transactionModel.Title = currentTransaction.Title;
                transactionModel.Description = currentTransaction.Description;
                transactionModel.Id = currentTransaction.Id;
                transactionModel.Amount = currentTransaction.Amount;
                transactionModel.Date = currentTransaction.Date;
                transactionModel.CategoryId = currentTransaction.CategoryId;
                transactionModel.Categories = currentHouseholdCategories;
                return Ok(transactionModel);
            }
            else
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Authorize]
        [Route("EditTransaction/{id:int}/{transactionId:int}")]
        public IHttpActionResult EditTransaction(int id, int transactionId, TransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentTransaction = DbContext.Transactions.FirstOrDefault(
                    trans => trans.Id == transactionId);

            var currentHousehold = DbContext.Households.FirstOrDefault(
                    house => house.Id == id);

            var currentBankAccount = DbContext.BankAccounts.FirstOrDefault(
                    account => account.Id == currentTransaction.BankAccountId);

            var userId = User.Identity.GetUserId();

            var currentTransactionCreator = DbContext.Users.FirstOrDefault(
                    user => user.Id == userId && currentTransaction.CreatedById == userId);


            if (currentTransaction == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId || currentTransactionCreator != null)
            {

                currentTransaction.Title = model.Title;
                currentTransaction.Description = model.Description;
                currentTransaction.DateUpdated = DateTime.Today;
                currentTransaction.Date = model.Date;
                currentTransaction.Amount = model.Amount;
                currentTransaction.CategoryId = model.CategoryId;

                UpdateBankAccountBalance(currentBankAccount.Id);
                DbContext.SaveChanges();
                return Ok();

            }
            else
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Authorize]
        [Route("DeleteTransaction/{id:int}")]
        public IHttpActionResult DeleteTransaction(int id)
        {
            var currentTransaction = DbContext.Transactions.FirstOrDefault(
                    trans => trans.Id == id);

            var currentCategory = DbContext.Categories.FirstOrDefault(
                    cat => cat.Id == currentTransaction.CategoryId);

            var currentHousehold = DbContext.Households.FirstOrDefault(
                    house => house.Id == currentCategory.HouseholdId);

            var currentBankAccount = DbContext.BankAccounts.FirstOrDefault(
                    account => account.Id == currentTransaction.BankAccountId);

            var userId = User.Identity.GetUserId();

            var currentTransactionCreator = DbContext.Users.FirstOrDefault(
                    user => user.Id == userId && currentTransaction.CreatedById == userId);

            if (currentHousehold == null || currentBankAccount == null || currentCategory == null || currentTransaction == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId || currentTransactionCreator != null)
            {
                DbContext.Transactions.Remove(currentTransaction);
                UpdateBankAccountBalance(currentBankAccount.Id);
                DbContext.SaveChanges();
                return Ok("Transaction Deleted");
            }
            else
            {
                return BadRequest("User not owner of household or not creator of transaction");
            }

        }

        [HttpPost]
        [Authorize]
        [Route("VoidTransaction/{id:int}")]
        public IHttpActionResult VoidTransaction(int id)
        {
            var currentTransaction = DbContext.Transactions.FirstOrDefault(
                    trans => trans.Id == id);

            var currentCategory = DbContext.Categories.FirstOrDefault(
                    cat => cat.Id == currentTransaction.CategoryId);

            var currentHousehold = DbContext.Households.FirstOrDefault(
                    house => house.Id == currentCategory.HouseholdId);

            var currentBankAccount = DbContext.BankAccounts.FirstOrDefault(
                    account => account.Id == currentTransaction.BankAccountId);

            var userId = User.Identity.GetUserId();

            var currentTransactionCreator = DbContext.Users.FirstOrDefault(
                    user => user.Id == userId && currentTransaction.CreatedById == userId);

            if (currentHousehold.CreatedById == userId || currentTransactionCreator != null)
            {
                if (currentTransaction.VoidTransaction)
                {
                    return BadRequest("Account already voided");
                }

                currentTransaction.VoidTransaction = true;
                UpdateBankAccountBalance(currentBankAccount.Id);
                DbContext.SaveChanges();
                return Ok("Transaction Voided");
            }
            else
            {
                return BadRequest("User not owner of household or not creator of transaction");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("ViewBankAccountTransactions/{id}")]
        public IHttpActionResult ViewBankAccountTransactions(int id)
        {
            var currentBankAccount = DbContext.BankAccounts.FirstOrDefault(
                     account => account.Id == id);

            var currentHousehold = DbContext.Households.FirstOrDefault(
                    house => house.Id == currentBankAccount.HouseHoldId);

            var userId = User.Identity.GetUserId();

            var currentLoggedUser = DbContext.Users.FirstOrDefault(
            usr => usr.Id == userId);

            if (currentHousehold == null || currentBankAccount == null)
            {
                return NotFound();
            }

            if (currentHousehold.HouseholdJoinedMembers.Contains(currentLoggedUser))
            {
                var currentHouseholdTransactions = (from trans in currentBankAccount.Transactions
                                               select new TransactionViewModel
                                               {
                                                   Id = trans.Id,
                                                   Title = trans.Title,
                                                   Description = trans.Description,
                                                   Date = trans.Date,
                                                   DateCreated = trans.DateCreated,
                                                   DateUpdated = trans.DateUpdated,
                                                   Amount = trans.Amount,
                                                   VoidTransaction = trans.VoidTransaction,
                                                   HouseholdId = currentHousehold.Id
                                               }).ToList();



                return Ok(currentHouseholdTransactions);
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
