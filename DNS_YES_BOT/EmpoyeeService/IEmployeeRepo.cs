using DNS_YES_BOT.Models;

namespace DNS_YES_BOT.EmpoyeeService
{
    public interface IEmployeeRepo
    {
        Task<Guid> GetEmployeeId(string employeeName, string employeeLastName);
        Task<Employee> GetEmployeeById(Guid employeeId);
        Task<Employee> GetEmployeeByName(string employeeName, string employeeLastName);
        Task<List<Employee>> GetEmployeesByShopName(string shopName);
        Task<List<Employee>> GetAllEmployees();
        Task<bool> AddEmployee(string employeeName, string employeeLastName);
        Task<bool> DeleteEmployee(Guid employeeId);
    }
}
