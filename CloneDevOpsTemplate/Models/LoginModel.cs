using System.ComponentModel.DataAnnotations;

namespace CloneDevOpsTemplate.Models;

public class LoginModel
{
    [Required]
    public string OrganizationName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string AccessToken { get; set; } = string.Empty;
}