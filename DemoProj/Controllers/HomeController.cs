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
    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Search = search;
        ViewBag.Requests = await _azureTableService.GetFilteredRequests(search);
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
        ViewBag.Requests = await _azureTableService.GetFilteredRequests(null);
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
            Topic = req.Topic,
            Phone = req.Phone,
            Category = req.Category,
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
    
    [HttpPost]
    public async Task<IActionResult> Delete(string rowKey)
    {
        await _azureTableService.DeleteRequest(rowKey);
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    public async Task<IActionResult> Close(string rowKey)
    {
        await _azureTableService.CloseRequest(rowKey);
        return RedirectToAction("Index");
    }
}