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
    [RoutePrefix("api/Category")]
    public class CategoryController : ApiController
    {
        private ApplicationDbContext DbContext;

        public CategoryController()
        {
            DbContext = new ApplicationDbContext();
        }

        [HttpPost]
        [Authorize]
        [Route("CreateCategory/{id:int}")]
        public IHttpActionResult CreateCategory(int id, CategoryBindingModel model)
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
                Category newCategory;

                newCategory = new Category();
                newCategory.Name = model.Name;
                newCategory.Description = model.Description;
                newCategory.DateCreated = DateTime.Today;

                currentHousehold.HouseholdCategories.Add(newCategory);
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
        [Route("EditCategory/{id:int}/{categoryId:int}")]
        public IHttpActionResult EditCategory(int id, int categoryId)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == id);

            var currentCategory = currentHousehold.HouseholdCategories.FirstOrDefault(
              cat => cat.Id == categoryId);

            var userId = User.Identity.GetUserId();

            if (currentHousehold == null || currentCategory == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId)
            {
                var categoryModel = new EditCategoryViewModel();
                categoryModel.Name = currentCategory.Name;
                categoryModel.Description = currentCategory.Description;
                return Ok(categoryModel);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpPost]
        [Authorize]
        [Route("EditCategory/{id:int}/{categoryId:int}")]
        public IHttpActionResult EditCategory(int id, int categoryId, CategoryViewModel categoryData)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == id);

            var currentCategory = currentHousehold.HouseholdCategories.FirstOrDefault(
               cat => cat.Id == categoryId);

            var userId = User.Identity.GetUserId();

            if (currentHousehold == null || currentCategory == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId)
            {
                currentCategory.Name = categoryData.Name;
                currentCategory.Description = categoryData.Description;
                currentCategory.DateUpdated = DateTime.Today;

                DbContext.SaveChanges();
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        [HttpPost]
        [Authorize]
        [Route("DeleteCategory/{id:int}/{categoryId:int}")]
        public IHttpActionResult DeleteCategory(int id, int categoryId)
        {
            var currentHousehold = DbContext.Households.FirstOrDefault(
                house => house.Id == id);

            var currentCategory = currentHousehold.HouseholdCategories.FirstOrDefault(
               cat => cat.Id == categoryId);

            var userId = User.Identity.GetUserId();

            if (currentHousehold == null || currentCategory == null)
            {
                return NotFound();
            }

            if (currentHousehold.CreatedById == userId)
            {

                DbContext.Categories.Remove(currentCategory);
                DbContext.SaveChanges();
                return Ok("Category Deleted");
            }
            else
            {
                return BadRequest("User not owner of household");
            }

        }

        [HttpGet]
        [Authorize]
        [Route("ViewAllHouseholdCategories/{id}")]
        public IHttpActionResult ViewAllHouseholdCategories(int id)
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
                var currentHouseholdCategories = (from cat in currentHousehold.HouseholdCategories
                                                  select new CategoryViewModel
                                                  {
                                                      Id = cat.Id,
                                                      Name = cat.Name,
                                                      Description = cat.Description,
                                                      DateCreated = cat.DateCreated,
                                                      DateUpdated = cat.DateUpdated,
                                                      HouseholdId = id
                                               }).ToList();



                return Ok(currentHouseholdCategories);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
