using HomeEnergyApi.Models;

namespace HomeEnergyApi.Dtos
{
    public class UserDtoV2
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public FullAddress Address { get; set; }
    }
}