using CapManagement.Client.Pages.Driver.Driver;
using CapManagement.Server.IRepository;
using CapManagement.Server.IService;
using CapManagement.Server.Repository;
using CapManagement.Shared;
using CapManagement.Shared.DtoModels.ContractDto;
using CapManagement.Shared.DtoModels.ExpenseDtoModels;
using CapManagement.Shared.Models;

namespace CapManagement.Server.Services
{
    public class ExpenseService : IExpenseService
    {

        private readonly IExpenseRepository _expenseRepository;

        public ExpenseService(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }


        /// <summary>
        /// Archives (soft-deletes) a specific expense record for a given company.
        /// </summary>
        /// <param name="expenseId">
        /// The unique identifier of the expense to archive.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the expense.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{Boolean}"/> indicating whether the expense was successfully archived.
        /// </returns>
        /// <remarks>
        /// This method performs a soft delete by marking the expense as archived rather than removing it permanently.
        /// If the expense does not exist or is already archived, the operation will fail gracefully.
        /// </remarks>

        public async Task<ApiResponse<bool>> ArchiveExpenseAsync(Guid expenseId, Guid companyId)
        {
            var response = new ApiResponse<bool>();
            try
            {
                var result = await _expenseRepository.ArchiveExpenseAsync(expenseId, companyId);

                if (!result)
                {
                    response.Success = false;
                    response.Data = false;
                    response.Errors.Add("expense not found or already archived.");
                    return response;
                }

                response.Success = true;
                response.Data = true;
                response.Message = "expense archived successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Data = false;
                response.Errors.Add($"Error archiving expense: {ex.Message}");
            }

            return response;
        }




        /// <summary>
        /// Creates a new expense record for a company and associates it with a car and/or contract.
        /// </summary>
        /// <param name="expenseDto">
        /// The data transfer object containing expense details such as amount, type, VAT, and related entities.
        /// </param>
        /// <param name="userId">
        /// The identifier of the user creating the expense record.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{ExpenseDto}"/> containing the created expense if successful,
        /// or validation errors if the operation fails.
        /// </returns>
        /// <remarks>
        /// This method validates the input data, maps the DTO to a domain entity,
        /// persists the expense via the repository, and returns the created record.
        /// It supports financial tracking and VAT reporting through structured expense storage.
        /// </remarks>
        public async Task<ApiResponse<ExpenseDto>> CreateExpenseAsync(CreateExpenseDto expenseDto, string userId)
        {
            var response = new ApiResponse<ExpenseDto>();

            try
            {
                // ---------- NULL CHECK ----------
                if (expenseDto == null)
                {
                    response.Success = false;
                    response.Message = "Expense data cannot be null.";
                    return response;
                }

                // ---------- VALIDATION ----------
                if (expenseDto.CompanyId == Guid.Empty)
                {
                    response.Success = false;
                    response.Message = "Company ID is required.";
                    return response;
                }

                if (expenseDto.CarId == Guid.Empty)
                {
                    response.Success = false;
                    response.Message = "Car ID is required.";
                    return response;
                }

                if (expenseDto.Amount <= 0)
                {
                    response.Success = false;
                    response.Message = "Amount must be greater than zero.";
                    return response;
                }

                // ---------- MAP DTO → ENTITY ----------
                var expense = new Expense
                {
                    ExpenseId = Guid.NewGuid(),
                    CarId = expenseDto.CarId,
                    CompanyId = expenseDto.CompanyId,
                    Type = expenseDto.Type,
                    Amount = expenseDto.Amount,
                    VatPercent = expenseDto.VatPercent,
                    ExpenseDate = expenseDto.ExpenseDate
                };

                // ---------- CALL REPOSITORY ----------
                var createdEntity = await _expenseRepository.CreateExpenseAsync(expense);

                if (createdEntity == null)
                {
                    response.Success = false;
                    response.Message = "Failed to create expense.";
                    return response;
                }

                // ---------- MAP ENTITY → DTO ----------
                var resultDto = new ExpenseDto
                {
                    ExpenseId = createdEntity.ExpenseId,
                    CarId = createdEntity.CarId,
                    CarName = createdEntity.Car?.Brand,   // if included in repo
                    CompanyId = createdEntity.CompanyId,
                    Type = createdEntity.Type,
                    Amount = createdEntity.Amount,
                    VatPercent = createdEntity.VatPercent,
                    VatAmount = createdEntity.VatAmount,     // computed
                    NetAmount = createdEntity.NetAmount,     // computed
                    ExpenseDate = createdEntity.ExpenseDate,
                    Quarter = createdEntity.Quarter
                };

                response.Success = true;
                response.Message = "Expense created successfully.";
                response.Data = resultDto;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating expense: {ex.Message}";
                return response;
            }
        }




