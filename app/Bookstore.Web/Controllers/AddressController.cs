using Bookstore.Domain.Addresses;
using Bookstore.Domain.Customers;
using Bookstore.Web.Helpers;
using Bookstore.Web.ViewModel.Address;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Controllers
{
    public class AddressController : Controller
    {
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;

        public AddressController(IAddressService addressService, ICustomerService customerService)
        {
            _addressService = addressService;
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var addresses = await _addressService.GetAddressesAsync(User.GetSub()!);
            return View(new AddressIndexViewModel(addresses));
        }

        public IActionResult Create(string? returnUrl)
        {
            var model = new AddressCreateUpdateViewModel(returnUrl);
            return View("CreateUpdate", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddressCreateUpdateViewModel model)
        {
            if (!ModelState.IsValid) return View("CreateUpdate", model);

            var dto = new CreateAddressDto(model.AddressLine1!, model.AddressLine2, model.City!, model.State!, model.Country!, model.ZipCode!, User.GetSub()!);
            await _addressService.CreateAddressAsync(dto);
            return Redirect(model.ReturnUrl ?? "/");
        }

        public async Task<IActionResult> Update(int id, string? returnUrl)
        {
            var address = await _addressService.GetAddressAsync(User.GetSub()!, id);
            return View("CreateUpdate", new AddressCreateUpdateViewModel(address, returnUrl));
        }

        [HttpPost]
        public async Task<IActionResult> Update(AddressCreateUpdateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var dto = new UpdateAddressDto(model.Id, model.AddressLine1!, model.AddressLine2, model.City!, model.State!, model.Country!, model.ZipCode!, User.GetSub()!);
            await _addressService.UpdateAddressAsync(dto);
            return Redirect(model.ReturnUrl ?? "/");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var dto = new DeleteAddressDto(id, User.GetSub()!);
            await _addressService.DeleteAddressAsync(dto);
            this.SetNotification("Address deleted");
            return RedirectToAction(nameof(Index));
        }
    }
}
