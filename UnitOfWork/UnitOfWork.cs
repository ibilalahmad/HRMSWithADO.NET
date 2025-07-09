using AhmadHRManagementSystem.Data;
using AhmadHRManagementSystem.Models;
using AhmadHRManagementSystem.Repository.Abstractions;
using AhmadHRManagementSystem.Repository.Repositories;

namespace AhmadHRManagementSystem.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseHelper _dbHelper;
        public IRepository<Employee> Employees { get; }
        public IRepository<EmployeeDepartment> EmployeeDepartment { get; }
        public IRepository<EmployeeDesignation> EmployeeDesignation { get; }

        public UnitOfWork(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            Employees = new EmployeeRepository(_dbHelper);
            EmployeeDepartment = new EmployeeDepartmentRepository(_dbHelper);
            EmployeeDesignation = new EmployeeDesignationRepository(_dbHelper);
        }

        public void Dispose()
        {
            // Dispose logic if needed
        }
    }

}
