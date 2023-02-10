﻿using AutoMapper;
using DrugStore;
using DrugStore.Data;
using DrugStore.SignalRHub;
using DrugStoreCore.Dto;
using DrugStoreCore.Enums;
using DrugStoreCore.ViewModel;
using DrugStoreData.Models;
using DrugStoreInfrastructure.PaginationHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelColumn = DrugStoreInfrastructure.ExcelHelpers.ExcelColumn;
using ExcelRow = DrugStoreInfrastructure.ExcelHelpers.ExcelRow;

namespace DrugStoreInfrastructure.Services.DisplayOrders
{
    public class DisplayOrdersService : IDisplayOrdersService
    {
        private readonly DrugStoreDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<Pharmacy> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        public DisplayOrdersService(DrugStoreDbContext db, IMapper mapper,
            UserManager<Pharmacy> userManager, IHubContext<ChatHub> hubContext)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            _hubContext = hubContext; ;
        }

        // Get Current Orders To Admin
        public async Task<ResponseViewModel<ResponseDto>> AdminCurrentOrders
            (PagingWithQueryDto pagination)
        {
            return await AdminCurrentAndArchivedOrders
             (pagination, Status.Pending, "pending Orders", "Not Found Any pending Orders");
        }

        // Get Archived Orders To Admin
        public async Task<ResponseViewModel<ResponseDto>> AdminArchivedOrders
            (PagingWithQueryDto pagination)
        {
            return await AdminCurrentAndArchivedOrders
             (pagination, Status.Completed, "Archived Orders", "Not Found Any Archived Orders");
        }

        // Process Pending Orders With Convert It To Completed And Export It To Excel
        public async Task<ResponseViewModel<byte[]>> ProcessPendingOrdersAndExportExcel(int[] UnProcessedOrders, Query query)
        {
            var notificationsList = new List<Notification>();
            var pendingOrders =  _db.OrdersDetails.Include(drug => drug.Drug)
                             .Include(pharOrds => pharOrds.PharmacyOrders)
                             .ThenInclude(phar => phar.Pharmacy)
                             .Where(ord => ord.Status == Status.Pending
                                    && ((UnProcessedOrders.Length == 0) ? true : !UnProcessedOrders.Any(id => id == ord.PharmacyOrdersId)))
                              .OrderByDescending(ords => ords.PharmacyOrders.CreatedAt)
                             .AsQueryable();

            //To Check If Not Found Any pending Orders
            if (pendingOrders.Count() == 0)
            {
                return new ResponseViewModel<byte[]>(false, "Not Found Any pending Orders", null);
            }

            var resultFilter = GenericFilter(pendingOrders, query);

            //To Check If Not Found Any pending Orders After Filter
            if (resultFilter is null)
            {
                return new ResponseViewModel<byte[]>(false, "Not Found Any pending Orders", null);
            }
            var executeOrdDetails = await resultFilter.ToListAsync();


            //Change Pending Orders To Completed
            foreach (var ord in executeOrdDetails)
            {
                ord.Status = Status.Completed;
            }
            _db.OrdersDetails.UpdateRange(executeOrdDetails);
            await _db.SaveChangesAsync();

            // Export Excel File
            var exportExcel = ExcelHelpers.ExcelHelpers.ToExcel(new Dictionary<string, ExcelColumn>
            {
                {"CreatedAt", new ExcelColumn("CreatedAt", 0)},
                {"Time", new ExcelColumn("Time", 1)},
                {"PharmacyName", new ExcelColumn("PharmacyName", 2)},
                {"DrugId", new ExcelColumn("DrugId", 3)},
                {"DrugName", new ExcelColumn("DrugName", 4)},
                {"Quantity", new ExcelColumn("Quantity", 5)},
                {"UnitPrice", new ExcelColumn("UnitPrice", 6)},
                {"TotalPrice", new ExcelColumn("TotalPrice", 7)},
                {"OldStatus", new ExcelColumn("OldStatus", 8)},
                {"NewStatus", new ExcelColumn("NewStatus", 9)}
            }, new List<ExcelRow>(executeOrdDetails.Select(ord => new ExcelRow
            {
                Values = new Dictionary<string, string>
                {
                    {"CreatedAt", ord.PharmacyOrders.CreatedAt.ToString("yyyyy-MM-dd")},
                    {"Time", ord.PharmacyOrders.CreatedAt.ToString("HH:mm")},
                    {"PharmacyName", ord.PharmacyOrders.Pharmacy.PharmacyName},
                    {"DrugId", ord.Drug.DrugId.ToString()},
                    {"DrugName", ord.Drug.DrugName.ToString()},
                    {"Quantity", ord.Quantity.ToString()},
                    {"UnitPrice", ord.UnitPrice.ToString()},
                    {"TotalPrice", (ord.Quantity * ord.UnitPrice).ToString()},
                    {"OldStatus", "Pending"},
                    {"NewStatus", "Completed"}
                }
            })));

            //Start Add Notification To Pharmacies
            //Grouping By PharmacyOrdersId And PharmacyId
            var notificationGroupings = pendingOrders.GroupBy(ords => new
            {
                ords.PharmacyOrdersId,
                ords.PharmacyOrders.PharmacyId,
                ords.PharmacyOrders.CreatedAt
            }).Select(ordsGroup => new NotificationGroupingBy
            {
                PharmacyOrdersId = ordsGroup.Key.PharmacyOrdersId,
                PharmacyId = ordsGroup.Key.PharmacyId,
                CreateAt = ordsGroup.Key.CreatedAt
            }).ToList();

            foreach (var notificationItem in notificationGroupings)
            {
                //Realtime Notification Using SingnalR
                var pharmacyNotifications = new PharmacyNotifications
                {
                    PharmacyOrdersId = notificationItem.PharmacyOrdersId,
                    OrderCreateAt = notificationItem.CreateAt.ToString("MM/dd/yyyy"),
                    TimeOrderCreateAt = notificationItem.CreateAt.ToString("hh:mm tt")
                };
                //Call ChatHub To Push Notification To Admin
                await _hubContext.Clients.User(notificationItem.PharmacyId)
                    .SendAsync("PharmacyReceiveNotifications", pharmacyNotifications);



                //Add All PharmacyOrders In List To Add Table Notifications In DB
                var notification = new Notification
                {
                    PharmacyId = notificationItem.PharmacyId,
                    PharmacyOrdersId = notificationItem.PharmacyOrdersId,
                    NotificationStatus = NotificationStatus.Processed
                };
                notificationsList.Add(notification);
            }
            //End
            await _db.Notifications.AddRangeAsync(notificationsList);
            await _db.SaveChangesAsync();
            //End Notification
            return new ResponseViewModel<byte[]>(true, "Succeeded Process", exportExcel);
        }

