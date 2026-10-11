using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using OrderModel = CarRentoWeb.Models.Order;

namespace CarRentoWeb.Services
{
    public interface IOrderPdfService
    {
        byte[] Generate(OrderModel order);
    }

    public class OrderPdfService : IOrderPdfService
    {
        private const string Primary = "#0d6efd";
        private readonly IWebHostEnvironment _env;

        public OrderPdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] Generate(OrderModel order)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Lato"));

                    page.Header().Element(c => ComposeHeader(c, order));
                    page.Content().Element(c => ComposeContent(c, order));
                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Page ");
                        t.CurrentPageNumber();
                        t.Span(" of ");
                        t.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, OrderModel order)
        {
            container.PaddingBottom(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("CarRentoWeb").FontSize(22).Bold().FontColor(Primary);
                    col.Item().Text("Ernakulam, Kerala, India").FontColor(Colors.Grey.Darken1);
                    col.Item().Text("info@carrentoweb.com").FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(170).Column(col =>
                {
                    col.Item().AlignRight().Text("ORDER RECEIPT").FontSize(16).Bold();
                    col.Item().AlignRight().Text($"Order #{order.OrderId}");
                    col.Item().AlignRight().Text($"Date: {order.CreatedAt.ToLocalTime():dd MMM yyyy}");
                });
            });
        }

        private void ComposeContent(IContainer container, OrderModel order)
        {
            container.Column(col =>
            {
                col.Spacing(18);

                // Customer + Order details side by side
                col.Item().Row(row =>
                {
                    row.Spacing(15);

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                    {
                        c.Spacing(4);
                        c.Item().Text("Customer Details").Bold().FontSize(12).FontColor(Primary);
                        InfoRow(c, "Name", order.User?.FullName);
                        InfoRow(c, "Email", order.User?.Email);
                        InfoRow(c, "Phone", order.User?.PhoneNumber);
                        InfoRow(c, "Address", order.User?.Address);
                        InfoRow(c, "License No", order.User?.DrivingLicenseNo);
                    });

                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                    {
                        c.Spacing(4);
                        c.Item().Text("Order Details").Bold().FontSize(12).FontColor(Primary);
                        InfoRow(c, "Order #", order.OrderId.ToString());
                        InfoRow(c, "Order Status", order.Status.ToString());
                        InfoRow(c, "Payment", order.Payment?.Status.ToString());
                        InfoRow(c, "Payment ID", order.Payment?.ProviderPaymentId);
                        InfoRow(c, "Paid On", order.Payment?.PaidAt?.ToLocalTime().ToString("dd MMM yyyy, hh:mm tt"));
                    });
                });

                // Items table
                col.Item().Text("Order Items").Bold().FontSize(12).FontColor(Primary);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);   // Car
                        c.ConstantColumn(70);  // Image
                        c.RelativeColumn(3);   // Dates
                        c.ConstantColumn(40);  // Days
                        c.RelativeColumn(2);   // Price/day
                        c.RelativeColumn(2);   // Line total
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Car").FontColor(Colors.White).Bold();
                        h.Cell().Element(HeaderCell).Text("");
                        h.Cell().Element(HeaderCell).Text("Rental Dates").FontColor(Colors.White).Bold();
                        h.Cell().Element(HeaderCell).AlignCenter().Text("Days").FontColor(Colors.White).Bold();
                        h.Cell().Element(HeaderCell).AlignRight().Text("Price/Day").FontColor(Colors.White).Bold();
                        h.Cell().Element(HeaderCell).AlignRight().Text("Total").FontColor(Colors.White).Bold();
                    });

                    foreach (var item in order.OrderItems ?? new List<Models.OrderItem>())
                    {
                        table.Cell().Element(BodyCell).Element(c =>
                        {
                            var img = GetCarImage(item.Car);
                            if (img != null)
                                c.Height(40).Image(img).FitArea();
                        });

                        table.Cell().Element(BodyCell).Column(c =>
                        {
                            c.Item().Text(item.Car?.CarName ?? "-").SemiBold();
                            c.Item().Text(item.Car?.Brand?.BrandName ?? "").FontColor(Colors.Grey.Darken1).FontSize(9);
                        });
                        
                        table.Cell().Element(BodyCell).Text($"{item.RentalStartDate:dd MMM yyyy} - {item.RentalEndDate:dd MMM yyyy}");
                        table.Cell().Element(BodyCell).AlignCenter().Text(item.Days.ToString());
                        table.Cell().Element(BodyCell).AlignRight().Text($"Rs. {item.PricePerDay:N2}");
                        table.Cell().Element(BodyCell).AlignRight().Text($"Rs. {item.LineTotal:N2}");
                    }
                });

                // Totals
                col.Item().AlignRight().Width(220).Column(t =>
                {
                    t.Spacing(4);
                    TotalRow(t, "Subtotal", $"Rs. {order.SubTotal:N2}", false);
                    TotalRow(t, "Tax", $"Rs. {order.Tax:N2}", false);
                    t.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    TotalRow(t, "Total", $"Rs. {order.Total:N2}", true);
                });

                col.Item().PaddingTop(20).AlignCenter()
                    .Text("Thank you for choosing CarRentoWeb!").FontColor(Colors.Grey.Darken1).Italic();
            });
        }

        private static void InfoRow(ColumnDescriptor col, string label, string? value)
        {
            col.Item().Row(r =>
            {
                r.ConstantItem(80).Text(label).SemiBold().FontColor(Colors.Grey.Darken2);
                r.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value);
            });
        }

        private static void TotalRow(ColumnDescriptor col, string label, string value, bool bold)
        {
            col.Item().Row(r =>
            {
                var left = r.RelativeItem().Text(label);
                var right = r.RelativeItem().AlignRight().Text(value);
                if (bold)
                {
                    left.Bold().FontSize(12);
                    right.Bold().FontSize(12);
                }
            });
        }

        private static IContainer HeaderCell(IContainer c) =>
            c.Background(Primary).Padding(6);

        private static IContainer BodyCell(IContainer c) =>
            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6);

        private byte[]? GetCarImage(Models.Car? car)
        {
            var url = car?.CarImages?.FirstOrDefault()?.ImageUrl;
            if (string.IsNullOrEmpty(url)) return null;

            var ext = Path.GetExtension(url).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png") return null;

            var path = Path.Combine(_env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }
    }
}