using Microsoft.AspNetCore.Mvc;

public class PaymentController : Controller
{
  
    public IActionResult Step1()
    {
        return View("PaymentStep1");
    }

  
    [HttpPost]
    public IActionResult Step2(string firstName, string lastName, string email)
    {
        ViewBag.FirstName = firstName;
        ViewBag.LastName = lastName;
        ViewBag.Email = email;
        return View("PaymentStep2");
    }

   
    [HttpPost]
    public IActionResult Confirm()
    {
        return View("Confirmation"); 
    }
}
