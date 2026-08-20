using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;
using Bookstore.Web.Areas.Admin.Models.Offers;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Areas.Admin.Controllers
{
    public class OffersController : AdminAreaControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly IReferenceDataService _referenceDataService;

        public OffersController(IOfferService offerService, IReferenceDataService referenceDataService)
        {
            _offerService = offerService;
            _referenceDataService = referenceDataService;
        }

        public async Task<IActionResult> Index(OfferFilters filters, int pageIndex = 1, int pageSize = 10)
        {
            var offers = await _offerService.GetOffersAsync(filters, pageIndex, pageSize);
            var referenceData = await _referenceDataService.GetAllReferenceDataAsync();
            return View(new OfferIndexViewModel(offers, referenceData));
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAsync(int id)
        {
            return await UpdateOfferStatus(id, OfferStatus.Approved, "The offer has been approved");
        }

        [HttpPost]
        public async Task<IActionResult> RejectAsync(int id)
        {
            return await UpdateOfferStatus(id, OfferStatus.Rejected, "The offer has been rejected");
        }

        [HttpPost]
        public async Task<IActionResult> ReceivedAsync(int id)
        {
            return await UpdateOfferStatus(id, OfferStatus.Received, "The book has been received");
        }

        [HttpPost]
        public async Task<IActionResult> PaidAsync(int id)
        {
            return await UpdateOfferStatus(id, OfferStatus.Paid, "The customer has been paid");
        }

        private async Task<IActionResult> UpdateOfferStatus(int id, OfferStatus status, string message)
        {
            var dto = new UpdateOfferStatusDto(id, status);
            await _offerService.UpdateOfferStatusAsync(dto);
            TempData["Message"] = message;
            return RedirectToAction("Index");
        }
    }
}
