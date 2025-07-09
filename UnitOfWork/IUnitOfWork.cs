using AhmadHRManagementSystem.Models;
using AhmadHRManagementSystem.Repository.Abstractions;

namespace AhmadHRManagementSystem.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Employee> Employees { get; }
        IRepository<EmployeeDepartment> EmployeeDepartment { get; }
        IRepository<EmployeeDesignation> EmployeeDesignation { get; }
    }
}
