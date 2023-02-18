using System.ComponentModel.DataAnnotations;

namespace Authentication.Dtos;

public class AuthLogin
{
    [Required] public string UserName { get; set; }

    [Required] public string Password { get; set; }
}