using CapManagement.Shared.DtoModels.SettlementDtoModels;
using CapManagement.Shared.DtoModels.EarningDtoModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CapManagement.Server.Services
{
    public class PdfService
    {

        /// <summary>
        /// Generates a PDF settlement report based on the provided settlement data.
        /// </summary>
        /// <param name="s">
        /// The settlement data transfer object containing financial and earnings information.
        /// </param>
        /// <returns>
        /// A byte array representing the generated PDF document.
        /// </returns>
        /// <remarks>
        /// The generated PDF includes:
        /// <list type="bullet">
        /// <item><description>Settlement header and identification</description></item>
        /// <item><description>Settlement period and status information</description></item>
        /// <item><description>Financial summary (gross, deductions, net payout)</description></item>
        /// <item><description>Detailed earnings breakdown</description></item>
        /// </list>
        /// This method uses QuestPDF to construct and render the document layout.
        /// </remarks>
        public byte[] GenerateSettlmentPdf(SettlementDto s)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(35);
                    page.Size(PageSizes.A4);

                    // ---------------------------------------------------
                    // HEADER
                    // ---------------------------------------------------
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Settlement Report")
                            .Bold()
                            .FontSize(22)
                            .FontColor(Colors.Blue.Medium);

                        col.Item().Text($"Settlement ID: {s.SettlementId}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    // ---------------------------------------------------
                    // CONTENT
                    // ---------------------------------------------------
                    page.Content().Column(col =>
                    {
                        col.Spacing(20);

                        // ---------------------- Settlement Info ----------------------
                        col.Item().Text("Settlement Information")
                            .Bold().FontSize(14).Underline();

                        col.Item().Column(info =>
                        {
                            info.Item().Text($"Company ID: {s.CompanyId}");
                            info.Item().Text($"Period Start: {s.PeriodStart:dd/MM/yyyy}");
                            info.Item().Text($"Period End: {s.PeriodEnd:dd/MM/yyyy}");
                            info.Item().Text($"Status: {s.Status}");
                            info.Item().Text($"Confirmed By Driver: {(s.ConfirmedByDriver ? "Yes" : "No")}");
                            info.Item().Text($"Confirmed At: {s.ConfirmedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A"}");
                            info.Item().Text($"Description: {s.Description ?? "N/A"}");
                        });

                        // ---------------------- Financial Summary ----------------------
                        col.Item().Text("Financial Summary")
                            .Bold().FontSize(14).Underline();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(160);
                                cols.RelativeColumn();
                            });

                            AddRow(table, "Gross Amount", s.GrossAmount.ToString("C"));
                            AddRow(table, "Rent Deduction", s.RentDeduction.ToString("C"));
                            AddRow(table, "Extra Costs", s.ExtraCosts.ToString("C"));

                            // Highlight final
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Net Payout").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(s.NetPayout.ToString("C")).SemiBold();
                        });

                        // ---------------------- Earnings Table ----------------------
                        if (s.Earnings != null && s.Earnings.Any())
                        {
                            col.Item().Text("Earnings Breakdown")
                                .Bold().FontSize(14).Underline();

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(80);   // Date
                                    cols.ConstantColumn(100);  // Platform
                                    cols.ConstantColumn(90);   // Week Start
                                    cols.ConstantColumn(90);   // Week End
                                    cols.RelativeColumn();     // Gross Income
                                    cols.RelativeColumn();     // BTW Amount
                                    cols.RelativeColumn();     // Net Income
                                });

                                // Header row
                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCell).Text("Date");
                                    header.Cell().Element(HeaderCell).Text("Platform");
                                    header.Cell().Element(HeaderCell).Text("Week Start");
                                    header.Cell().Element(HeaderCell).Text("Week End");
                                    header.Cell().Element(HeaderCell).Text("Gross");
                                    header.Cell().Element(HeaderCell).Text("BTW");
                                    header.Cell().Element(HeaderCell).Text("Net");
                                });

                                foreach (var e in s.Earnings.OrderBy(x => x.IncomeDate))
                                {
                                    table.Cell().Padding(4).Text(e.IncomeDate.ToString("dd/MM"));
                                    table.Cell().Padding(4).Text(e.Platform.ToString());
                                    table.Cell().Padding(4).Text(e.WeekStart.ToString("dd/MM"));
                                    table.Cell().Padding(4).Text(e.WeekEnd.ToString("dd/MM"));
                                    table.Cell().Padding(4).Text(e.GrossIncome.ToString("C"));
                                    table.Cell().Padding(4).Text(e.BtwAmount.ToString("C"));
                                    table.Cell().Padding(4).Text(e.NetIncome.ToString("C"));
                                }
                            });

                            // Totals block remains unchanged...

                            // ---------------------- Totals ----------------------
                            col.Item().PaddingTop(10).Text("Earnings Totals")
                                .Bold().FontSize(13);

                            var totalGross = s.Earnings.Sum(x => x.GrossIncome);
                            var totalBtw = s.Earnings.Sum(x => x.BtwAmount);
                            var totalNet = s.Earnings.Sum(x => x.NetIncome);

                            col.Item().Text($"Total Gross Income: {totalGross:C}");
                            col.Item().Text($"Total BTW Amount: {totalBtw:C}");
                            col.Item().Text($"Total Net Income: {totalNet:C}");
                        }
                        else
                        {
                            col.Item().Text("No earnings data available.")
                                .Italic().FontColor(Colors.Grey.Darken1);
                        }
                    });

                    // ---------------------------------------------------
                    // FOOTER
                    // ---------------------------------------------------
                    //page.Footer().AlignCenter().Text(text =>
                    //{
                    //    text.Span("Generated on ").SemiBold();
                    //    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    //    text.Span(" | Page ");
                    //    text.CurrentPageNumber();
                    //}).FontSize(10).FontColor(Colors.Grey.Darken2);
                });
            });

            return document.GeneratePdf();
        }

        // Helper for normal rows
        private static void AddRow(TableDescriptor table, string key, string value)
        {
            table.Cell().Padding(4).Text(key).SemiBold();
            table.Cell().Padding(4).Text(value);
        }

        // Helper for header cells
        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Padding(4)
                .Background(Colors.Grey.Lighten2)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten1);
        }
    }
}