        /// <summary>
        /// Retrieves a paginated list of expenses for a specific company with optional sorting and filtering.
        /// </summary>
        /// <param name="pageNumber">
        /// The page number to retrieve (1-based index).
        /// </param>
        /// <param name="pageSize">
        /// The number of records to include per page.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company whose expenses are being retrieved.
        /// </param>
        /// <param name="orderBy">
        /// Optional field name used to sort the results (e.g., date, amount).
        /// </param>
        /// <param name="filter">
        /// Optional filter criteria used to narrow down the result set (e.g., expense type, car).
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{PagedResponse{ExpenseDto}}"/> containing a paginated list of expenses
        /// including VAT and net amount details.
        /// </returns>
        /// <remarks>
        /// This method supports pagination, sorting, and filtering to efficiently browse company expenses.
        /// It is designed for financial reporting, auditing, and VAT analysis.
        /// </remarks>
        public async Task<ApiResponse<PagedResponse<ExpenseDto>>> GetAllExpenseAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            var response = new ApiResponse<PagedResponse<ExpenseDto>>();

            var pagedResult = await _expenseRepository.GetAllExpensesAsync(pageNumber, pageSize, companyId, orderBy, filter);

            var pagedResponse = new PagedResponse<ExpenseDto>
            {
                Data = pagedResult.Items.Select(e => new ExpenseDto
                {
                    ExpenseId = e.ExpenseId,
                    CarId = e.CarId,
                    CarName = e.Car != null ? e.Car.Brand : "",
                    NumberPlate = e.Car?.NumberPlate,
                    CompanyId = e.CompanyId,
                    Type = e.Type,                       // ExpenseType enum
                    Amount = e.Amount,                   // total incl VAT
                    VatPercent = e.VatPercent,
                    VatAmount = e.VatAmount,             // computed in entity
                    NetAmount = e.NetAmount,             // computed in entity
                    ExpenseDate = e.ExpenseDate,
                    Quarter = e.Quarter
                }).ToList(),
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalPages = (int)Math.Ceiling(pagedResult.TotalCount / (double)pagedResult.PageSize),
                TotalItems = pagedResult.TotalCount
            };

            response.Success = true;
            response.Data = pagedResponse;
            return response;
        }



        /// <summary>
        /// Retrieves a paginated list of archived (inactive) expenses for a specific company,
        /// with optional sorting and filtering support.
        /// </summary>
        /// <param name="pageNumber">
        /// The page number to retrieve (1-based index).
        /// </param>
        /// <param name="pageSize">
        /// The number of archived expense records to include per page.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company whose archived expenses are being retrieved.
        /// </param>
        /// <param name="orderBy">
        /// Optional field name used to sort the archived expenses (e.g., date, amount, type).
        /// </param>
        /// <param name="filter">
        /// Optional filter criteria used to narrow down the archived expenses
        /// (e.g., expense type, car number plate).
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{PagedResponse{ExpenseDto}}"/> containing a paginated list of archived expenses,
        /// including VAT and net amount details.
        /// </returns>
        /// <remarks>
        /// Archived expenses are no longer active but are preserved for auditing, reporting,
        /// and compliance purposes. This method is optimized for financial reviews and history tracking.
        /// </remarks>
        public async Task<ApiResponse<PagedResponse<ExpenseDto>>> GetArchivedExpensesAsync(int pageNumber, int pageSize, Guid companyId, string? orderBy, string? filter)
        {
            var response = new ApiResponse<PagedResponse<ExpenseDto>>();

            var pagedResult = await _expenseRepository.GetAllArchivedExpenseAsync(pageNumber, pageSize, companyId, orderBy, filter);

            var pagedResponse = new PagedResponse<ExpenseDto>
            {
                Data = pagedResult.Items.Select(e => new ExpenseDto
                {
                    ExpenseId = e.ExpenseId,
                    CarId = e.CarId,
                    CarName = e.Car != null ? e.Car.Brand : "",
                    NumberPlate = e.Car?.NumberPlate,
                    CompanyId = e.CompanyId,
                    Type = e.Type,                       // ExpenseType enum
                    Amount = e.Amount,                   // total incl VAT
                    VatPercent = e.VatPercent,
                    VatAmount = e.VatAmount,             // computed in entity
                    NetAmount = e.NetAmount,             // computed in entity
                    ExpenseDate = e.ExpenseDate,
                    Quarter = e.Quarter
                }).ToList(),
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalPages = (int)Math.Ceiling(pagedResult.TotalCount / (double)pagedResult.PageSize),
                TotalItems = pagedResult.TotalCount
            };

            response.Success = true;
            response.Data = pagedResponse;
            return response;
        }



