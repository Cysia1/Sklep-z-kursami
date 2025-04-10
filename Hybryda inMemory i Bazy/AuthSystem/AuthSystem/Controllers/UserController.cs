using Microsoft.AspNetCore.Mvc;

public class UserController : Controller
{
    public IActionResult UserPanel()
    {
        return View("~/Views/Account/UserPanel.cshtml");
    }
}
