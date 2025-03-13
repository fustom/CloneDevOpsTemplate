using System.ComponentModel.DataAnnotations;

namespace CloneDevOpsTemplate.Models;

public class LoginModel
{
    [Required(ErrorMessage = "Organization name is required.")]
    [Display(Name = "Organization name")]
    public string OrganizationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Access token is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Access token")]
    public string AccessToken { get; set; } = string.Empty;
}