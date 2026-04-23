using System;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using QColors = QuestPDF.Helpers.Colors;
using Top5.Models;
using Top5.ViewModels;
using Top5.Utils;

namespace Top5.Services
{
    public static class PdfReportService
    {
        public static void GeneratePdf(MainViewModel vm, string filePath)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(15, Unit.Point);
                        page.PageColor(QColors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                        page.Content().Column(col =>
                        {
                            col.Item().AlignCenter().Text("Rapport de Production Top 5").Bold().FontSize(20);
                            col.Item().AlignCenter().PaddingBottom(10).Text($"{vm.ViewingDate:dd/MM/yyyy} — Jour : {vm.ViewingDayOfYear}").FontSize(14).FontColor(QColors.Grey.Darken2);

                            bool hasConsignes = !string.IsNullOrWhiteSpace(vm.TeamCommentMatin) || !string.IsNullOrWhiteSpace(vm.TeamCommentApresMidi) || !string.IsNullOrWhiteSpace(vm.TeamCommentNuit);
                            if (hasConsignes)
                            {
                                col.Item().PaddingBottom(10).Border(1).BorderColor(QColors.Red.Darken2).Column(c =>
                                {
                                    c.Item().Background(QColors.Red.Lighten4).Padding(4).Text("⚠️ CONSIGNES D'ÉQUIPES DU JOUR").Bold().FontColor(QColors.Red.Darken4);
                                    if (!string.IsNullOrWhiteSpace(vm.TeamCommentMatin)) c.Item().Padding(4).Text(t => { t.Span("Matin : ").Bold(); t.Span(vm.TeamCommentMatin); });
                                    if (!string.IsNullOrWhiteSpace(vm.TeamCommentApresMidi)) c.Item().Padding(4).Text(t => { t.Span("Après-Midi : ").Bold(); t.Span(vm.TeamCommentApresMidi); });
                                    if (!string.IsNullOrWhiteSpace(vm.TeamCommentNuit)) c.Item().Padding(4).Text(t => { t.Span("Nuit : ").Bold(); t.Span(vm.TeamCommentNuit); });
                                });
                            }

                            foreach (var row in vm.ProductionRows.Where(r => r.Production.Piece != "---" && r.Production.Moule != "---"))
                            {
                                // --- SÉCURITÉ POKA-YOKE : ShowEntire empêche physiquement le bloc machine d'être orphelin ---
                                col.Item().ShowEntire().Column(rowCol =>
                                {
                                    rowCol.Item().PaddingTop(5).Background(QColors.Grey.Lighten4).Padding(4).Row(r =>
                                    {
                                        r.RelativeItem().Text($" {row.Production.Machine} | Pièce: {row.Production.Piece} | Moule: {row.Production.Moule}   ").Bold().FontSize(12);

                                        string dmsStatus = GetDmsText(row.Production.DMSColor);
                                        string dmsBg = row.Production.DMSColor ?? "#DDDDDD";
                                        string dmsFg = dmsBg == "#DDDDDD" ? QColors.Black : QColors.White;

                                        r.AutoItem().PaddingRight(5).Text(" DMS : ").Bold().FontColor(QColors.Grey.Darken3);
                                        r.AutoItem().Background(dmsBg).PaddingHorizontal(4).Text(dmsStatus).Bold().FontColor(dmsFg);
                                        r.AutoItem().Text($" (Expire le {row.Production.DMSExpirationDateString})").FontColor(QColors.Grey.Darken3);
                                    });

                                    rowCol.Item().PaddingBottom(5).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); });

                                        // Couleur Slate Gray appliquée pour le visuel des entêtes d'équipes
                                        table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Background("#778899").Padding(2).AlignCenter().Text($"Matin ({GetControllerName(vm.ControllerMatin)})").Bold().FontColor(QColors.White);
                                        table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Background("#778899").Padding(2).AlignCenter().Text($"A-Midi ({GetControllerName(vm.ControllerApresMidi)})").Bold().FontColor(QColors.White);
                                        table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Background("#778899").Padding(2).AlignCenter().Text($"Nuit ({GetControllerName(vm.ControllerNuit)})").Bold().FontColor(QColors.White);

                                        table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Padding(2).Element(e => DrawShift(e, row.ReportMatin));
                                        table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Padding(2).Element(e => DrawShift(e, row.ReportApresMidi));
                                        table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Padding(2).Element(e => DrawShift(e, row.ReportNuit));
                                    });
                                });
                            }
                        });
                    });
                });

                doc.GeneratePdf(filePath);
            }
            catch (Exception ex)
            {
                Logger.Log($"ERREUR FATALE QUESTPDF : {ex.Message} \n {ex.StackTrace}");
                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); } catch { }
                }
            }
        }

        private static void DrawShift(IContainer container, ShiftReport report)
        {
            if (report == null) return;

            if (report.IsSP)
            {
                container = container.Background(QColors.Grey.Lighten2);
            }

            container.Column(col =>
            {
                if (report.IsSP)
                {
                    col.Item().PaddingBottom(5).AlignCenter().Text("SANS PRODUCTION").Bold().FontSize(10).FontColor(QColors.Grey.Darken3);
                }

                col.Item().PaddingBottom(2).Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                    t.Cell().Element(e => DrawStatusBadge(e, "RX", report.RXState.ToString()));
                    t.Cell().Element(e => DrawStatusBadge(e, "3D", report.DimensionalState.ToString()));
                    t.Cell().Element(e => DrawStatusBadge(e, "AC", report.AspectState.ToString()));
                });

                col.Item().PaddingBottom(2).Text($"Avis NC : {report.AncCount}").SemiBold().FontSize(9);

                if (report.Defects != null && report.Defects.Any())
                {
                    col.Item().Text("Défauts :").Bold().FontColor(QColors.Red.Darken3).FontSize(9);
                    foreach (var d in report.Defects)
                    {
                        string noyau = string.IsNullOrEmpty(d.CoreNumber) ? "" : $" [Nº {d.CoreNumber}]";
                        string stateHex = d.State.ToString() switch { "B" => "#2ECC71", "AA" => "#F39C12", "NC" => "#E74C3C", _ => "#DDDDDD" };
                        string fgColor = stateHex == "#DDDDDD" ? QColors.Black : QColors.White;

                        col.Item().PaddingTop(2).Row(r =>
                        {
                            r.AutoItem().Background(stateHex).PaddingHorizontal(3).Text(d.State.ToString()).Bold().FontSize(8).FontColor(fgColor);
                            r.AutoItem().PaddingLeft(3).Text($"{d.DefectType}{noyau}").SemiBold().FontSize(9);
                        });

                        if (!string.IsNullOrWhiteSpace(d.Comment))
                        {
                            col.Item().PaddingLeft(10).Text($"> {d.Comment}").Italic().FontSize(8).FontColor(QColors.Grey.Darken2);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(report.GeneralComment))
                {
                    col.Item().PaddingTop(2).Text($"Obs : {report.GeneralComment}").Italic().FontSize(9).FontColor(QColors.Grey.Darken3);
                }
            });
        }

        private static void DrawStatusBadge(IContainer container, string label, string state)
        {
            string bgBrush = QColors.Grey.Lighten2;
            if (state == "B") bgBrush = "#2ECC71";
            else if (state == "AA") bgBrush = "#F39C12";
            else if (state == "NC") bgBrush = "#E74C3C";

            container.Border(0.5f).BorderColor(QColors.Grey.Medium).Background(bgBrush).Padding(1).AlignCenter().Text(label).Bold().FontSize(9).FontColor(QColors.Black);
        }

        private static string GetControllerName(string name) => string.IsNullOrWhiteSpace(name) ? "?" : name;
        private static string GetDmsText(string hex) => hex == "#2ECC71" ? "À jour" : hex == "#F39C12" ? "Expire bientôt" : hex == "#E74C3C" ? "Expiré" : "N/A";
    }
}