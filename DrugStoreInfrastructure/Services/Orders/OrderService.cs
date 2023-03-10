using AutoMapper;
using DrugStore.Data;
using DrugStore.SignalRHub;
using DrugStoreCore.Dto;
using DrugStoreCore.Enums;
using DrugStoreCore.ViewModel;
using DrugStoreData.Models;
using DrugStoreInfrastructure.PaginationHelpers;
using DrugStoreInfrastructure.Services.Drugs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreInfrastructure.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly DrugStoreDbContext _db;
        private readonly IMapper _mapper;
        private readonly IDrugService _drugService;
        private readonly UserManager<Pharmacy> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private ResponseViewModel<DrugDetailsViewModel> orderDetails;
        private string ResultCheckQuantity = string.Empty;
        private int PharmacyOrderId = 0;
        public OrderService(DrugStoreDbContext db, IMapper mapper, IDrugService drugService,
                            UserManager<Pharmacy> userManager, IHubContext<ChatHub> hubContext)
        {
            _db = db;
            _mapper = mapper;
            _drugService = drugService;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // Get Current Orders To Any Pharmacy
        public async Task<ResponseViewModel<ResponseDto>> PharmacyCurrentOrders
                             (PagingWithOutQueryDto pagination, string PharmacyId)
        {
            var paginationWithQuery = new PagingWithQueryDto()
            {
                Page = pagination.Page,
                PerPage = pagination.PerPage
            };
            return await PharmacyCurrentAndArchivedOrders
            (PharmacyId, paginationWithQuery, Status.Pending, "pending Orders", "Not Found Any pending Orders");
        }

        // Get Archived Odrders To Any Pharmacy
        public async Task<ResponseViewModel<ResponseDto>> PharmacyArchivedOrders
                            (PagingWithOutQueryDto pagination, string PharmacyId)
        {
            var paginationWithQuery = new PagingWithQueryDto()
            {
                Page = pagination.Page,
                PerPage = pagination.PerPage
            };
            return await PharmacyCurrentAndArchivedOrders
            (PharmacyId, paginationWithQuery, Status.Completed, "Archived Orders", "Not Found Any Archived Orders");
        }

        //Create Order
        public async Task<ResponseViewModel<List<OrderViewModel>>> CreateOrder(List<CreateOrderDto> OrderDto, string PharmacyId)
        {
            //Call Create Method 
            var createResult = await CreateOrderGeneric(OrderDto, PharmacyId, 0);

            //Check if All Orders Quantities UnAvaialable
            if (createResult.Count == 0)
            {
                return new ResponseViewModel<List<OrderViewModel>>
                (false, "Quantity To All Orders Not available in store!", null);
            }


            //Add Notification To Admin
            //Realtime Notification Using SingnalR
            var pharmacyName = await _db.Users.Where(phar => phar.Id == PharmacyId)
                                     .Select(phar => phar.PharmacyName).SingleOrDefaultAsync();
            var orderCreateAt = await _db.PharmacysOrders.Where(ord => ord.Id == PharmacyOrderId)
                                     .Select(ord => ord.CreatedAt).SingleOrDefaultAsync();
            var adminNotification = new AdminNotifications
            {
                PharmacyName = pharmacyName,
                PharmacyOrdersId = PharmacyOrderId,
                OrderCreateAt = orderCreateAt.ToString("MM/dd/yyyy"),
                TimeOrderCreateAt = orderCreateAt.ToString("hh:mm tt")
            };
            //Get Admin Id
            var adminId = await _db.Users.Where(user => user.PharmacyRole == "Admin")
                .Select(phar => phar.Id).SingleOrDefaultAsync();


            //Call ChatHub To Push Notification To Admin
            await _hubContext.Clients.User(adminId)
                .SendAsync("AdminReceiveNotifications", adminNotification);



            //Store Notification In DB
            var notification = new Notification
            {
                PharmacyId = PharmacyId,
                PharmacyOrdersId = PharmacyOrderId,
                NotificationStatus = NotificationStatus.Created
            };
            await _db.Notifications.AddAsync(notification);
            await _db.SaveChangesAsync();






            //Create All Orders Succeeded
            if (ResultCheckQuantity.Length == 0)
            {
                return new ResponseViewModel<List<OrderViewModel>>(true, "Created Succeeded", createResult);
            }
            //Create All Orders But Some Orders Not Added Because Quantity Unavailable
            else
            {
                return new ResponseViewModel<List<OrderViewModel>>
                    (true, "Created Succeeded " + ResultCheckQuantity, createResult);
            }
        }

        //Get Odrers To Update (Return Pending Orders)
        public async Task<ResponseViewModel<List<OrderViewModel>>> UpdateOrderByPharmacy(int OrderId, string PharmacyId)
        {
            //return await ReOrderUpdateOrderByPharmacy(OrderId, PharmacyId, Status.Pending);
            var ordersDetailsList = await _db.OrdersDetails
                                    .Include(drug => drug.Drug)
                                    .Include(phar => phar.PharmacyOrders)
                                    .Where(ord => ord.PharmacyOrdersId == OrderId
                                                && ord.Status == Status.Pending
                                                && ord.PharmacyOrders.PharmacyId == PharmacyId).ToListAsync();
            if (ordersDetailsList.Count == 0)
            {
                return new ResponseViewModel<List<OrderViewModel>>(false, "Not Found Orders", null);
            }
            var orders = _mapper.Map<List<OrderViewModel>>(ordersDetailsList);
            return new ResponseViewModel<List<OrderViewModel>>(true, "Orders Is Found", orders);
        }

        //Get Orders To Reorder (Return Completes & Canceled Orders)
        public async Task<ResponseViewModel<List<OrderViewModel>>> PharmacyReorder(int OrderId, string PharmacyId)
        {
            //return await ReOrderUpdateOrderByPharmacy(OrderId, PharmacyId, Status.Completed);
            var ordersDetailsList = await _db.OrdersDetails
                                    .Include(drug => drug.Drug)
                                    .Include(phar => phar.PharmacyOrders)
                                    .Where(ord => ord.PharmacyOrdersId == OrderId
                                                && ord.Status != Status.Pending
                                                && ord.PharmacyOrders.PharmacyId == PharmacyId).ToListAsync();
            if (ordersDetailsList.Count == 0)
            {
                return new ResponseViewModel<List<OrderViewModel>>(false, "Not Found Orders", null);
            }
            //Get Drug SellPrice From DB Not Order Datails To Read Last Update To SellPrice
            foreach (var order in ordersDetailsList)
            {
                order.UnitPrice = await _db.Drugs.Where(drug => drug.Id == order.DrugId)
                                        .Select(drug => drug.SellPrice??0).SingleOrDefaultAsync();
            }

            var orders = _mapper.Map<List<OrderViewModel>>(ordersDetailsList);
            return new ResponseViewModel<List<OrderViewModel>>(true, "Orders Is Found", orders);
        }

        //update Odrer
        public async Task<ResponseViewModel<ValidAndInvalidOrdersViewModel>> UpdateOrderByPharmacy(List<UpdateOrderDto> OrdersDto, string PharmacyId)
        {
            //Return Variable ResultCheckQuantity To Empty String(Default);
            ResultCheckQuantity = string.Empty;
            var flagCreateResult = false;
            var flagResultCheckQuantity = false;
            var invalidOrders = new List<OrderDetails>();
            var validOrders = new List<OrderDetails>();
            var OrdersId = new List<int>();
            var NewOrders = new List<CreateOrderDto>();
            float diffOldNewTotalPrice = 0;
            //Get Orders Id And Store In List
            foreach (var ord in OrdersDto)
            {
                //Check New Orders
                if (ord.Id < 1)
                {
                    NewOrders.Add(new CreateOrderDto { DrugId = ord.DrugId, Quantity = ord.Quantity });
                }
                //Old Orders
                else
                {
                    OrdersId.Add(ord.Id);
                }
            }
            var ordersDetailsList = await _db.OrdersDetails.Include(drug => drug.Drug)
                                    .Include(ord => ord.PharmacyOrders)
                                    .Where(order => OrdersId.Contains(order.Id)
                                           && order.PharmacyOrders.PharmacyId == PharmacyId).ToListAsync();
            if (ordersDetailsList.Count == 0)
            {
                return new ResponseViewModel<ValidAndInvalidOrdersViewModel>
                     (false, "Not Found Orders", null);
            }
            foreach (var order in ordersDetailsList)
            {
                //Check If Drug Become NonActive Denied Update 
                if (order.Drug.DrugStatus == DrugStatus.NonActive)
                {
                    validOrders.Add(order);
                    continue;
                }

                //Check If Order Status IsHanging
                if (order.IsHanging)
                {
                    validOrders.Add(order);
                    continue;
                }

                //Get Single Drug By Id 
                var currentOrderDto = OrdersDto.SingleOrDefault(drug => drug.DrugId == order.DrugId);
                var drug = await _db.Drugs.FindAsync(currentOrderDto.DrugId);
                //Check If Drug Not Found, Make Drug Equal Old Drug
                if (drug is null)
                {
                    drug = order.Drug;
                }
                // Get Difference Between Old Quantity And New Quantity
                var quantityDifference = currentOrderDto.Quantity - order.Quantity;
                // If Difference Greater Than 0 (Mean Order Extra Quantity) 
                if (quantityDifference > 0)
                {
                    // Check If Not Found Enough Quantity In Stock
                    if (drug.Stock < quantityDifference)
                    {
                        invalidOrders.Add(order);
                        continue;
                    }
                }
                //If Found Enough Quantity In Stock, Discount From Drug Quantity
                drug.Stock -= quantityDifference;
                _db.Drugs.Update(drug);
                //Old Order Total Price
                var oldOrderTotalPrice = order.Quantity * order.UnitPrice;
                order.Quantity = currentOrderDto.Quantity;
                order.DrugId = drug.Id;
                order.UnitPrice = (float)drug.SellPrice;

                //New Order Total Price
                var newOrderTotalPrice = order.Quantity * order.UnitPrice;
                // Difference Old And New Total Price
                diffOldNewTotalPrice = newOrderTotalPrice - oldOrderTotalPrice;
                validOrders.Add(order);
            }
            if (validOrders.Count != 0)
                //Update Total Price Pharmacy Orders
                validOrders[0].PharmacyOrders.OrdersTotalPrice += diffOldNewTotalPrice;

            // Save Changes On Drugs And OrderDetails In DB
            _db.OrdersDetails.UpdateRange(validOrders);
            await _db.SaveChangesAsync();

            var validOrdersView = _mapper.Map<List<OrderViewModel>>(validOrders);
            var invalidOrdersView = _mapper.Map<List<OrderViewModel>>(invalidOrders);
            var validAndInvalidOrders = new ValidAndInvalidOrdersViewModel()
            {
                validOrders = validOrdersView,
                InvalidOrders = invalidOrdersView
            };

            //Check If Pharmacy Add New Orders When Update To Add In DB
            if (NewOrders.Count > 0)
            {
                //Call Create Method 
                var createResult = await CreateOrderGeneric(NewOrders, PharmacyId, ordersDetailsList[0].PharmacyOrdersId);
                //Check if All Orders Quantities UnAvaialable
                if (createResult.Count == 0)
                {
                    flagCreateResult = true;//Invalid Create
                }
                else
                {
                    //Create All Orders Succeeded
                    if (ResultCheckQuantity.Length > 0)
                    {
                        flagResultCheckQuantity = true;//Invalid Create
                    }
                    //Add New Create Orders To validOrdersView List
                    validOrdersView.AddRange(createResult);
                }
            }

            //Check If All Orders Invalid
            if (validOrdersView.Count == 0 && flagCreateResult)
            {
                return new ResponseViewModel<ValidAndInvalidOrdersViewModel>
                      (false, "All Orders Invalid Becouse Not Enough Quantity In Stock!", null);
            }
            else
            {
                //Check If Found Invalid Orders
                if (invalidOrdersView.Count > 0 && flagResultCheckQuantity)
                {
                    return new ResponseViewModel<ValidAndInvalidOrdersViewModel>
                        (true, "Update Succeeded But Found Some Orders Invalid Becouse Not Enough Quantity In Stock!", validAndInvalidOrders);
                }
                //Not Found Any Invalid Orders
                else
                {
                    return new ResponseViewModel<ValidAndInvalidOrdersViewModel>
                        (true, "Update Succeeded", validAndInvalidOrders);
                }
            }
        }

        //Delete Order
        public async Task<ResponseViewModel<OrderViewModel>> DeleteOrder(int OrderId, string PharmacyId)
        {
            var OrdersDetails = await _db.OrdersDetails.Include(D => D.Drug)
                                .Include(ord => ord.PharmacyOrders)
                                .SingleOrDefaultAsync(ord => ord.Id == OrderId
                                        && ord.Status == Status.Pending
                                        && ord.PharmacyOrders.PharmacyId == PharmacyId);
            if (OrdersDetails is null)
            {
                return new ResponseViewModel<OrderViewModel>(false, "Order Not Found", null);
            }
            OrdersDetails.Status = Status.Cancelled;
            //To Update Drug Quantity From Stock
            var drug = await _db.Drugs.FindAsync(OrdersDetails.DrugId);
            drug.Stock += OrdersDetails.Quantity;
            //Old Order Total Price
            var oldOrderTotalPrice = OrdersDetails.Quantity * OrdersDetails.UnitPrice;
            OrdersDetails.PharmacyOrders.OrdersTotalPrice -= oldOrderTotalPrice;
            _db.Drugs.Update(drug);
            _db.OrdersDetails.Update(OrdersDetails);
            await _db.SaveChangesAsync();
            var deletedOrder = _mapper.Map<OrderViewModel>(OrdersDetails);
            return new ResponseViewModel<OrderViewModel>(true, "Deleted Succeeded", deletedOrder);
        }

        //Display Pharmacy Account Information
        public async Task<ResponseViewModel<PharmacyAccountInformationViewModel>> AccountInformation(string PharmacyId)
        {
            var pharmacy = await _db.Users.Where(phar => phar.Id == PharmacyId)
                .Select(phar => new PharmacyAccountInformationViewModel
                {
                    PharmacyName = phar.PharmacyName,
                    AccountNumber = phar.AccountNumber,
                    Address = phar.Address,
                    Latitude = phar.Latitude,
                    Longitude = phar.Longitude
                }).SingleOrDefaultAsync();
            if (pharmacy is null)
            {
                return new ResponseViewModel<PharmacyAccountInformationViewModel>(false, "Pharmacy Not Found!", null);
            }
            return new ResponseViewModel<PharmacyAccountInformationViewModel>(true, "Pharmacy Information", pharmacy);
        }

        //Edit Pharmacy Name
        public async Task<ResponseViewModel<string>> EditAccountInformation
            (string PharmacyName, string PharmacyId)
        {
            var pharmacy = await _db.Users.FindAsync(PharmacyId);
            if (pharmacy is null)
            {
                return new ResponseViewModel<string>(false, "Error, Pharmacy Not Found!", null);
            }
            if (PharmacyName is null)
            {
                return new ResponseViewModel<string>(false, "Error, please Send New Pharmacy Name", null);
            }
            pharmacy.PharmacyName = PharmacyName;
            _db.Users.Update(pharmacy);
            await _db.SaveChangesAsync();
            return new ResponseViewModel<string>(true, "Update Succeeded", PharmacyName);
        }

        //Edit Pharmacy Password
        public async Task<ResponseViewModel<EditPharmacyPasswordDto>> EditPassword
            (EditPharmacyPasswordDto editPharmacyPassword, string PharmacyId)
        {
            var pharmacy = await _db.Users.FindAsync(PharmacyId);
            if (pharmacy is null)
            {
                return new ResponseViewModel<EditPharmacyPasswordDto>(false, "Pharmacy Not Found!", null);
            }
            if (editPharmacyPassword is null)
            {
                return new ResponseViewModel<EditPharmacyPasswordDto>(false, "Error, please Send New Pharmacy Password", null);
            }
            if (string.IsNullOrEmpty(editPharmacyPassword.CurrentPassword)
                || string.IsNullOrEmpty(editPharmacyPassword.NewPassword))
            {
                return new ResponseViewModel<EditPharmacyPasswordDto>(false, "Error, Current And New Password Is Required", null);
            }
            var Result = await _userManager.ChangePasswordAsync(pharmacy, editPharmacyPassword.CurrentPassword, editPharmacyPassword.NewPassword);
            if (!Result.Succeeded)
            {
                return new ResponseViewModel<EditPharmacyPasswordDto>(false, "Error, Invalid Current Password", null);
            }
            _db.Users.Update(pharmacy);
            await _db.SaveChangesAsync();
            return new ResponseViewModel<EditPharmacyPasswordDto>(true, "Update Succeeded", editPharmacyPassword);
        }

        //Edit Pharmacy Location
        public async Task<ResponseViewModel<EditPharmacyLocation>> EditPharmacyLocation
            (EditPharmacyLocation editPharmacyLocation, string PharmacyId)
        {
            var pharmacy = await _db.Users.FindAsync(PharmacyId);
            if (pharmacy is null)
            {
                return new ResponseViewModel<EditPharmacyLocation>(false, "Pharmacy Not Found!", null);
            }
            if (editPharmacyLocation is null)
            {
                return new ResponseViewModel<EditPharmacyLocation>(false, "Error, please Send Pharmacy Coordinates", null);
            }
            if (string.IsNullOrEmpty(editPharmacyLocation.Address) ||
                string.IsNullOrEmpty(editPharmacyLocation.Longitude) ||
                string.IsNullOrEmpty(editPharmacyLocation.Latitude))
            {
                return new ResponseViewModel<EditPharmacyLocation>(false, "Error, Pharmacy Coordinates Is Required", null);
            }
            pharmacy.Address = editPharmacyLocation.Address;
            pharmacy.Latitude = editPharmacyLocation.Latitude;
            pharmacy.Longitude = editPharmacyLocation.Longitude;
            _db.Users.Update(pharmacy);
            await _db.SaveChangesAsync();
            return new ResponseViewModel<EditPharmacyLocation>(true, "Update Succeeded", editPharmacyLocation);
        }

        //Dispaly Name And Id From All Pharmacys 
        public async Task<ResponseViewModel<List<GetAllPharmacys>>> AllPharmacys()
        {
            var pharmacys = await _db.Users
                .Select(phar => new GetAllPharmacys { Id = phar.Id, PharmacyName = phar.PharmacyName }).ToListAsync();
            if (pharmacys is null)
            {
                return new ResponseViewModel<List<GetAllPharmacys>>(false, "Not Found Data", null);
            }
            return new ResponseViewModel<List<GetAllPharmacys>>(true, "Succeeded Process", pharmacys);
        }

        //Dispaly Pharmacy Notifications
        public async Task<ResponseViewModel<List<PharmacyNotifications>>> PharmacyNotifications(string PharmacyId)
        {
            var pharmacyNotifications = await _db.Notifications.Include(pharOrds => pharOrds.PharmacyOrders)
                                        .Where(notification => notification.PharmacyId == PharmacyId
                                         && !notification.IsRead
                                         && notification.NotificationStatus == NotificationStatus.Processed)
                                        .OrderByDescending(notification => notification.PharmacyOrders.CreatedAt)
                                        .Select(notificate => new PharmacyNotifications
                                        {
                                            PharmacyOrdersId = notificate.PharmacyOrdersId,
                                            OrderCreateAt = notificate.PharmacyOrders.CreatedAt.ToString("MM/dd/yyyy"),
                                            TimeOrderCreateAt = notificate.PharmacyOrders.CreatedAt.ToString("hh:mm tt")
                                        }).ToListAsync();

            if (pharmacyNotifications is null)
            {
                return new ResponseViewModel<List<PharmacyNotifications>>(false, "Not Found Notifications", null);
            }
            return new ResponseViewModel<List<PharmacyNotifications>>(true, "All Notifications", pharmacyNotifications);
        }

        //Change Pharmacy Notification Status To Become IsReaded
        public async Task<ResponseViewModel<bool>> ChangePharmacyNotificationStatus(string PharmacyId)
        {
            var pharmacyNotifications = await _db.Notifications.Where(notification =>
                                     notification.NotificationStatus == NotificationStatus.Processed
                                     && !notification.IsRead
                                     && notification.PharmacyId == PharmacyId).ToListAsync();
            //Change Notifications Status To Become IsReaded If Found Notifications
            if (pharmacyNotifications is not null)
            {
                foreach (var notification in pharmacyNotifications)
                {
                    notification.IsRead = true;
                }
                _db.UpdateRange(pharmacyNotifications);
                await _db.SaveChangesAsync();
                return new ResponseViewModel<bool>(true, "Change All Notifications Succeeded", true);
            }
            return new ResponseViewModel<bool>(true, "Not Found Notifications Unread", false);
        }

        // ======================= Starte Generic Method ======================= :
        // Generic Method To Display Current And Archived Orders To Pharmacy
        public async Task<ResponseViewModel<ResponseDto>> PharmacyCurrentAndArchivedOrders
            (string PharmacyId, PagingWithQueryDto pagination, Status status, string SuccessedProcess, string FailedProcees)
        {
            var pendingOrArchivedOrders = new List<OrderDetails>().AsQueryable();
            if (status == Status.Pending)
            {
                pendingOrArchivedOrders = _db.OrdersDetails.Include(ord => ord.PharmacyOrders)
                                             .ThenInclude(phar => phar.Pharmacy)
                                             .Include(ord => ord.Drug)
                                             .Where(ord => ord.PharmacyOrders.PharmacyId == PharmacyId &&
                                              ord.Status == Status.Pending)
                                             .OrderByDescending(ords => ords.PharmacyOrders.CreatedAt)
                                             .AsQueryable();
            }
            else
            {
                pendingOrArchivedOrders = _db.OrdersDetails.Include(ord => ord.PharmacyOrders)
                                             .ThenInclude(phar => phar.Pharmacy)
                                             .Include(ord => ord.Drug)
                                             .Where(ord => ord.PharmacyOrders.PharmacyId == PharmacyId &&
                                              ord.Status != Status.Pending)
                                             .OrderByDescending(ords => ords.PharmacyOrders.CreatedAt)
                                             .AsQueryable();
            }

            if (pendingOrArchivedOrders is null)
            {
                return new ResponseViewModel<ResponseDto>(false, FailedProcees, null);
            }
            var dataCount = pendingOrArchivedOrders.Count();
            var skipValue = pagination.GetSkipValue(dataCount);
            var pages = pagination.GetPages(dataCount);
            var pendingOrdersList = await pendingOrArchivedOrders.Skip(skipValue).Take(pagination.PerPage).ToListAsync();
            var AllpendingOrders = _mapper.Map<List<CurrentAndArchivedOrdersViewModel>>(pendingOrdersList);
            var pendingOrdersGrouping = AllpendingOrders.GroupBy
                (ord => new
                {
                    ord.PharmacyOrders.CreatedAt.Date,
                    ord.PharmacyOrdersId
                }).Select(ords => new PharmacyGroupingByResult
                {
                    Key = new PharmacyKey
                    {
                        CreateAt = ords.Key.Date.ToString("ddd-dd,MMM,yyyy"),
                        PharmacyOrdersId = ords.Key.PharmacyOrdersId
                    },
                    OrdersDetails = ords.ToList()
                }).ToList();

            var pendingOrdersResult = new ResponseDto
            {
                data = pendingOrdersGrouping,
                meta = new Meta
                {
                    page = pagination.Page,
                    perpage = pagination.PerPage,
                    pages = pages,
                    total = dataCount,
                }
            };
            return new ResponseViewModel<ResponseDto>(true, SuccessedProcess, pendingOrdersResult);
        }

        // Generic Method To Create Order
        public async Task<List<OrderViewModel>> CreateOrderGeneric
            (List<CreateOrderDto> OrderDto, string PharmacyId, int PharmacyOrdId)
        {
            var InvalidOrders = new List<OrderDetails>();
            var DrugsUpdate = new List<Drug>();
            float OrdersTotalPrice = 0;
            int pharmacyOrdersId = PharmacyOrdId;
            var PharmacyOrders = new PharmacyOrders();
            var AllOrders = new List<OrderViewModel>();

            var orders = _mapper.Map<List<OrderDetails>>(OrderDto);
            //Create New Order (Not Update Operation)
            if (pharmacyOrdersId == 0)
            {
                PharmacyOrders.PharmacyId = PharmacyId;
                await _db.PharmacysOrders.AddAsync(PharmacyOrders);
                await _db.SaveChangesAsync();
                //Get New Pharmacy Orders Id
                pharmacyOrdersId = PharmacyOrders.Id;
                // Store (PharmacyOrders.Id) In (PharmacyOrderId) To Notification
                PharmacyOrderId = PharmacyOrders.Id;
            }
            foreach (var order in orders)
            {
                orderDetails = await _drugService.DrugDetails(order.DrugId);
                if (orderDetails.Data.Stock < order.Quantity || order.Quantity < 1)
                {
                    InvalidOrders.Add(order);
                    ResultCheckQuantity = "But Some Orders Not Added Because Quantity Unavailable!";
                    continue;
                }
                order.PharmacyOrdersId = pharmacyOrdersId;
                order.UnitPrice = orderDetails.Data.SellPrice;
                // To Sum Total Price To All Orders And Store In Column OrdersTotalPrice
                OrdersTotalPrice += order.Quantity * orderDetails.Data.SellPrice;
                //To Discount Quantity From Drug Stock
                var drug = await _db.Drugs.FindAsync(order.DrugId);
                drug.Stock -= order.Quantity;
                DrugsUpdate.Add(drug);
            }
            // Check if Found Orders Quantitys UnAvailable
            if (InvalidOrders.Count > 0)
            {
                foreach (var order in InvalidOrders)
                {
                    orders.Remove(order);
                }
            }
            // Check if All Orders Quantitys UnAvaialable
            if (orders.Count < 1)
            {
                _db.PharmacysOrders.Remove(PharmacyOrders);
                await _db.SaveChangesAsync();
                //Return Empty List
                return AllOrders;
            }
            //Update On Drugs Stock
            _db.Drugs.UpdateRange(DrugsUpdate);
            // Add Orders And Update on Drug Stock
            await _db.OrdersDetails.AddRangeAsync(orders);
            await _db.SaveChangesAsync();

            //Create New Order (Not Update Operation)
            if (PharmacyOrdId == 0)
            {
                //Update OrdersTotalPrice To Store Sum Orders Total Price
                PharmacyOrders.OrdersTotalPrice = OrdersTotalPrice;
                _db.PharmacysOrders.Update(PharmacyOrders);
                await _db.SaveChangesAsync();
                AllOrders = _mapper.Map<List<OrderViewModel>>(orders);
            }
            //Update Current Order
            else
            {
                var pharmacyOrders = await _db.PharmacysOrders.SingleOrDefaultAsync(pharOrd => pharOrd.Id == PharmacyOrdId);
                if (pharmacyOrders is not null)
                {
                    pharmacyOrders.OrdersTotalPrice += OrdersTotalPrice;
                    _db.PharmacysOrders.Update(pharmacyOrders);
                    await _db.SaveChangesAsync();
                    AllOrders = _mapper.Map<List<OrderViewModel>>(orders);
                }
            }


            return AllOrders;
        }

        //// Generic Method To Update Order (Return Pending) And Reorder (Return Completes & Canceled)
        //public async Task<ResponseViewModel<List<OrderViewModel>>> ReOrderUpdateOrderByPharmacy
        //                                    (int OrderId, string PharmacyId, Status status)
        //{
        //    var ordersDetailsList = await _db.OrdersDetails
        //                            .Include(drug => drug.Drug)
        //                            .Include(phar => phar.PharmacyOrders)
        //                            .Where(ord => ord.PharmacyOrdersId == OrderId
        //                                   && ((status == Status.Pending) ? ord.Status == Status.Pending :
        //                                                                   ord.Status != Status.Pending)
        //                                   && ord.PharmacyOrders.PharmacyId == PharmacyId).ToListAsync();
        //    if (ordersDetailsList.Count == 0)
        //    {
        //        return new ResponseViewModel<List<OrderViewModel>>(false, "Not Found Orders", null);
        //    }
        //    var orders = _mapper.Map<List<OrderViewModel>>(ordersDetailsList);
        //    return new ResponseViewModel<List<OrderViewModel>>(true, "Orders Is Found", orders);
        //}

        // ======================= End Generic Method ======================= 
        public async Task<DataTableOutput<List<GetOrdersDataTable>>> GetPharmacyCurrentOrders(DataTableDto dto, string PharmacyId)
        {
            var pageSize = dto.length;
            var skip = dto.start;

            var OrdersDetails = _db.OrdersDetails.Include(O => O.Drug).Include(PO => PO.PharmacyOrders)
                .ThenInclude(P => P.Pharmacy)
                .Where(P => P.PharmacyOrders.PharmacyId.Contains(PharmacyId) && P.Status == Status.Pending)
                .Select(x => new GetOrdersDataTable
                {
                    CreatedAt = x.PharmacyOrders.CreatedAt,
                    CreatedAtString = x.PharmacyOrders.CreatedAt.ToString("MM/dd/yyyy HH:mm:ss"),
                    DrugId = x.Drug.DrugId,
                    DrugName = x.Drug.DrugName,
                    OrderId = x.Id,
                    PricePerUnit = x.UnitPrice,
                    Quantity = x.Quantity,
                    TotalPrice = x.Quantity * x.UnitPrice,
                    Status = x.Status,
                    PharmacyOrderId = x.PharmacyOrdersId,
                    TotalPricePharmacyOrders = x.PharmacyOrders.OrdersTotalPrice

                }).OrderByDescending(a => a.OrderId);

            var data = await OrdersDetails.Skip(skip).Take(pageSize).ToListAsync();

            var recordsTotal = await OrdersDetails.CountAsync();

            return new DataTableOutput<List<GetOrdersDataTable>>(recordsTotal, recordsTotal, data);
        }

        public async Task<DataTableOutput<List<GetOrdersDataTable>>> GetPharmacyArchivedOrders
            (DataTableDto dto, string PharmacyId, int pharmacyOrdersId)
        {

            var pageSize = dto.length;
            var skip = dto.start;

            var OrdersDetails = _db.OrdersDetails.Include(O => O.Drug).Include(PO => PO.PharmacyOrders)
                .ThenInclude(P => P.Pharmacy)
                .Where(P => P.PharmacyOrders.PharmacyId.Contains(PharmacyId) && P.Status != Status.Pending)
                .Where(P => pharmacyOrdersId == 0 ? true : P.PharmacyOrdersId.Equals(pharmacyOrdersId))
                .Select(x => new GetOrdersDataTable
                {
                    CreatedAt = x.PharmacyOrders.CreatedAt,
                    CreatedAtString = x.PharmacyOrders.CreatedAt.ToString("MM/dd/yyyy HH:mm:ss"),
                    PharmacyOrderId = x.PharmacyOrdersId,
                    DrugId = x.Drug.DrugId,
                    DrugName = x.Drug.DrugName,
                    OrderId = x.Id,
                    PricePerUnit = x.UnitPrice,
                    Quantity = x.Quantity,
                    TotalPrice = x.Quantity * x.UnitPrice,
                    Status = x.Status,
                    ArchivedTotalPricePharmacyOrders = x.PharmacyOrders.Orders.Where(x => x.Status == Status.Completed)
                                                        .Sum(x => x.Quantity * x.UnitPrice)
                }).OrderByDescending(a => a.OrderId);

            var data = await OrdersDetails.Skip(skip).Take(pageSize).ToListAsync();

            var recordsTotal = await OrdersDetails.CountAsync();

            return new DataTableOutput<List<GetOrdersDataTable>>(recordsTotal, recordsTotal, data);
        }

        public async Task<List<Status>> GetStatusForPharmacy(string id)
        {
            var status = new List<Status>();
            var ordersList = _db.PharmacysOrders.Include(x => x.Orders).Where(x => x.PharmacyId == id).Select(x => x.Orders.Select(y => y.Status)).ToList();
            if (ordersList is not null)
                status = ordersList.SelectMany(x => x).ToList();
            return status;
        }
    }
}
