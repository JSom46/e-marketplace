using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos;

public class Login
{
    [Required] public string UserName { get; set; }

    [Required] public string Password { get; set; }
}