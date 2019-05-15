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

            Household newHouseholde;

            newHouseholde = new Household();
            newHouseholde.Name = model.Name;
            newHouseholde.Description = model.Description;
            newHouseholde.CreatedById = userId;
            newHouseholde.DateCreated = DateTime.Today;

            DbContext.Households.Add(newHouseholde);
            DbContext.SaveChanges();



            var householdModel = new HouseholdViewModel();
            householdModel.Name = newHouseholde.Name;
            householdModel.Description = newHouseholde.Description;
            householdModel.DateCreated = newHouseholde.DateCreated;

            //var url = Url.Link("RetrieveCust",
            //   new { id = newCustomer.Id });

            //return Created(url, customerModel);

            return Ok();
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
                return Ok("Household Edited");
            }
            else
            {
                return NotFound();
            }
            
        }

        [HttpPost]
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
                return Ok("Household Deleted");
            }
            else
            {
                return BadRequest("User not owner of household");
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
                return Ok("Member invited to Household");
            }
            else
            {
                return BadRequest("User not owner of household");
            }

        }

        [HttpPost]
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
                return Ok("You are now a member of the household");
            }
            else
            {
                return BadRequest("You were not invited for this household");
            }

        }

        [HttpPost]
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
                return Ok("You are no longer a member of this household");
            }
            else
            {
                return BadRequest("You are not a member of this household");
            }

        }
    }
}
