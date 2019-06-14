using HouseHoldBudgeter.Models;
using HouseHoldBudgeter.Models.Domain;
using HouseHoldBudgeter.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;

namespace HouseHoldBudgeter.Controllers
{
    [RoutePrefix("api/Household")]
    public class HouseholdController : ApiController
    {
        private ApplicationDbContext DbContext;

        public HouseholdController()
        {
            DbContext = new ApplicationDbContext();
        }

        [HttpPost]
        [Authorize]
        [Route("CreateHousehold")]
        public IHttpActionResult CreateHousehold(HouseholdBindingModel model)
        {
            var userId = User.Identity.GetUserId();

            var currentLoggedUser = DbContext.Users.FirstOrDefault(
            usr => usr.Id == userId);

            Household newHouseholde;

            newHouseholde = new Household();
            newHouseholde.Name = model.Name;
            newHouseholde.Description = model.Description;
            newHouseholde.CreatedById = userId;
            newHouseholde.DateCreated = DateTime.Today;
            newHouseholde.HouseholdJoinedMembers.Add(currentLoggedUser);

            DbContext.Households.Add(newHouseholde);
            newHouseholde.HouseholdJoinedMembers.Add(currentLoggedUser);
            DbContext.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("EditHousehold/{id:int}")]
        public IHttpActionResult EditHousehold(int id)
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
                var householdModel = new EditHouseholdViewModel();
                householdModel.Name = currentHousehold.Name;
                householdModel.Description = currentHousehold.Description;
                return Ok(householdModel);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpPost]
        [Authorize]
        [Route("EditHousehold/{id:int}")]
        public IHttpActionResult EditHousehold(int id, HouseholdViewModel householdData)
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
                currentHousehold.Name = householdData.Name;
                currentHousehold.Description = householdData.Description;
                currentHousehold.DateUpdated = DateTime.Today;

