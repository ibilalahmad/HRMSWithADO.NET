using AhmadHRManagementSystem.Exceptions;
using AhmadHRManagementSystem.Models;
using AhmadHRManagementSystem.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AhmadHRManagementSystem.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var employees = await _unitOfWork.Employees.GetAllAsync();
                var activeEmployees = employees.Where(e => e.IsActive = true).ToList();

                return View(activeEmployees);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex is CustomException customEx
                    ? customEx.Message
                    : "An unknown error occurred while fetching employees.";

                return RedirectToAction("Error", "Home");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var designationName = await _unitOfWork.EmployeeDesignation.GetAllAsync();
            ViewBag.DesignationList = new SelectList(designationName, "Id", "DesignationName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                var designationName = await _unitOfWork.EmployeeDesignation.GetAllAsync();
                ViewBag.DesignationName = new SelectList(designationName, "Id", "DesignationName");

                return View(employee);
            }

            try
            {
                await _unitOfWork.Employees.AddAsync(employee);
                TempData["SuccessMessage"] = "Employee added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex is CustomException customEx
                    ? customEx.Message
                    : "An unexpected error occurred while adding the employee.";
            }

            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);

                if (employee == null)
                {
                    return NotFound();
                }

                var designationName = await _unitOfWork.EmployeeDesignation.GetAllAsync();
                ViewBag.DesignationList = new SelectList(designationName, "Id", "DesignationName", employee.EmployeeDesignationID);

                return View(employee);
            }
            catch (CustomException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
        //[HttpGet]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var employee = await _employeeRepository.GetEmployeeByIdAsync(id); // Fetch only required employee

        //    if (employee == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(employee);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Edit(Employee employee)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(employee);
        //    }

        //    bool isUpdated = await _employeeRepository.UpdateEmployeeAsync(employee);

        //    if (!isUpdated)
        //    {
        //        ModelState.AddModelError("", "Failed to update employee. Please try again.");
        //        return View(employee);
        //    }

        //    return RedirectToAction("Index");
        //}

        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                return View(employee);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
