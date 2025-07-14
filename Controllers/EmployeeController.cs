using AhmadHRManagementSystem.Data;
using AhmadHRManagementSystem.Exceptions;
using AhmadHRManagementSystem.Models;
using AhmadHRManagementSystem.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AhmadHRManagementSystem.Controllers
{
    [AuthorizeRole("Admin", "User")]
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
                var activeEmployees = employees.Where(e => e.IsActive == true).ToList();
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

        [HttpPost]
        public async Task<IActionResult> Edit(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                var designationName = await _unitOfWork.EmployeeDesignation.GetAllAsync();
                ViewBag.DesignationList = new SelectList(designationName, "Id", "DesignationName", employee.EmployeeDesignationID);
                return View(employee);
            }

            try
            {
                await _unitOfWork.Employees.UpdateAsync(employee);
                TempData["UpdateMessage"] = "Employee updated successfully!";
                return RedirectToAction("Index");
            }
            catch (CustomException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while updating the employee.";
            }

            var designationRetry = await _unitOfWork.EmployeeDesignation.GetAllAsync();
            ViewBag.DesignationList = new SelectList(designationRetry, "Id", "DesignationName", employee.EmployeeDesignationID);
            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
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

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
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

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _unitOfWork.Employees.SoftDeleteAsync(id);
                TempData["DeleteMessage"] = "Employee deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (CustomException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> RecycleBinIndex()
        {
            try
            {
                var employees = await _unitOfWork.Employees.GetAllAsync();
                var deactiveEmployees = employees.Where(e => e.IsActive == false).ToList();
                return View(deactiveEmployees);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex is CustomException customEx
                    ? customEx.Message
                    : "An unknown error occurred while fetching employees.";

                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> RestoreEmployee(int id)
        {
            try
            {
                await _unitOfWork.Employees.RestoreDeleteAsync(id);
                TempData["SuccessMessage"] = "Employee restored successfully!";
                return RedirectToAction("Index");
            }
            catch (CustomException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
