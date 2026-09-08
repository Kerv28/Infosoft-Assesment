using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace BVSWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly BvsService _bvsService;

        public IndexModel(BvsService bvsService)
        {
            _bvsService = bvsService;
        }

        public List<Customer> Customers { get; set; } = new();
        public List<Video> Videos { get; set; } = new();
        public List<InventoryReportItem> InventoryReport { get; set; } = new();
        public List<CustomerRentalReportItem> CustomerReport { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        public void OnGet()
        {
            LoadData();
        }

        public IActionResult OnPostAddCustomer(string name, string contact)
        {
            _bvsService.AddCustomer(name, contact);
            Message = "Customer added successfully!";
            return RedirectToPage();
        }

        public IActionResult OnPostAddVideo(string title, string category, int days, int quantity)
        {
            _bvsService.AddVideo(title, category, days, quantity);
            Message = "Video added successfully!";
            return RedirectToPage();
        }

        public IActionResult OnPostDeleteVideo(int videoId)
        {
            _bvsService.DeleteVideo(videoId);
            Message = "Video deleted!";
            return RedirectToPage();
        }

        public IActionResult OnPostRent(int customerId, int videoId, int days)
        {
            _bvsService.RentVideo(customerId, videoId, days);
            Message = "Rental processed successfully!";
            return RedirectToPage();
        }

        public IActionResult OnPostReturn(int rentalId)
        {
            double penalty = _bvsService.ReturnVideo(rentalId);
            if (penalty < 0) Message = "Rental ID not found.";
            else Message = $"Video returned! Overdue Penalty: ₱{penalty:F2}";
            return RedirectToPage();
        }

        private void LoadData()
        {
            Customers = _bvsService.GetCustomers();
            Videos = _bvsService.GetVideos();
            InventoryReport = _bvsService.GetInventoryReport();
            CustomerReport = _bvsService.GetCustomerRentalsReport();
        }
    }
}