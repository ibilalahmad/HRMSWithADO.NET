using AhmadHRManagementSystem.Data;
using AhmadHRManagementSystem.Exceptions;
using AhmadHRManagementSystem.Models;
using AhmadHRManagementSystem.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace AhmadHRManagementSystem.Controllers
{
    [AuthorizeRole("Admin")]
    public class EmployeeDesignationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public EmployeeDesignationController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var employeeDesig = await _unitOfWork.EmployeeDesignation.GetAllAsync();

                return View(employeeDesig);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex is CustomException customEx
                    ? customEx.Message
                    : "An unknown error occurred while fetching employees Designations.";

                return RedirectToAction("Error", "Home");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var departments = await _unitOfWork.EmployeeDepartment.GetAllAsync();

            ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeDesignation designation)
        {
            ModelState.Remove("DepartmentName");
            if (!ModelState.IsValid)
            {
                var departments = await _unitOfWork.EmployeeDepartment.GetAllAsync();
                ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName");
                return View(designation);
            }

            try
            {
                await _unitOfWork.EmployeeDesignation.AddAsync(designation);
                TempData["SuccessMessage"] = "Employee designation added successfully.";
                return RedirectToAction("Index");
            }
            catch (CustomException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var employeeDesignation = await _unitOfWork.EmployeeDesignation.GetByIdAsync(id);

                if (employeeDesignation == null)
                {
                    return NotFound();
                }

                var departments = await _unitOfWork.EmployeeDepartment.GetAllAsync();
                ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName", employeeDesignation.EmployeeDepartmentID);

                return View(employeeDesignation);
            }
            catch (CustomException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeDesignation employeeDesignation)
        {
            ModelState.Remove("DepartmentName");

            if (!ModelState.IsValid)
            {
                var departments = await _unitOfWork.EmployeeDepartment.GetAllAsync();
                ViewBag.Departments = new SelectList(departments, "Id", "DepartmentName", employeeDesignation.EmployeeDepartmentID);
                return View(employeeDesignation);
            }

            try
            {
                await _unitOfWork.EmployeeDesignation.UpdateAsync(employeeDesignation);
                TempData["UpdateMessage"] = "Employee Designation updated successfully!";
                return RedirectToAction("Index");
            }
            catch (CustomException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while updating the employee designation.";
            }

            var departmentsRetry = await _unitOfWork.EmployeeDepartment.GetAllAsync();
            ViewBag.Departments = new SelectList(departmentsRetry, "Id", "DepartmentName", employeeDesignation.EmployeeDepartmentID);
            return View(employeeDesignation);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var employeeDesignation = await _unitOfWork.EmployeeDesignation.GetByIdAsync(id);

                if (employeeDesignation == null)
                {
                    return NotFound();
                }

                return View(employeeDesignation);
            }
            catch (CustomException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid designation ID.";
                return RedirectToAction("Index");
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("AHRMS_CS");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM EmployeeDesignation WHERE Id = @Id", connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            TempData["DeleteMessage"] = "Designation deleted successfully.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Designation not found or already deleted.";
                        }
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the designation.";
                return RedirectToAction("Index");
            }
        }
    }
}
