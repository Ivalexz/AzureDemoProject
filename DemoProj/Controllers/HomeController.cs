using Microsoft.AspNetCore.Mvc;
using DemoProj.Models;
using DemoProj.Services;

namespace DemoProj.Controllers;

public class HomeController : Controller
{
    private readonly AzureTableService _azureTableService;

    public HomeController(AzureTableService azureTableService)
    {
        _azureTableService = azureTableService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Requests = await _azureTableService.GetAllRequest();
        return View(new RequestFromModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(RequestFromModel obj)
    {
        if (ModelState.IsValid)
        {
            await _azureTableService.AddRequest(obj);
            return RedirectToAction("Index");
        }
        ViewBag.Requests = await _azureTableService.GetAllRequest();
        return View(obj);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string rowKey)
    {
        var req = await _azureTableService.GetRequestByRowKey(rowKey);

        if (req == null)
        {
            return NotFound();
        }

        var obj = new RequestFromModel
        {
            Name = req.Name,
            Email = req.Email,
            Message = req.Message,
            Topic = req.Topic
        };

        ViewBag.RowKey = rowKey;
        return View(obj);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string rowKey, RequestFromModel obj)
    {
        if (ModelState.IsValid)
        {
            await _azureTableService.UpdateRequest(rowKey, obj);
            return RedirectToAction("Index");
        }
        ViewBag.Requests = await _azureTableService.GetAllRequest();
        return View(obj);
    }
}