                DbContext.SaveChanges();
                return Ok(currentHousehold);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Authorize]
        [Route("DeleteHousehold/{id:int}")]
        public IHttpActionResult DeleteHousehold(int id)
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
                DbContext.Households.Remove(currentHousehold);
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
        [Route("InviteToHousehold/{id:int}")]
        public IHttpActionResult InviteToHousehold(int id)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == id);

            var userId = User.Identity.GetUserId();

            var currentUsers = (from user in DbContext.Users
                                select new MemberViewModel
                                {
                                    Email = user.Email,
                                    UserName = user.UserName
                                }).ToList();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (currentHousehold == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId)
            {

                return Ok(currentUsers);
            }
            else
            {
                return BadRequest();
            }

        }


        [HttpPost]
        [Authorize]
        [Route("InviteToHousehold")]
        public IHttpActionResult InviteToHousehold(InviteUser inviteData)
        {

            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == inviteData.HouseholdId);

            var currentInvitedUser = DbContext.Users.FirstOrDefault(
              usr => usr.Email == inviteData.Email);

            var userId = User.Identity.GetUserId();

            var url = $"http://localhost:50602/Household/JoinHousehold/{inviteData.HouseholdId}";

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (currentHousehold == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId)
            {
                currentHousehold.HouseholdInvitedMembers.Add(currentInvitedUser);
                DbContext.SaveChanges();
                SendEmailNotification(inviteData.Email, "Join Household", "Please visit this link to accept joining the household " + url);
                return Ok();
            }
            else
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Authorize]
        [Route("JoinHousehold/{id:int}")]
        public IHttpActionResult JoinHousehold(int id)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == id);

            var userId = User.Identity.GetUserId();

            var currentInvitedUser = DbContext.Users.FirstOrDefault(
              usr => usr.Id == userId);

            if (currentHousehold == null)
            {
                return NotFound();
            }

            if (currentHousehold.HouseholdInvitedMembers.Contains(currentInvitedUser))
            {
                currentHousehold.HouseholdJoinedMembers.Add(currentInvitedUser);
                currentHousehold.HouseholdInvitedMembers.Remove(currentInvitedUser);
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
        [Route("LeaveHousehold/{id:int}")]
        public IHttpActionResult LeaveHousehold(int id)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == id);

            var userId = User.Identity.GetUserId();

            var currentJoinedUser = DbContext.Users.FirstOrDefault(
              usr => usr.Id == userId);

            if (currentHousehold == null)
            {
                return NotFound();
            }

            if (currentHousehold.HouseholdJoinedMembers.Contains(currentJoinedUser))
            {
                currentHousehold.HouseholdJoinedMembers.Remove(currentJoinedUser);
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
        [Route("ViewHouseholdMembers/{id}")]
        public IHttpActionResult ViewHouseholdMembers(int id)
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
                var currentHouseholdMembers = (from member in currentHousehold.HouseholdJoinedMembers
                                               select new MemberViewModel
                                               {
                                                   Email = member.Email,
                                                   UserName = member.UserName
                                               }).ToList();



                return Ok(currentHouseholdMembers);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Authorize]
        [Route("ViewMyCreatedHouseholds")]
        public IHttpActionResult ViewMyCreatedHouseholds()
        {

            var userId = User.Identity.GetUserId();

            var myCreatedHouseholds = (from house in DbContext.Households
                                       where house.CreatedById == userId
                                       select new HouseholdViewModel
                                       {
                                           Id = house.Id,
                                           Name = house.Name,
                                           DateCreated = house.DateCreated,
                                           DateUpdated = house.DateUpdated
                                       }).ToList();


            return Ok(myCreatedHouseholds);

        }

        [HttpGet]
        [Authorize]
        [Route("ViewHouseholdsJoined")]
        public IHttpActionResult ViewHouseholdsJoined()
        {

            var userId = User.Identity.GetUserId();

            var currentLoggedUser = DbContext.Users.FirstOrDefault(
            usr => usr.Id == userId);


            var myJoinedHouseholds = (from usr in currentLoggedUser.JoinedHouseholds

                                      select new HouseholdViewModel
                                      {
                                          Id = usr.Id,
                                          Name = usr.Name,
                                          DateCreated = usr.DateCreated,
                                          DateUpdated = usr.DateUpdated
                                      }).ToList();


            return Ok(myJoinedHouseholds);

        }

        [HttpGet]
        [Authorize]
        [Route("ViewHouseholdDetails/{id}")]
        public IHttpActionResult ViewHouseholdDetails(int id)
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

                var currentHouseholdAccounts = (from acc in currentHousehold.BankAccounts
                                                select new BankAccountViewModel
                                                {
                                                    Id = acc.Id,
                                                    Name = acc.Name,
                                                    Balance = acc.Balance,
                                                    DateCreated = acc.DateCreated,
                                                    DateUpdated = acc.DateUpdated,
                                                    HouseholdId = acc.HouseHoldId,
                                                    Description = acc.Description
                                                }).ToList();

                var balanceList = (from acc in currentHouseholdAccounts
                                   select acc.Balance).ToList();

                var categoryList = (from cat in DbContext.Categories
                                    where cat.HouseholdId == id
                                    select new CategoryDetailsViewModel
                                    {
                                        Id = cat.Id,
                                        Name = cat.Name,
                                        Transactions = cat.Transactions,
                                        DateCreated = cat.DateCreated,
                                        DateUpdated = cat.DateUpdated,
                                        Description = cat.Description,
                                        HouseholdId = cat.HouseholdId,
                                        TotalTransactionAmounts = (from trans in cat.Transactions
                                                                   select trans.Amount).ToList().AsQueryable().Sum()
                                    }).ToList();

                var totalBalance = balanceList.AsQueryable().Sum();
                var householdDetailModel = new HouseholdDetailViewModel();
                householdDetailModel.Id = id;
                householdDetailModel.Name = currentHousehold.Name;
                householdDetailModel.DateCreated = currentHousehold.DateCreated;
                householdDetailModel.DateUpdated = currentHousehold.DateUpdated;
                householdDetailModel.TotalBalance = totalBalance;
                householdDetailModel.BankAccounts = currentHouseholdAccounts;
                householdDetailModel.HouseholdCategories = categoryList;

                return Ok(householdDetailModel);
            }
            else
            {
                return BadRequest();
            }

        }

        protected void SendEmailNotification(string email, string subject, string body)
        {

            MailAddress from = new MailAddress("nour@gmail.com");
            MailAddress to = new MailAddress(email);
            MailMessage message = new MailMessage(from, to);

            message.Subject = subject;
            message.Body = body;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.mailtrap.io", 2525);
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("e4ffc6c1f9b78a", "0e3ad862fa6817");

            try
            {
                client.Send(message);
            }
            catch
            {
                //error message?
            }
            finally
            {

            }
        }

    }
}
