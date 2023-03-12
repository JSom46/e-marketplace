using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos;

public class Register
{
    [Required] public string UserName { get; set; }

    [Required] public string Password { get; set; }

    [Required] public string Email { get; set; }
}