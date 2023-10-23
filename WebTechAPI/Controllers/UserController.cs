using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using WebTechAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebTechAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    
    public class UserController : ControllerBase
    {


        /// <summary>
        /// вывод одного пользователя
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        
        [HttpGet("{id}")]
        public ActionResult<User> Get(int id)
        {
            using (UserContext context = new UserContext())
            {
                var user = context.Users.Include(c => c.Roles).Where(p => p.Id == id).ToList();
                if (user.IsNullOrEmpty()) return NotFound();
                return user.First();
            }
        }

        /// <summary>
        /// вывод пользователей с фильтровкой и сортировкой
        /// </summary>
        /// <param name="sortOrder">id, name, age, email, role</param>
        /// <param name="nameFilter">id, name, age, age_more, age_less, email, role</param>
        /// <param name="valueFilter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("Sort_Filtr_Users")]
        public ActionResult<IEnumerable<User>> Get(string? sortOrder, string? nameFilter, string? valueFilter, int? pageNumber, int? pageSize)
        {

            using (UserContext context = new UserContext())
            {

                //var users = context.Users.Include(c => c.Roles);
                var users = from u in context.Users select u;
                users = users.Include(c => c.Roles);
                try
                {
                    switch (nameFilter)
                    {
                        case "id": { int id = Convert.ToInt32(valueFilter); users = users.Where(p => p.Id == id); } break;
                        case "name": { users = users.Where(p => p.Name == valueFilter); } break;
                        case "age": { int age = Convert.ToInt32(valueFilter); users = users.Where(p => p.Age == age); } break;
                        case "age_more": { int age = Convert.ToInt32(valueFilter); users = users.Where(p => p.Age > age);} break;
                        case "age_less": { int age = Convert.ToInt32(valueFilter); users = users.Where(p => p.Age < age); } break;
                        case "email": { users = users.Where(p => p.Email == valueFilter); } break;
                        case "role": { users = users.Where(p => p.Roles.Any(r => r.RoleName == valueFilter)); } break;
                        default: { } break; //no filtr
                    }
                }
                catch { return BadRequest(); }
                switch (sortOrder)
                {
                    case "id": { users = users.OrderBy(p => p.Id); } break;
                    case "id_desc": { users = users.OrderByDescending(p => p.Id); } break;
                    case "name": { users = users.OrderBy(p => p.Name); } break;
                    case "name_desc": { users = users.OrderByDescending(p => p.Name); } break;
                    case "age": { users = users.OrderBy(p => p.Age); } break;
                    case "age_desc": { users = users.OrderByDescending(p => p.Age); } break;
                    case "email": { users = users.OrderBy(p => p.Email); } break;
                    case "email_desc": { users = users.OrderByDescending(p => p.Email); } break;
                    case "role": {
                            users = users.Select(u => new
                            {
                                User = u,
                                FirstRoleName = u.Roles.OrderBy(r => r.RoleName).First().RoleName,
                            })
                            .OrderBy(u => u.FirstRoleName)
                            .Select(u => u.User); 
                        } break;

                    case "role_desc": {
                            users = users.Select(u => new
                            {
                                User = u,
                                FirstRoleName = u.Roles.OrderByDescending(r => r.RoleName).First().RoleName,
                            })
                            .OrderBy(u => u.FirstRoleName)
                            .Select(u => u.User);
                        } break;

                    default: { users.OrderBy(p => p.Id); } break; //id
                }

                users = users.Skip((pageNumber ?? 1 - 1) * (pageSize ?? 5)).Take(pageSize ?? 5);
                
                return users.ToList();


            }

            
            
        }

        // POST api/<UserController>
        /// <summary>
        /// добавление нового пользователя
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="age"></param>
        /// <param name="email"></param>
        /// <param name="roleName">User по умолчанию, возможно несколько,через запятую без пробела</param>
        /// <returns></returns>
        [HttpPost("name={name},age={age},email={email}")]
        public ActionResult Post(int? id, string name, int age, string email, string? roleName)
        {
            using (UserContext context = new UserContext())
            {
                if(age<=0 ) return BadRequest();
                if (EmailChecker.Check(email) != true) return BadRequest();
                else
                {
                    if(!context.Users.Where(p=>p.Email == email).ToList().IsNullOrEmpty()) return Conflict();
                    if(!context.Users.Where(p=>p.Id == id).ToList().IsNullOrEmpty()) return Conflict();

                    
                    var user = new User(id ??= context.Users.Max(p=>p.Id)+1, name, age, email);

                    

                    List <Role> role = new List<Role>();
                    List<string> roleNames = new List<string>();
                    if (roleName != null && roleName != "") 
                    {
                        foreach (string s in roleName.Split(','))
                            if (s == "User" || s == "Admin" || s == "SuperAdmin" || s == "Support")
                                roleNames.Add(s);
                    }

                    if (roleNames.IsNullOrEmpty())
                        role.Add(new Role
                        {
                            UserId = user.Id,
                            RoleName = "User"

                        });
                    else
                        foreach (var r in roleNames)
                            role.Add(new Role
                            {
                                UserId = user.Id,
                                RoleName = r

                            });


                    context.Users.AddRange(user);
                    context.SaveChanges();
                    context.Roles.AddRange(role);
                    context.SaveChanges();
                    return Accepted();
                }

                
            }
            
        }

        /// <summary>
        /// добавление роли пользователю
        /// </summary>
        /// <param name="id"></param>
        /// <param name="roleName">возможно несколько,через запятую без пробела</param>
        /// <returns></returns>
        [HttpPost("id={id},role={roleName}")]
        public IActionResult AddRolePost(int id, string roleName)
        {
            using (UserContext context = new UserContext())
            {
                var users = context.Users.Where(p => p.Id == id).ToList();
                if (!users.IsNullOrEmpty())
                { 
                    List<Role> role = new List<Role>();
                    List<string> roleNames = new List<string>();
                    if (roleName != null || roleName != "")
                    {
                        foreach (string s in roleName.Split(','))
                            if (s == "User" || s == "Admin" || s == "SuperAdmin" || s == "Support")
                                roleNames.Add(s);
                    }

                    if (roleNames.IsNullOrEmpty())
                        return BadRequest();
                    else
                    {
                        foreach (var r in roleNames)
                            if (context.Roles.Where(p => p.RoleName == r).Where(p => p.UserId == id).ToList().IsNullOrEmpty())
                                role.Add(new Role
                                {
                                    UserId = id,
                                    RoleName = r

                                });
                            else return Conflict();
                        
                        context.Roles.AddRange(role);
                        context.SaveChanges();
                        return Accepted();
                    }
                } else return BadRequest();

            }

        }
        /// <summary>
        /// изменение данных пользователя
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newName"></param>
        /// <param name="newAge"></param>
        /// <param name="newEmail"></param>
        /// <returns></returns>
        [HttpPut("id={id}")]
        public IActionResult Put(int id, string? newName, int? newAge, string? newEmail)
        {
            using (UserContext context = new UserContext())
            {
                var users= context.Users.Where(p => p.Id == id).ToList();
                if (!users.IsNullOrEmpty())
                {
                    User u = users.First();
                    User user = new User
                    {
                        Id = id,
                        Name = newName ?? u.Name,
                        Age = newAge ?? u.Age,
                        Email = newEmail ?? u.Email
                    };
                    context.Users.Update(user);
                    context.SaveChanges();
                    return Accepted();
                }else  return BadRequest();
            }
               
        }


        /// <summary>
        /// удаление пользователя
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            using (UserContext context = new UserContext())
            {

                context.Roles.RemoveRange(context.Roles.Where(p=>p.UserId==id));
                context.SaveChanges();
                context.Users.RemoveRange(context.Users.Where(p=>p.Id==id));
                context.SaveChanges();
            }
        }
    }
}
