using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos;

public class AuthRegister
{
    [Required] public string UserName { get; set; }

    [Required] public string Password { get; set; }

    [Required] public string Email { get; set; }
}