        // Export Archived Orders By Filters To Admin(Admin Reports)
        public async Task<ResponseViewModel<byte[]>> ExportArchivedOrders(Query query)
        {
            var ArchivedOrders = _db.OrdersDetails.Include(drug => drug.Drug)
                             .Include(pharOrds => pharOrds.PharmacyOrders)
                             .ThenInclude(phar => phar.Pharmacy)
                             .Where(ord => ord.Status != Status.Pending)
                             .OrderByDescending(ords => ords.PharmacyOrders.CreatedAt)
                             .AsQueryable();

            var resultFilter = GenericFilter(ArchivedOrders, query);

            if (resultFilter is null)
            {
                return new ResponseViewModel<byte[]>(false, "Not Found Data", null);
            }
            var archivedgOrdersList = await resultFilter.ToListAsync();

            // Export Excel File
            var exportExcel = ExcelHelpers.ExcelHelpers.ToExcel(new Dictionary<string, ExcelColumn>
            {
                {"CreatedAt", new ExcelColumn("CreatedAt", 0)},
                {"Time", new ExcelColumn("Time", 1)},
                {"PharmacyName", new ExcelColumn("PharmacyName", 2)},
                {"DrugId", new ExcelColumn("DrugId", 3)},
                {"DrugName", new ExcelColumn("DrugName", 4)},
                {"Quantity", new ExcelColumn("Quantity", 5)},
                {"UnitPrice", new ExcelColumn("UnitPrice", 6)},
                {"TotalPrice", new ExcelColumn("TotalPrice", 7)},
                {"Status", new ExcelColumn("Status", 8)},
            }, new List<ExcelRow>(archivedgOrdersList.Select(ord => new ExcelRow
            {
                Values = new Dictionary<string, string>
                {
                    {"CreatedAt", ord.PharmacyOrders.CreatedAt.ToString("yyyyy-MM-dd")},
                    {"Time", ord.PharmacyOrders.CreatedAt.ToString("HH:mm")},
                    {"PharmacyName", ord.PharmacyOrders.Pharmacy.PharmacyName},
                    {"DrugId", ord.Drug.DrugId.ToString()},
                    {"DrugName", ord.Drug.DrugName.ToString()},
                    {"Quantity", ord.Quantity.ToString()},
                    {"UnitPrice", ord.UnitPrice.ToString()},
                    {"TotalPrice", (ord.Quantity * ord.UnitPrice).ToString()},
                    {"Status", ord.Status.ToString()},
                }
            })));
            return new ResponseViewModel<byte[]>(true, "Succeeded Process", exportExcel);
        }

        // Get Orders To Admin To Update
        public async Task<ResponseViewModel<List<OrderViewModel>>> UpdateOrderByAdmin(int OrderId)
        {
            var ordersDetailsList = await _db.OrdersDetails.Include(drug => drug.Drug)
                                .Where(ord => ord.PharmacyOrdersId == OrderId
                                       && ord.Status == Status.Pending).ToListAsync();
            if (ordersDetailsList.Count == 0)
            {
                return new ResponseViewModel<List<OrderViewModel>>(false, "Not Found Orders", null);
            }
            var orders = _mapper.Map<List<OrderViewModel>>(ordersDetailsList);
            return new ResponseViewModel<List<OrderViewModel>>(true, "Orders Is Found", orders);
        }

        // Post Orders By Admin To Update
        public async Task<ResponseViewModel<ValidAndInvalidOrdersViewModel>> UpdateOrderByAdmin(List<UpdateOrderDto> OrdersDto)
        {
            var counter = 0;
            var invalidOrders = new List<OrderDetails>();
            var validOrders = new List<OrderDetails>();
            var OrdersId = new List<int>();
            float diffOldNewTotalPrice = 0;
            //Get Orders Id And Store In List
            foreach (var ord in OrdersDto)
            {
                //Check If Admin Change Any Drug Quantity To Less Than 1 Return Error  
                if (ord.Quantity < 1)
                {
                    return new ResponseViewModel<ValidAndInvalidOrdersViewModel>
                      (false, "Error, All Drug Quantities Must Be Greater Than 0", null);
                }

                OrdersId.Add(ord.Id);
            }
            var ordersDetailsList = await _db.OrdersDetails.Include(drug => drug.Drug)
                                    .Include(ord => ord.PharmacyOrders)
                                    .Where(order => OrdersId.Contains(order.Id)).ToListAsync();
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
                    counter++;
                    continue;
                }

                //Check If Order Status IsHanging
                if (order.IsHanging)
                {
                    validOrders.Add(order);
                    counter++;
                    continue;
                }

                //Get Single Drug By Id 
                var drug = await _db.Drugs.FindAsync(OrdersDto[counter].DrugId);
                //Check If Drug Not Found, Make Drug Equal Old Drug
                if (drug is null)
                {
                    drug = order.Drug;
                }
                // Get Difference Between Old Quantity And New Quantity
                var quantityDifference = OrdersDto[counter].Quantity - order.Quantity;
                // If Difference Greater Than 0 (Mean Order Extra Quantity) 
                if (quantityDifference > 0)
                {
                    // Check If Not Found Enough Quantity In Stock
                    if (drug.Stock < quantityDifference)
                    {
                        invalidOrders.Add(order);
                        counter++;
                        continue;
                    }
                }
                //If Found Enough Quantity In Stock, Discount From Drug Quantity
                drug.Stock -= quantityDifference;
                _db.Drugs.Update(drug);
                //Old Order Total Price
                var oldOrderTotalPrice = order.Quantity * order.UnitPrice;
                order.Quantity = OrdersDto[counter].Quantity;
                order.DrugId = drug.Id;
                order.UnitPrice = (float)drug.SellPrice;

