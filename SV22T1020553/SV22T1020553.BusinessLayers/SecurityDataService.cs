using SV22T1020553.BusinessLayers;
using SV22T1020553.DataLayers.Interfaces;
using SV22T1020553.DataLayers.SQLServer;
using SV22T1020553.Models.Security;


namespace SV22T1020553.BusinessLayers
{
    public static class SecurityDataService
    {
        private static readonly IUserAccountRepository employeeAccountDB;
        private static readonly IUserAccountRepository customerAccountDB;

        static SecurityDataService()
        {
            employeeAccountDB = new EmployeeAccountRepository(Configuration.ConnectionString);
            customerAccountDB = new CustomerAccountRepository(Configuration.ConnectionString);
        }

        public static async Task<UserAccount?> AuthenticateEmployeeAsync(string userName, string password)
        {
            return await employeeAccountDB.AuthenticateAsync(userName, password);
        }

        public static async Task<bool> ChangeEmployeePasswordAsync(string userName, string password)
        {
            return await employeeAccountDB.ChangePasswordAsync(userName, password);
        }

        public static async Task<UserAccount?> AuthenticateCustomerAsync(string userName, string password)
        {
            return await customerAccountDB.AuthenticateAsync(userName, password);
        }

        public static async Task<bool> ChangeCustomerPasswordAsync(string userName, string password)
        {
            return await customerAccountDB.ChangePasswordAsync(userName, password);
        }
        public static async Task<bool> CheckEmailInUseAsync(string email)
        {
            return await customerAccountDB.CheckEmailAsync(email);
        }

        public static async Task<int> RegisterCustomerAsync(string displayName, string email, string password)
        {
            return await customerAccountDB.RegisterAsync(displayName, email, password);
        }
    }
}