        /// <summary>
        /// Retrieves a single expense record by its unique identifier for a given company.
        /// </summary>
        /// <param name="expenseId">
        /// The unique identifier of the expense to retrieve.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the expense.
        /// </param>
        /// <returns>
        /// An <see cref="ExpenseDto"/> containing the expense details if found; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="expenseId"/> or <paramref name="companyId"/> is empty.
        /// </exception>
        /// <remarks>
        /// This method validates the input parameters, retrieves the expense from the repository,
        /// and maps the domain entity to a DTO, including VAT and net amount calculations.
        /// </remarks>

        public async Task<ExpenseDto> GetExpenseByIdAsync(Guid expenseId, Guid companyId)
        {
            // ---------- VALIDATION ----------
            if (expenseId == Guid.Empty)
                throw new ArgumentException("Expense ID cannot be empty.", nameof(expenseId));

            if (companyId == Guid.Empty)
                throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

            // ---------- CALL REPOSITORY ----------
            var expense = await _expenseRepository.GetExpenseByIdAsync(expenseId, companyId);

            if (expense == null)
                return null;

            // ---------- MAP ENTITY → DTO ----------
            var dto = new ExpenseDto
            {
                ExpenseId = expense.ExpenseId,
                CarId = expense.CarId,
                CarName = expense.Car?.Brand,   // only if repo includes Car navigation
                CompanyId = expense.CompanyId,
                Type = expense.Type,
                Amount = expense.Amount,
                VatPercent = expense.VatPercent,
                VatAmount = expense.VatAmount,    // computed in entity
                NetAmount = expense.NetAmount,    // computed in entity
                ExpenseDate = expense.ExpenseDate,
                Quarter = expense.Quarter
            };

            return dto;
        }


        /// <summary>
        /// Retrieves a paginated list of expenses associated with a specific car for a given company.
        /// </summary>
        /// <param name="carId">
        /// The unique identifier of the car whose expenses are being retrieved.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the car and expenses.
        /// </param>
        /// <param name="pageNumber">
        /// The page number to retrieve (1-based index).
        /// </param>
        /// <param name="pageSize">
        /// The number of expense records to include per page.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{PageResult{ExpenseDto}}"/> containing a paginated list of expenses
        /// for the specified car, including VAT and net amount details.
        /// </returns>
        /// <remarks>
        /// This method validates ownership (car and company), retrieves expenses from the repository,
        /// and maps domain entities to DTOs. It is primarily used for car-level financial tracking
        /// and reporting.
        /// </remarks>

        public async Task<ApiResponse<PageResult<ExpenseDto>>> GetExpensesByCarIdAsync(Guid carId, Guid companyId, int pageNumber, int pageSize)
        {
            var response = new ApiResponse<PageResult<ExpenseDto>>();

            if (carId == Guid.Empty || companyId == Guid.Empty)
            {
                response.Success = false;
                response.Message = "Invalid car or company ID.";
                return response;
            }

            try
            {
                var pageResult = await _expenseRepository.GetExpensesByCarIdAsync(
                    carId,
                    companyId,
                    pageNumber,
                    pageSize);

                // ✅ Manual mapping (later AutoMapper)
                var dtoPage = new PageResult<ExpenseDto>
                {
                    TotalCount = pageResult.TotalCount,
                    Items = pageResult.Items.Select(e => new ExpenseDto
                    {
                        ExpenseId = e.ExpenseId,
                        CarId = e.CarId,
                        CompanyId = e.CompanyId,
                        Type = e.Type,
                        Amount = e.Amount,
                        VatPercent = e.VatPercent,
                        VatAmount = e.VatAmount,
                        NetAmount = e.NetAmount,
                        ExpenseDate = e.ExpenseDate,
                        Quarter = e.Quarter,
                        CarName = e.Car?.Brand,
                        NumberPlate = e.Car?.NumberPlate
                    }).ToList()
                };

                response.Success = true;
                response.Data = dtoPage;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error loading expenses: {ex.Message}";
                return response;
            }
        }




