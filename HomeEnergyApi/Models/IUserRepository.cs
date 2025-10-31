namespace HomeEnergyApi.Models
{
    public interface IUserRepository : IWriteRepository<int, User>
    {
        User FindByUsername(string username);
        bool UserIsAdmin(string role)
        {
            return role == "Admin";
        }
    }
}