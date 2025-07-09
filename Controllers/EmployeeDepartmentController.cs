using AhmadHRManagementSystem.Exceptions;
using AhmadHRManagementSystem.Models;
using AhmadHRManagementSystem.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AhmadHRManagementSystem.Controllers
{
    public class EmployeeDepartmentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public EmployeeDepartmentController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var employeeDept = await _unitOfWork.EmployeeDepartment.GetAllAsync();

                return View(employeeDept);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex is CustomException customEx
                    ? customEx.Message
                    : "An unknown error occurred while fetching employees Departments.";

                return RedirectToAction("Error", "Home");
            }
        }
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeDepartment employeeDepartment)
        {
            if (!ModelState.IsValid)
            {
                return View(employeeDepartment);
            }

            try
            {
                await _unitOfWork.EmployeeDepartment.AddAsync(employeeDepartment);
                TempData["SuccessMessage"] = "Employee Department added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex is CustomException customEx
                    ? customEx.Message
                    : "An unexpected error occurred while adding the employee department.";
            }

            return View(employeeDepartment);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employeeDepartment = await _unitOfWork.EmployeeDepartment.GetByIdAsync(id);

            if (employeeDepartment == null)
            {
                return NotFound();
            }

            return View(employeeDepartment);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeDepartment employeeDepartment)
        {
            if (!ModelState.IsValid)
            {
                return View(employeeDepartment);
            }

            try
            {
                await _unitOfWork.EmployeeDepartment.UpdateAsync(employeeDepartment);
                TempData["UpdateMessage"] = "Employee Department updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex is CustomException customEx
                    ? customEx.Message
                    : "An unexpected error occurred while updating the employee department.";
            }

            return View(employeeDepartment);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var employeeDepartment = await _unitOfWork.EmployeeDepartment.GetByIdAsync(id);

            if (employeeDepartment == null)
            {
                return NotFound();
            }

            return View(employeeDepartment);
        }


        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("AHRMS_CS");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM EmployeeDepartment WHERE Id = @Id", connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            TempData["SuccessMessage"] = "Department deleted successfully.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Department not found or already deleted.";
                        }
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the department.";
                return RedirectToAction("Index");
            }
        }
    }
}