        /// <summary>
        /// Restores a previously archived (soft-deleted) expense record for a specific company.
        /// </summary>
        /// <param name="expenseId">
        /// The unique identifier of the expense to restore.
        /// </param>
        /// <param name="companyId">
        /// The unique identifier of the company that owns the expense.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{Boolean}"/> indicating whether the expense was successfully restored.
        /// </returns>
        /// <remarks>
        /// This method reverses the soft-delete operation by marking the expense as active again.
        /// It is typically used for correcting accidental deletions or restoring historical financial records.
        /// </remarks>
        public async Task<ApiResponse<bool>> RestoreExpenseAsync(Guid expenseId, Guid companyId)
        {
            var response = new ApiResponse<bool>();

            try
            {
                var result = await _expenseRepository.RestoreExpenseAsync(expenseId, companyId);

                if (!result)
                {
                    response.Success = false;
                    response.Errors = new List<string> { "expense not found or failed to restore." };
                    return response;
                }

                response.Success = true;
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Errors = new List<string> { "An error occurred while restoring the driver.", ex.Message };
                // Optional: log the exception here
            }

            return response;
        }




        /// <summary>
        /// Updates an existing expense record for a specific company.
        /// </summary>
        /// <param name="UpdateExpenseDto">
        /// A DTO containing the updated expense data.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse{Boolean}"/> indicating whether the expense was successfully updated.
        /// </returns>
        /// <remarks>
        /// This method validates the input DTO, retrieves the existing expense from the repository,
        /// applies updated values, and persists the changes.
        /// Business rules such as VAT and net amount calculations are handled at the entity level.
        /// </remarks>
        public async Task<ApiResponse<bool>> UpdateExpenseAsync(UpdateExpenseDto UpdateExpenseDto)
        {
            var response = new ApiResponse<bool>();

            // 🔒 Validations
            if (UpdateExpenseDto == null)
            {
                response.Success = false;
                response.Message = "Expense data is required.";
                return response;
            }

            if (UpdateExpenseDto.ExpenseId == Guid.Empty)
            {
                response.Success = false;
                response.Message = "Expense ID cannot be empty.";
                return response;
            }

            

            try
            {
                // 🔍 Fetch existing expense
                var existingExpense = await _expenseRepository
                    .GetExpenseByIdAsync(UpdateExpenseDto.ExpenseId, UpdateExpenseDto.CompanyId);


                if (existingExpense == null)
                {
                    response.Success = false;
                    response.Message = "Expense not found.";
                    return response;
                }

                // ✏️ Update properties (MAPPING IN SERVICE)
                existingExpense.CarId = UpdateExpenseDto.CarId;
                existingExpense.Type = UpdateExpenseDto.Type;
                existingExpense.Amount = UpdateExpenseDto.Amount;
                existingExpense.VatPercent = UpdateExpenseDto.VatPercent;
                existingExpense.ExpenseDate = UpdateExpenseDto.ExpenseDate;

                // 🧠 Business logic
                //existingExpense.Quarter = (UpdateExpenseDto.ExpenseDate.Month - 1) / 3 + 1;
                //existingExpense.VatAmount = UpdateExpenseDto.Amount * UpdateExpenseDto.VatPercent / 100;
                //existingExpense.NetAmount = UpdateExpenseDto.Amount - existingExpense.VatAmount;
                existingExpense.UpdatedAt = DateTime.UtcNow;

                // 💾 Save via repository
                var updated = await _expenseRepository.UpdateExpenseAsync(existingExpense);

                response.Success = updated;
                response.Data = updated;
                response.Message = updated
                    ? "Expense updated successfully."
                    : "Failed to update expense.";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating expense: {ex.Message}";
                return response;
            }
        }
    }
}
