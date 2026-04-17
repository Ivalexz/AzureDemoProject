using System.ComponentModel.DataAnnotations;

namespace DemoProj.Models;

public class RequestFromModel
{
    [Required(ErrorMessage = "Enter your name")]  
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Enter your email")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Enter your message")]
    public string Message { get; set; } = string.Empty;
    [Required(ErrorMessage = "Enter topic")]
    public string Topic { get; set; } = string.Empty;
}