                //New Order Total Price
                var newOrderTotalPrice = order.Quantity * order.UnitPrice;
                // Difference Old And New Total Price
                diffOldNewTotalPrice = newOrderTotalPrice - oldOrderTotalPrice;
                validOrders.Add(order);
                counter++;
            }
            //Update Total Price Pharmacy Orders
            if (validOrders.Count != 0)
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
            //Check If All Orders Invalid
            if (validOrdersView.Count == 0)
            {
                return new ResponseViewModel<ValidAndInvalidOrdersViewModel>
                      (false, "All Orders Invalid Becouse Not Enough Quantity In Stock!", null);
            }
            else
            {
                //Check If Found Invalid Orders
                if (invalidOrdersView.Count > 0)
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

        // Import Drugs Information As Excel File 
        public async Task<ResponseViewModel<int?>> ImportDrugsInfoAsExcel
            (IFormFile excelFile)
        {
            var DrugsIdList = new List<int>();
            if (excelFile is null)
            {
                return new ResponseViewModel<int?>(false, "Error, Please Upload Excel File!", null);
            }
            var ExistsDrugs = new List<Drug>();
            var NewDrugs = new List<Drug>();
            var drugFromExcel = new Drug();
            var drugFromDb = new Drug();
            int rowCount = 0;
            //Get File Information
            FileInfo fileInfo = new FileInfo(excelFile.FileName);
            //Get File Extention
            string fileExtension = fileInfo.Extension;
            //Check If File Extention Not 'xlsx' return Invalid File
            if (fileExtension != ".xlsx")
            {
                return new ResponseViewModel<int?>(false, "Error, Please Upload Excel File As .xlsx!", null);
            }
            using (var stream = new MemoryStream())
            {
                //Convert File To Stream
                await excelFile.CopyToAsync(stream);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Pass Stream To ExcelPackage To Reading
                using (var package = new ExcelPackage(stream))
                {
                    //Holde Excel File In variable(worksheet) Type ExcelWorksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    // Count Rows In Excel File
                    rowCount = worksheet.Dimension.Rows;
                    //Loop On All Rows And Store Each Row In List
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            drugFromExcel = new Drug
                            {
                                DrugId = (int?)(double?)worksheet.Cells[row, 1].Value,
                                DrugName = worksheet.Cells[row, 2].Value?.ToString(),
                                Unit = worksheet.Cells[row, 3].Value?.ToString(),
                                Stock = (float?)(double?)worksheet.Cells[row, 4].Value,
                                SellPrice = (float?)(double?)worksheet.Cells[row, 5].Value,
                            };
                            //If Drug Not Have Drug Number Will Be Ignore
                            if (drugFromExcel.DrugId is null)
                            {
                                continue;
                            }
                            drugFromDb = await _db.Drugs.SingleOrDefaultAsync
                                            (drug => drug.DrugId == drugFromExcel.DrugId);
                            //If Drug is Exists
                            if (drugFromDb is not null)
                            {
                                //Change Drug Status To Active If It NonActive
                                drugFromDb.DrugStatus = DrugStatus.Active;
                                drugFromDb.DrugId = drugFromExcel.DrugId;
                                drugFromDb.DrugName = drugFromExcel.DrugName;
                                drugFromDb.Unit = drugFromExcel.Unit;
                                drugFromDb.Stock = drugFromExcel.Stock;
                                drugFromDb.SellPrice = drugFromExcel.SellPrice;

                                ExistsDrugs.Add(drugFromDb);
                                //Add DrugId To List To Check Drugs Will Be Removed
                                DrugsIdList.Add(drugFromDb.DrugId ?? 0);//?? If Null Return 0
                            }
                            //If Drug is Not Exists
                            else
                            {
                                drugFromExcel.DrugStatus = DrugStatus.Active;
                                NewDrugs.Add(drugFromExcel);
                                //Add DrugId To List To Check Drugs Will Be Removed
                                DrugsIdList.Add(drugFromExcel.DrugId ?? 0);//?? If Null Return 0
                            }
                        }
                        catch (Exception ex)
                        {
                            //If Happened Any Exception Will  Ignore This Period And Will Be It Continue
                            continue;
                        }
                    }
                }

                //Add And Update Drugs
                if (ExistsDrugs.Count != 0)
                    _db.Drugs.UpdateRange(ExistsDrugs);
                if (NewDrugs.Count != 0)
                    await _db.Drugs.AddRangeAsync(NewDrugs);

                await _db.SaveChangesAsync();
            }

            //Send DrugsId List To Change Drug Status If Found Drug Not Found In Excel
            await ChangeDrugStatusToNonActive(DrugsIdList);

            // Change OrderS tatus To Hanging If Found Any Pending Orders
            await ChangeOrderStatusToHanging();

            return new ResponseViewModel<int?>(true, "Upload Excel File Succeeded", rowCount);
        }

        //Import Pharmacys Information As Excel File
        public async Task<ResponseViewModel<int?>> ImportPharmacysInfoAsExcel
            (IFormFile excelFile)
        {
            if (excelFile is null)
            {
                return new ResponseViewModel<int?>(false, "Error, Please Upload Excel File!", null);
            }
            var ExistsPharmacys = new List<Pharmacy>();
            var PharmacyFromExcel = new Pharmacy();
            var PharmacyFromDb = new Pharmacy();
            int rowCount = 0;
            //Get File Information
            FileInfo fileInfo = new FileInfo(excelFile.FileName);
            //Get File Extention
            string fileExtension = fileInfo.Extension;
            //Check If File Extention Not 'xlsx' return Invalid File
            if (fileExtension != ".xlsx")
            {
                return new ResponseViewModel<int?>(false, "Error, Please Upload Excel File As .xlsx!", null);
            }
            using (var stream = new MemoryStream())
            {
                //Convert File To Stream
                await excelFile.CopyToAsync(stream);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Pass Stream To ExcelPackage To Reading
                using (var package = new ExcelPackage(stream))
                {
                    //Holde Excel File In variable(worksheet) Type ExcelWorksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    // Count Rows In Excel File
                    rowCount = worksheet.Dimension.Rows;
                    //Loop On All Rows And Store Each Row In List
                    for (int row = 3; row <= rowCount; row++)
                    {
                        try
                        {
                            PharmacyFromExcel = new Pharmacy
                            {
                                AccountNumber = (int?)(double?)worksheet.Cells[row, 1].Value,
                                PharmacyName = worksheet.Cells[row, 2].Value?.ToString(),
                            };
                            //If Pharmacy Not Have Account Number Will Be Ignore
                            if (PharmacyFromExcel.AccountNumber is null)
                            {
                                continue;
                            }
                            PharmacyFromDb = await _db.Users.SingleOrDefaultAsync
                                            (phar => phar.AccountNumber == PharmacyFromExcel.AccountNumber);
                            //If Pharmacy is Exists
                            if (PharmacyFromDb is not null)
                            {
                                PharmacyFromDb.PharmacyName = PharmacyFromExcel.PharmacyName;
                                ExistsPharmacys.Add(PharmacyFromDb);
                            }
                            //If Pharmacy is Not Exists
                            else
                            {
                                PharmacyFromExcel.PharmacyRole = "Pharmacy";
                                PharmacyFromExcel.UserName = PharmacyFromExcel.AccountNumber.ToString();
                                PharmacyFromExcel.SupplierId = null;
                                await _userManager.CreateAsync(PharmacyFromExcel, PharmacyFromExcel.UserName);
                            }
                        }
                        catch (Exception ex)
                        {
                            //If Happened Any Exception Will  Ignore This Period And Will Be It Continue
                            continue;
                        }
                    }
                }

                //Add And Update Pharmacys
                if (ExistsPharmacys.Count != 0)
                    _db.Users.UpdateRange(ExistsPharmacys);
                await _db.SaveChangesAsync();
            }
            return new ResponseViewModel<int?>(true, "Upload Excel File Succeeded", rowCount);
        }

        // Import Location Information As Excel File 
        public async Task<ResponseViewModel<int?>> ImportLocationsInfoAsExcel
            (IFormFile excelFile)
        {
            var PharmaciesIdList = new List<int>();
            if (excelFile is null)
            {
                return new ResponseViewModel<int?>(false, "Error, Please Upload Excel File!", null);
            }
            var existsPharmacy = new List<Pharmacy>();
            var pharmacyFromExcel = new Pharmacy();
            var pharmacyFromDb = new Pharmacy();
            int rowCount = 0;
            Random random = new Random();
            //Get File Information
            FileInfo fileInfo = new FileInfo(excelFile.FileName);
            //Get File Extention
            string fileExtension = fileInfo.Extension;
            //Check If File Extention Not 'xlsx' return Invalid File
            if (fileExtension != ".xlsx")
            {
                return new ResponseViewModel<int?>(false, "Error, Please Upload Excel File As .xlsx!", null);
            }
            using (var stream = new MemoryStream())
            {
                //Convert File To Stream
                await excelFile.CopyToAsync(stream);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Pass Stream To ExcelPackage To Reading
                using (var package = new ExcelPackage(stream))
                {
                    //Holde Excel File In variable(worksheet) Type ExcelWorksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    // Count Rows In Excel File
                    rowCount = worksheet.Dimension.Rows;
                    //Loop On All Rows And Store Each Row In List
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var supplierId = worksheet.Cells[row, 3].Value?.ToString();
                            var pharmacyId = worksheet.Cells[row, 5].Value?.ToString();
                            //If Admin Not Send supplierId Or supplierId Not Add Row From Excel
                            if (supplierId is null || supplierId is null)
                            {
                                continue;
                            }
                            //Get Supplier By Supplier Id
                            var supplierFromDb = await _db.Suppliers.SingleOrDefaultAsync
                                            (suplier => suplier.SupplierId.Equals(int.Parse(supplierId)));

                            //Check Supplier Is Null(Not Found In DB) Will Ignore This Row From Excel
                            if (supplierFromDb is null)
                            {
                                continue;
                            }

                            pharmacyFromExcel = new Pharmacy()
                            {
                                PharmacyName = worksheet.Cells[row, 4].Value?.ToString(),
                                Latitude = worksheet.Cells[row, 6].Value?.ToString(),
                                Longitude = worksheet.Cells[row, 7].Value?.ToString()
                            };

                            pharmacyFromDb = await _db.Users.SingleOrDefaultAsync
                                            (phar => phar.AccountNumber == int.Parse(pharmacyId));

                            //If Pharmacy is Exists
                            if (pharmacyFromDb is not null)
                            {
                                //Change Pharmacy Status To Active If It Nonactive
                                pharmacyFromDb.PharmacyStatus = PharmacyStatus.Active;
                                pharmacyFromDb.PharmacyName = pharmacyFromExcel.PharmacyName;
                                pharmacyFromDb.Latitude = pharmacyFromExcel.Latitude;
                                pharmacyFromDb.Longitude = pharmacyFromExcel.Longitude;
                                pharmacyFromDb.SupplierId = supplierFromDb.Id;
                                existsPharmacy.Add(pharmacyFromDb);
                                //Add Pharmacy Account To PharmacyIdList To Check Nonactive Pharmacy And Make It Nonactive
                                PharmaciesIdList.Add(int.Parse(pharmacyId));
                            }
                            //If Pharmacy is Not Exists
                            else
                            {
                                pharmacyFromExcel.PharmacyRole = "Pharmacy";
                                pharmacyFromExcel.PharmacyName = pharmacyFromExcel.PharmacyName;
                                pharmacyFromExcel.Latitude = pharmacyFromExcel.Latitude;
                                pharmacyFromExcel.Longitude = pharmacyFromExcel.Longitude;
                                pharmacyFromExcel.AccountNumber = int.Parse(pharmacyId);
                                pharmacyFromExcel.UserName = pharmacyFromExcel.AccountNumber.ToString();
                                pharmacyFromExcel.SupplierId = supplierFromDb.Id;
                                var pharmacy = await _userManager.CreateAsync(pharmacyFromExcel, pharmacyFromExcel.UserName);
                                //Add Pharmacy Account To PharmacyIdList To Check Nonactive Pharmacy And Make It Nonactive
                                PharmaciesIdList.Add(int.Parse(pharmacyId));
                            }
                        }
                        catch (Exception ex)
                        {
                            //If Happened Any Exception Will  Ignore This Period And Will Be It Continue
                            continue;
                        }
                    }
                }
                //Add And Update Drugs
                if (existsPharmacy.Count != 0)
                    _db.Users.UpdateRange(existsPharmacy);
                await _db.SaveChangesAsync();
            }

            //Call Function To Change Pharmacy Status If Found Any Old Pharmacies
            await ChangePharmacyStatusToNonactive(PharmaciesIdList);

            return new ResponseViewModel<int?>(true, "Upload Excel File Succeeded", rowCount);
        }

        // Get All Customers (Return All Pharmacies)
        public async Task<ResponseViewModel<ResponseDto>> AllCustomers
            (DataTableDto pagination)
        {
            var customers = _db.Users.Where(phar => !phar.PharmacyRole.Equals("Admin") && phar.PharmacyStatus != PharmacyStatus.NonActive)
                .Select(phar => new CustomerInfoViewModel
                {
                    AccountNumber = phar.AccountNumber,
                    PharmacyName = phar.PharmacyName,
                    Latitude = phar.Latitude,
                    Longitude = phar.Longitude
                }).AsQueryable();
            if (customers is null)
            {
                return new ResponseViewModel<ResponseDto>(false, "Not Found Customers!", null);
            }
            var skip = pagination.start; //Skip
            //To Check PerPage Less Than 1 Make It Equal 10
            if (pagination.length < 1 || pagination.length > 100)
            {
                pagination.length = 10;
            }
            var pageSize = pagination.length;//Perpage
            var dataCount = customers.Count();
            var CustomersList = await customers.Skip(skip).Take(pageSize).ToListAsync();
            var CustomersListResult = new ResponseDto
            {
                data = CustomersList,
                meta = new Meta
                {
                    perpage = pageSize,
                    total = dataCount
                }
            };
            return new ResponseViewModel<ResponseDto>(true, "All Customers", CustomersListResult);
        }

        //Dispaly Admin Notifications
        public async Task<ResponseViewModel<List<AdminNotifications>>> AdminNotifications()
        {
            var adminNotifications = await _db.Notifications.Include(pharOrds => pharOrds.PharmacyOrders)
                                        .ThenInclude(phar => phar.Pharmacy)
                                        .Where(notification => notification.NotificationStatus == NotificationStatus.Created
                                               && !notification.IsRead)
                                        .OrderByDescending(notification => notification.PharmacyOrders.CreatedAt)
                                        .Select(notificate => new AdminNotifications
                                        {
                                            PharmacyName = notificate.PharmacyOrders.Pharmacy.PharmacyName,
                                            PharmacyOrdersId = notificate.PharmacyOrdersId,
                                            OrderCreateAt = notificate.PharmacyOrders.CreatedAt.ToString("MM/dd/yyyy"),
                                            TimeOrderCreateAt = notificate.PharmacyOrders.CreatedAt.ToString("hh:mm tt")
                                        }).ToListAsync();

            if (adminNotifications is null)
            {
                return new ResponseViewModel<List<AdminNotifications>>(false, "Not Found Notifications", null);
            }
            return new ResponseViewModel<List<AdminNotifications>>(true, "All Notifications", adminNotifications);
        }

        //Change Admin Notification Status To Become IsReaded
        public async Task<ResponseViewModel<bool>> ChangeAdminNotificationStatus()
        {
            var adminNotifications = await _db.Notifications.Where(notification =>
                                     notification.NotificationStatus == NotificationStatus.Created
                                     && !notification.IsRead).ToListAsync();
            //Change Notifications Status To Become IsReaded If Found Notifications
            if (adminNotifications is not null)
            {
                foreach (var notification in adminNotifications)
                {
                    notification.IsRead = true;
                }
                _db.UpdateRange(adminNotifications);
                await _db.SaveChangesAsync();
                return new ResponseViewModel<bool>(true, "Change All Notifications Succeeded", true);
            }
            return new ResponseViewModel<bool>(true, "Not Found Notifications Unread", false);
        }

        //======================= Start Generic Method ======================= :
        //Generic Method To Display Current And Archived Orders To Admin
        public async Task<ResponseViewModel<ResponseDto>> AdminCurrentAndArchivedOrders
            (PagingWithQueryDto pagination, Status status, string SuccessedProcess, string FailedProcees)
        {
            var pendingOrArchivedOrders = new List<OrderDetails>().AsQueryable();
            if (status == Status.Pending)
            {
                pendingOrArchivedOrders = _db.OrdersDetails.Include(pharOrds => pharOrds.PharmacyOrders)
                                .ThenInclude(phar => phar.Pharmacy)
                                .Include(drug => drug.Drug)
                                .Where(ordDetail => ordDetail.Status == Status.Pending)
                                .OrderByDescending(ords => ords.PharmacyOrders.CreatedAt).AsQueryable();
            }
            else
            {
                pendingOrArchivedOrders = _db.OrdersDetails.Include(pharOrds => pharOrds.PharmacyOrders)
                .ThenInclude(phar => phar.Pharmacy)
                .Include(drug => drug.Drug)
                .Where(ordDetail => ordDetail.Status != Status.Pending)
                .OrderByDescending(ords => ords.PharmacyOrders.CreatedAt).AsQueryable();
            }
            //Check If Pagination.query Is Null Create Query Object To Add Default Value
            if (pagination.query is null)
            {
                pagination.query = new Query();
            }
            //Filter By Drug Name
            if (pagination.query.DrugId != 0)
            {
                pendingOrArchivedOrders = pendingOrArchivedOrders.Where(ordDetail =>
                                ordDetail.Drug.Id == pagination.query.DrugId).AsQueryable();
                if (pendingOrArchivedOrders.Count() == 0)
                {
                    return new ResponseViewModel<ResponseDto>(false, "Not Found Data", null);
                }
            }
            //Filter By Pharmacy Name
            if (!string.IsNullOrEmpty(pagination.query.PharmacyId))
            {
                pendingOrArchivedOrders = pendingOrArchivedOrders.Where(ordDetail =>
                                ordDetail.PharmacyOrders.Pharmacy.Id ==
                                pagination.query.PharmacyId).AsQueryable();
                if (pendingOrArchivedOrders.Count() == 0)
                {
                    return new ResponseViewModel<ResponseDto>(false, "Not Found Data", null);
                }
            }
            //Filter By Orders Total Price
            if (pagination.query.MinTotalPrice != 0 && pagination.query.MaxTotalPrice != 0)
            {
                pendingOrArchivedOrders = pendingOrArchivedOrders.Where(ordDetail =>
                                ordDetail.PharmacyOrders.OrdersTotalPrice >= pagination.query.MinTotalPrice
                                && ordDetail.PharmacyOrders.OrdersTotalPrice <= pagination.query.MaxTotalPrice).AsQueryable();
                if (pendingOrArchivedOrders.Count() == 0)
                {
                    return new ResponseViewModel<ResponseDto>(false, "Not Found Data", null);
                }
            }
            //Filter By Date (From To) 
            if (pagination.query.From is not null && pagination.query.To is not null)
            {
                pendingOrArchivedOrders = pendingOrArchivedOrders.Where(ordDetail =>
                                    ordDetail.PharmacyOrders.CreatedAt.Date >= pagination.query.From
                                    && ordDetail.PharmacyOrders.CreatedAt.Date <= pagination.query.To).AsQueryable();
                if (pendingOrArchivedOrders.Count() == 0)
                {
                    return new ResponseViewModel<ResponseDto>(false, "Not Found Data", null);
                }
            }
            //Filter By Date (Month Name)
            if (!string.IsNullOrEmpty(pagination.query.MonthName))
            {
                int monthNum = GetMonthNumber(pagination.query.MonthName);
                pendingOrArchivedOrders = pendingOrArchivedOrders.Where(ordDetail =>
                                ordDetail.PharmacyOrders.CreatedAt.Month == monthNum).AsQueryable();
                if (pendingOrArchivedOrders.Count() == 0)
                {
                    return new ResponseViewModel<ResponseDto>(false, "Not Found Data", null);
                }
            }
            //Filter By Date (Period Of Time)
            if (!string.IsNullOrEmpty(pagination.query.PeriodOfTime))
            {
                var PeriodOfTimeNum = GetPeriodOfTimeNumber(pagination.query.PeriodOfTime);
                if (PeriodOfTimeNum == 0)
                {
                    return new ResponseViewModel<ResponseDto>(false,
                            "Pelase Search By: 'last week' Or 'last month' Or 'last 45 days' ", null);
                }
                var period = DateTime.Now.AddDays(-PeriodOfTimeNum).Date;
                pendingOrArchivedOrders = pendingOrArchivedOrders.Where(ordDetail => ordDetail.PharmacyOrders.CreatedAt.Date >= period
                                    && ordDetail.PharmacyOrders.CreatedAt.Date <= DateTime.Now.Date).AsQueryable();
                if (pendingOrArchivedOrders.Count() == 0)
                {
                    return new ResponseViewModel<ResponseDto>(false, "Not Found Data", null);
                }
            }
            //Filter By Status
            if (pagination.query.Status != 0)
            {
                pendingOrArchivedOrders = pendingOrArchivedOrders.Where(ordDetail =>
                                ordDetail.Status == pagination.query.Status).AsQueryable();
                if (pendingOrArchivedOrders.Count() == 0)
                {
                    return new ResponseViewModel<ResponseDto>(false, "Not Found Data", null);
                }
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
                ord.PharmacyOrders.Pharmacy.PharmacyName,
                ord.PharmacyOrdersId
            }).Select(ords => new AdminGroupingByResult
            {
                Key = new AdminKey
                {
                    CreateAt = ords.Key.Date.ToString("yyyy-MM-dd"),
                    PharmacyName = ords.Key.PharmacyName,
                    PharmacyOrdersId = ords.Key.PharmacyOrdersId
                },
                OrdersDetails = ords.ToList()
            })
            .ToList();
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

        // Switch Case To Return Month Number
        public int GetMonthNumber(string MonthName)
        {
            int MonthNumber = 0;
            MonthName = MonthName.ToLower();
            switch (MonthName)
            {
                case "january":
                    MonthNumber = 1;
                    break;
                case "february":
                    MonthNumber = 2;
                    break;
                case "march":
                    MonthNumber = 3;
                    break;
                case "april":
                    MonthNumber = 4;
                    break;
                case "may":
                    MonthNumber = 5;
                    break;
                case "june":
                    MonthNumber = 6;
                    break;
                case "july":
                    MonthNumber = 7;
                    break;
                case "august":
                    MonthNumber = 8;
                    break;
                case "september":
                    MonthNumber = 9;
                    break;
                case "october":
                    MonthNumber = 10;
                    break;
                case "november":
                    MonthNumber = 11;
                    break;
                case "december":
                    MonthNumber = 12;
                    break;
            }
            return MonthNumber;
        }

        // Switch Case To Return Month Number
        public int GetPeriodOfTimeNumber(string PeriodOfTime)
        {
            int PeriodOfTimeNumber = 0;
            PeriodOfTime = PeriodOfTime.ToLower();
            if (PeriodOfTime.Contains("last week"))
                return PeriodOfTimeNumber = 7;
            else if (PeriodOfTime.Contains("last month"))
                return PeriodOfTimeNumber = 30;
            else if (PeriodOfTime.Contains("last 45 days"))
                return PeriodOfTimeNumber = 45;
            else
                return PeriodOfTimeNumber;
        }

        //Filter Generic Method
        public IQueryable<OrderDetails> GenericFilter(IQueryable<OrderDetails> ordersDetails, Query query)
        {
            //Check If Query Is Null
            if (query is null)
            {
                return null;
            }

            //Filter By Drug Name
            if (query.DrugId != 0)
            {
                ordersDetails = ordersDetails.Where(ordDetail =>
                                ordDetail.Drug.Id == query.DrugId).AsQueryable();
                if (ordersDetails.Count() == 0)
                {
                    return null;
                }
            }
            //Filter By Pharmacy Name
            if (!string.IsNullOrEmpty(query.PharmacyId))
            {
                ordersDetails = ordersDetails.Where(ordDetail =>
                                ordDetail.PharmacyOrders.Pharmacy.Id ==
                                query.PharmacyId).AsQueryable();
                if (ordersDetails.Count() == 0)
                {
                    return null;
                }
            }
            //Filter By Orders Total Price
            if (query.MinTotalPrice != 0 && query.MaxTotalPrice != 0)
            {
                ordersDetails = ordersDetails.Where(ordDetail =>
                                ordDetail.PharmacyOrders.OrdersTotalPrice >= query.MinTotalPrice
                                && ordDetail.PharmacyOrders.OrdersTotalPrice <= query.MaxTotalPrice).AsQueryable();
                if (ordersDetails.Count() == 0)
                {
                    return null;
                }
            }
            //Filter By Date (From To) 
            if (query.From is not null && query.To is not null)
            {
                ordersDetails = ordersDetails.Where(ordDetail =>
                                    ordDetail.PharmacyOrders.CreatedAt.Date >= query.From
                                    && ordDetail.PharmacyOrders.CreatedAt.Date <= query.To).AsQueryable();
                if (ordersDetails.Count() == 0)
                {
                    return null;
                }
            }
            //Filter By Date (Month Name)
            if (!string.IsNullOrEmpty(query.MonthName))
            {
                int monthNum = GetMonthNumber(query.MonthName);
                ordersDetails = ordersDetails.Where(ordDetail =>
                                ordDetail.PharmacyOrders.CreatedAt.Month == monthNum).AsQueryable();
                if (ordersDetails.Count() == 0)
                {
                    return null;
                }
            }
            //Filter By Date (Period Of Time)
            if (!string.IsNullOrEmpty(query.PeriodOfTime))
            {
                var PeriodOfTimeNum = GetPeriodOfTimeNumber(query.PeriodOfTime);
                if (PeriodOfTimeNum == 0)
                {
                    return null;
                }
                var period = DateTime.Now.AddDays(-PeriodOfTimeNum).Date;
                ordersDetails = ordersDetails.Where(ordDetail => ordDetail.PharmacyOrders.CreatedAt.Date >= period
                                    && ordDetail.PharmacyOrders.CreatedAt.Date <= DateTime.Now.Date).AsQueryable();
                if (ordersDetails.Count() == 0)
                {
                    return null;
                }
            }
            //Filter By Status
            if (query.Status != 0)
            {
                ordersDetails = ordersDetails.Where(ordDetail =>
                                ordDetail.Status == query.Status).AsQueryable();
                if (ordersDetails.Count() == 0)
                {
                    return null;
                }
            }
            return ordersDetails;
        }

        // ======================= End Generic Method ======================= 

        //Change Drug Status From Active To NonActive
        public async Task<ResponseViewModel<bool>> ChangeDrugStatusToNonActive(List<int> DrugsId)
        {
            //Return All Drugs Not Found In Last Excel Updated
            var drugs = await _db.Drugs.Where(drug => !DrugsId.Contains(drug.DrugId ?? 0)).ToListAsync();

            if (drugs is null || drugs.Count == 0)
            {
                return new ResponseViewModel<bool>(false, "Not Found Drugs To Deleted", false);
            }
            //Change Drug Status To NonActive
            foreach (var drug in drugs)
            {
                drug.DrugStatus = DrugStatus.NonActive;
            }
            _db.Drugs.UpdateRange(drugs);
            await _db.SaveChangesAsync();
            return new ResponseViewModel<bool>(true, "Change Drug Status Succeeded", true);
        }

        //Change All Pending Orders To Hanging If Found Pending Order When Admin Upload Drug Excel To Denied Any Update
        public async Task ChangeOrderStatusToHanging()
        {
            // Return All Pending Orders
            var pendingOrders = await _db.OrdersDetails.Where(ord => ord.Status == Status.Pending).ToListAsync();

            //Check If Found Any Pending Orders Change to Hanging
            if (pendingOrders.Count != 0)
            {
                foreach (var order in pendingOrders)
                {
                    order.IsHanging = true;
                }
                _db.OrdersDetails.UpdateRange(pendingOrders);
                await _db.SaveChangesAsync();
            }
        }

        //Change Pharmacies Not Found In Location Excel To Nonactive (Safe Delete And Denied Login)
        public async Task ChangePharmacyStatusToNonactive(List<int> PharmaciesId)
        {
            //Return All Pharmacies Not Found In Last Excel Updated
            var pharmacies = await _db.Users.Where(phar => !PharmaciesId.Contains(phar.AccountNumber ?? 0)
                                                        && !phar.PharmacyRole.Equals("Admin")).ToListAsync();

            //If Found Any Old Pharmacies Will Be It To Nonactive
            if (pharmacies.Count != 0)
            {
                //Change Pharmacy Status To NonActive
                foreach (var pharmacy in pharmacies)
                {
                    pharmacy.PharmacyStatus = PharmacyStatus.NonActive;
                }
                _db.Users.UpdateRange(pharmacies);
                await _db.SaveChangesAsync();
            }
        }

        //==========================Code Mohammed================================

        public async Task<DataTableOutput<List<GetOrdersDataTable>>> GetAdminCurrentOrders
            (DataTableDto dto, int pharmacyOrderId)
        {
            var pageSize = dto.length;
            var skip = dto.start;

            int? drugId = dto.DataTableFilterDto.DrugId;
            var pharmacyId = dto.DataTableFilterDto.PharmacyId;

            var OrdersDetails = _db.OrdersDetails.Include(O => O.Drug).Include(PO => PO.PharmacyOrders)
                .ThenInclude(Ph => Ph.Pharmacy)
                .Where(a => pharmacyOrderId == 0 ? true : a.PharmacyOrdersId.Equals(pharmacyOrderId))
                .Where(a => a.Status == Status.Pending)
                .Where(m => drugId == 0 ? true : m.Drug.Id == drugId)
                .Where(m => string.IsNullOrEmpty(pharmacyId) ? true : m.PharmacyOrders.Pharmacy.Id.Contains(pharmacyId))
                 .Where(m => (dto.DataTableFilterDto.MinTotalPrice == 0 && dto.DataTableFilterDto.MaxTotalPrice == 0) ? true : (m.PharmacyOrders.OrdersTotalPrice >= dto.DataTableFilterDto.MinTotalPrice && m.PharmacyOrders.OrdersTotalPrice <= dto.DataTableFilterDto.MaxTotalPrice))
                .Where(m => (dto.DataTableFilterDto.From == null && dto.DataTableFilterDto.To == null) ? true : (m.PharmacyOrders.CreatedAt.Date >= dto.DataTableFilterDto.From && m.PharmacyOrders.CreatedAt.Date <= dto.DataTableFilterDto.To))
                .Select(x => new GetOrdersDataTable
                {
                    CreatedAt = x.PharmacyOrders.CreatedAt,
                    CreatedAtString = x.PharmacyOrders.CreatedAt.ToString("MM/dd/yyyy HH:mm:ss"),
                    OrderId = x.Id,
                    PharmacyOrderId = x.PharmacyOrdersId,
                    DrugId = x.Drug.DrugId,
                    DrugName = x.Drug.DrugName,
                    Quantity = x.Quantity,
                    Stock = x.Drug.Stock,
                    PricePerUnit = x.UnitPrice,
                    TotalPrice = x.UnitPrice * x.Quantity,
                    PharmacyName = x.PharmacyOrders.Pharmacy.PharmacyName,
                    PharmacyId = x.PharmacyOrders.Pharmacy.Id,
                    TotalPricePharmacyOrders = x.PharmacyOrders.Orders.Where(x => x.Status == Status.Pending)
                                                        .Sum(x => x.Quantity * x.UnitPrice)
                }).OrderByDescending(a => a.PharmacyOrderId);




            var data = await OrdersDetails.Skip(skip).Take(pageSize).ToListAsync();

            var recordsTotal = await OrdersDetails.CountAsync();

            return new DataTableOutput<List<GetOrdersDataTable>>(recordsTotal, recordsTotal, data);
        }

        public async Task<DataTableOutput<List<GetOrdersDataTable>>> GetAdminArchivedOrders(DataTableDto dto)
        {
            var pageSize = dto.length;
            var skip = dto.start;

            int? drugId = dto.DataTableFilterDto.DrugId;
            var pharmacyId = dto.DataTableFilterDto.PharmacyId;

            var OrdersDetails = _db.OrdersDetails.Include(O => O.Drug).Include(PO => PO.PharmacyOrders)
                .ThenInclude(Ph => Ph.Pharmacy)
                .Where(a => a.Status != Status.Pending)
                .Where(m => drugId == 0 ? true : m.Drug.Id == drugId)
                .Where(m => string.IsNullOrEmpty(pharmacyId) ? true : m.PharmacyOrders.Pharmacy.Id.Contains(pharmacyId))
                .Where(m => (dto.DataTableFilterDto.MinTotalPrice == 0 && dto.DataTableFilterDto.MaxTotalPrice == 0) ? true : (m.PharmacyOrders.OrdersTotalPrice >= dto.DataTableFilterDto.MinTotalPrice && m.PharmacyOrders.OrdersTotalPrice <= dto.DataTableFilterDto.MaxTotalPrice))
                .Where(m => (dto.DataTableFilterDto.From == null && dto.DataTableFilterDto.To == null) ? true : (m.PharmacyOrders.CreatedAt.Date >= dto.DataTableFilterDto.From && m.PharmacyOrders.CreatedAt.Date <= dto.DataTableFilterDto.To))
                .Select(x => new GetOrdersDataTable
                {
                    CreatedAt = x.PharmacyOrders.CreatedAt,
                    CreatedAtString = x.PharmacyOrders.CreatedAt.ToString("MM/dd/yyyy HH:mm:ss"),
                    OrderId = x.Id,
                    PharmacyOrderId = x.PharmacyOrdersId,
                    DrugId = x.Drug.DrugId,
                    DrugName = x.Drug.DrugName,
                    Quantity = x.Quantity,
                    PricePerUnit = x.UnitPrice,
                    TotalPrice = x.UnitPrice * x.Quantity,
                    PharmacyName = x.PharmacyOrders.Pharmacy.PharmacyName,
                    PharmacyId = x.PharmacyOrders.Pharmacy.Id,
                    ArchivedTotalPricePharmacyOrders = x.PharmacyOrders.Orders.Where(x => x.Status == Status.Completed)
                                                        .Sum(x => x.Quantity * x.UnitPrice),
                    Status = x.Status
                }).OrderByDescending(a => a.PharmacyOrderId);

            var data = await OrdersDetails.Skip(skip).Take(pageSize).ToListAsync();

            var recordsTotal = await OrdersDetails.CountAsync();

            return new DataTableOutput<List<GetOrdersDataTable>>(recordsTotal, recordsTotal, data);
        }

    }
}
