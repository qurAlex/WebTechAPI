namespace WebTechAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();

        public User() { }
        public User(int id, string name, int age, string email)
        {
            Id = id;
            Name = name;
            Age = age;
            Email = email;
        }
    }
}
