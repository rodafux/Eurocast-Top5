using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// ==========================================================
// ALIAS : Résolution des conflits entre WPF et QuestPDF
// ==========================================================
using QColors = QuestPDF.Helpers.Colors;
using QFonts = QuestPDF.Helpers.Fonts;
using WpfColor = System.Windows.Media.Color;

using Top5.Models;
using Top5.ViewModels;
using Top5.Utils; // Pour utiliser le Logger

namespace Top5.Services
{
    public static class PdfReportService
    {
        // =================================================================================
        // PARTIE 1 : MOTEUR WPF (Utilisé UNIQUEMENT pour l'Aperçu à l'écran via PdfPreviewWindow)
        // =================================================================================
        public static FlowDocument CreateDocument(MainViewModel vm)
        {
            FlowDocument doc = new FlowDocument { FontFamily = new System.Windows.Media.FontFamily("Segoe UI"), FontSize = 12 };
            doc.PagePadding = new Thickness(15);
            doc.PageWidth = 793.7;
            doc.PageHeight = 1122.5;
            doc.ColumnWidth = doc.PageWidth - doc.PagePadding.Left - doc.PagePadding.Right;

            Paragraph title = new Paragraph();
            title.TextAlignment = TextAlignment.Center;
            title.Margin = new Thickness(0, 0, 0, 10);

            title.Inlines.Add(new Run($"Rapport de Production Top 5") { FontSize = 20, FontWeight = FontWeights.Bold });
            title.Inlines.Add(new LineBreak());
            title.Inlines.Add(new Run($"{vm.ViewingDate:dd/MM/yyyy} — Jour : {vm.ViewingDayOfYear}") { FontSize = 16, Foreground = Brushes.DimGray });
            doc.Blocks.Add(title);

            if (!string.IsNullOrWhiteSpace(vm.TeamCommentMatin) ||
                !string.IsNullOrWhiteSpace(vm.TeamCommentApresMidi) ||
                !string.IsNullOrWhiteSpace(vm.TeamCommentNuit))
            {
                Table commentTable = new Table { Margin = new Thickness(0, 0, 0, 8), BorderBrush = Brushes.DarkRed, BorderThickness = new Thickness(1.5) };
                commentTable.Columns.Add(new TableColumn());
                TableRowGroup crg = new TableRowGroup();

                TableRow trTitle = new TableRow { Background = new SolidColorBrush(WpfColor.FromRgb(250, 235, 235)) };
                trTitle.Cells.Add(new TableCell(new Paragraph(new Run("⚠️ CONSIGNES D'ÉQUIPES DU JOUR")) { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkRed, Padding = new Thickness(4), FontSize = 13 }));
                crg.Rows.Add(trTitle);

                if (!string.IsNullOrWhiteSpace(vm.TeamCommentMatin)) crg.Rows.Add(CreateCommentRow("Matin", vm.TeamCommentMatin));
                if (!string.IsNullOrWhiteSpace(vm.TeamCommentApresMidi)) crg.Rows.Add(CreateCommentRow("Après-Midi", vm.TeamCommentApresMidi));
                if (!string.IsNullOrWhiteSpace(vm.TeamCommentNuit)) crg.Rows.Add(CreateCommentRow("Nuit", vm.TeamCommentNuit));

                commentTable.RowGroups.Add(crg);
                doc.Blocks.Add(commentTable);
            }

            foreach (var row in vm.ProductionRows)
            {
                if (row.Production.Piece == "---" && row.Production.Moule == "---") continue;

                Paragraph machineHeader = new Paragraph { Background = new SolidColorBrush(WpfColor.FromRgb(235, 237, 239)), Padding = new Thickness(3), Margin = new Thickness(0, 6, 0, 0) };
                machineHeader.Inlines.Add(new Run($" {row.Production.Machine} | Pièce: {row.Production.Piece} | Moule: {row.Production.Moule}   ") { FontWeight = FontWeights.Bold, FontSize = 14 });

                string dmsStatus = GetDmsText(row.Production.DMSColor);
                Brush dmsBgBrush = GetBrushFromHex(row.Production.DMSColor);
                Brush dmsFgBrush = GetForegroundBrush(row.Production.DMSColor);

                machineHeader.Inlines.Add(new Run(" DMS : ") { Foreground = Brushes.DarkSlateGray, FontWeight = FontWeights.Bold, FontSize = 12 });
                machineHeader.Inlines.Add(new Run($" {dmsStatus} ") { Background = dmsBgBrush, Foreground = dmsFgBrush, FontWeight = FontWeights.Bold, FontSize = 12 });
                machineHeader.Inlines.Add(new Run($" (Expire le {row.Production.DMSExpirationDateString})") { Foreground = Brushes.DarkSlateGray, FontWeight = FontWeights.SemiBold, FontSize = 12 });

                doc.Blocks.Add(machineHeader);

                Table table = new Table { CellSpacing = 0, BorderBrush = Brushes.DarkGray, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 4) };
                table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

                TableRowGroup rg = new TableRowGroup();
                table.RowGroups.Add(rg);

                TableRow headerRow = new TableRow { Background = Brushes.LightSlateGray };
                headerRow.Cells.Add(CreateHeaderCell($"Matin ({GetControllerName(vm.ControllerMatin)})"));
                headerRow.Cells.Add(CreateHeaderCell($"A-Midi ({GetControllerName(vm.ControllerApresMidi)})"));
                headerRow.Cells.Add(CreateHeaderCell($"Nuit ({GetControllerName(vm.ControllerNuit)})"));
                rg.Rows.Add(headerRow);

                TableRow contentRow = new TableRow();
                contentRow.Cells.Add(CreateShiftCell(row.ReportMatin));
                contentRow.Cells.Add(CreateShiftCell(row.ReportApresMidi));
                contentRow.Cells.Add(CreateShiftCell(row.ReportNuit));
                rg.Rows.Add(contentRow);

                doc.Blocks.Add(table);
            }

            return doc;
        }

        private static TableRow CreateCommentRow(string shift, string comment)
        {
            TableRow tr = new TableRow();
            Paragraph p = new Paragraph { Padding = new Thickness(5, 2, 5, 2), FontSize = 12 };
            p.Inlines.Add(new Run($"{shift} : ") { FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
            p.Inlines.Add(new Run(comment));
            tr.Cells.Add(new TableCell(p));
            return tr;
        }

        private static TableCell CreateShiftCell(ShiftReport report)
        {
            TableCell cell = new TableCell { BorderBrush = Brushes.DimGray, BorderThickness = new Thickness(0.5), Padding = new Thickness(2) };

            Table controlsTable = new Table { CellSpacing = 1, Margin = new Thickness(0, 0, 0, 2) };
            controlsTable.Columns.Add(new TableColumn());
            controlsTable.Columns.Add(new TableColumn());
            controlsTable.Columns.Add(new TableColumn());

            TableRowGroup trg = new TableRowGroup();
            TableRow tr = new TableRow();
            tr.Cells.Add(CreateStatusCell("RX", report.RXState.ToString()));
            tr.Cells.Add(CreateStatusCell("3D", report.DimensionalState.ToString()));
            tr.Cells.Add(CreateStatusCell("AC", report.AspectState.ToString()));
            trg.Rows.Add(tr);
            controlsTable.RowGroups.Add(trg);
            cell.Blocks.Add(controlsTable);

            cell.Blocks.Add(new Paragraph(new Run($"Avis NC : {report.AncCount}")) { Margin = new Thickness(0, 0, 0, 2), FontSize = 11, FontWeight = FontWeights.SemiBold });

            if (report.Defects.Count > 0)
            {
                cell.Blocks.Add(new Paragraph(new Run("Défauts :")) { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 2, 0, 0), FontSize = 11, Foreground = Brushes.DarkRed });
                foreach (var d in report.Defects)
                {
                    string noyau = string.IsNullOrEmpty(d.CoreNumber) ? "" : $" [Nº {d.CoreNumber}]";
                    string stateHex = d.State.ToString() switch { "B" => "#2ECC71", "AA" => "#F39C12", "NC" => "#E74C3C", _ => "#DDDDDD" };

                    Brush badgeBg = GetBrushFromHex(stateHex);
                    Brush badgeFg = GetForegroundBrush(stateHex);

                    Paragraph pDefect = new Paragraph { Margin = new Thickness(0, 2, 0, 0), FontSize = 11 };

                    pDefect.Inlines.Add(new Run($" {d.State} ") { Background = badgeBg, Foreground = badgeFg, FontWeight = FontWeights.Bold, FontSize = 10 });
                    pDefect.Inlines.Add(new Run($" {d.DefectType}{noyau}") { FontWeight = FontWeights.SemiBold });

                    cell.Blocks.Add(pDefect);

                    if (!string.IsNullOrWhiteSpace(d.Comment))
                    {
                        Paragraph pComment = new Paragraph { Margin = new Thickness(10, 0, 0, 1), FontSize = 10, FontStyle = FontStyles.Italic, Foreground = Brushes.DimGray };
                        pComment.Inlines.Add(new Run($"> {d.Comment}"));
                        cell.Blocks.Add(pComment);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(report.GeneralComment))
            {
                cell.Blocks.Add(new Paragraph(new Run($"Obs : {report.GeneralComment}")) { FontStyle = FontStyles.Italic, Margin = new Thickness(0, 2, 0, 0), FontSize = 10.5, Foreground = Brushes.DarkSlateGray });
            }

            return cell;
        }

        private static TableCell CreateStatusCell(string label, string state)
        {
            Brush bgBrush = Brushes.LightGray;

            if (state == "B") bgBrush = new SolidColorBrush(WpfColor.FromRgb(46, 204, 113));
            else if (state == "AA") bgBrush = new SolidColorBrush(WpfColor.FromRgb(243, 156, 18));
            else if (state == "NC") bgBrush = new SolidColorBrush(WpfColor.FromRgb(231, 76, 60));
            else bgBrush = new SolidColorBrush(WpfColor.FromRgb(220, 220, 220));

            Paragraph p = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0) };
            p.Inlines.Add(new Run($"{label}") { FontWeight = FontWeights.Bold, FontSize = 10.5 });

            return new TableCell(p) { Background = bgBrush, BorderBrush = Brushes.DarkGray, BorderThickness = new Thickness(0.5), Padding = new Thickness(1) };
        }

        private static SolidColorBrush GetBrushFromHex(string hex)
        {
            try { return new SolidColorBrush((WpfColor)ColorConverter.ConvertFromString(hex)); }
            catch { return Brushes.Black; }
        }

        private static Brush GetForegroundBrush(string hex)
        {
            if (hex == "#DDDDDD") return Brushes.Black;
            return Brushes.White;
        }

        private static TableCell CreateHeaderCell(string text)
        {
            Paragraph p = new Paragraph { Margin = new Thickness(2), TextAlignment = TextAlignment.Center };
            p.Inlines.Add(new Run(text) { FontWeight = FontWeights.Bold, FontSize = 12, Foreground = Brushes.White });
            return new TableCell(p) { BorderBrush = Brushes.DarkGray, BorderThickness = new Thickness(1) };
        }


        // =================================================================================
        // PARTIE 2 : MOTEUR QUESTPDF (Utilisé UNIQUEMENT en arrière-plan pour l'exportation auto)
        // =================================================================================

        public static void GeneratePdf(MainViewModel vm, string filePath)
        {
            try
            {
                // SÉCURITÉ : On garantit l'activation de la licence juste avant la génération
                QuestPDF.Settings.License = LicenseType.Community;

                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(15, Unit.Point);
                        page.PageColor(QColors.White);

                        // Laisse QuestPDF gérer ses polices natives pour éviter les crashs
                        page.DefaultTextStyle(x => x.FontSize(10));

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
                                col.Item().PaddingTop(5).Background(QColors.Grey.Lighten4).Padding(4).Row(r =>
                                {
                                    r.RelativeItem().Text($" {row.Production.Machine} | Pièce: {row.Production.Piece} | Moule: {row.Production.Moule}   ").Bold().FontSize(12);

                                    string dmsStatus = GetDmsText(row.Production.DMSColor);
                                    string dmsBg = row.Production.DMSColor ?? "#DDDDDD";
                                    string dmsFg = dmsBg == "#DDDDDD" ? QColors.Black : QColors.White;

                                    r.AutoItem().PaddingRight(5).Text(" DMS : ").Bold().FontColor(QColors.Grey.Darken3);
                                    r.AutoItem().Background(dmsBg).PaddingHorizontal(4).Text(dmsStatus).Bold().FontColor(dmsFg);
                                    r.AutoItem().Text($" (Expire le {row.Production.DMSExpirationDateString})").FontColor(QColors.Grey.Darken3);
                                });

                                col.Item().PaddingBottom(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); });

                                    table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Background(QColors.Grey.Darken1).Padding(2).AlignCenter().Text($"Matin ({GetControllerName(vm.ControllerMatin)})").Bold().FontColor(QColors.White);
                                    table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Background(QColors.Grey.Darken1).Padding(2).AlignCenter().Text($"A-Midi ({GetControllerName(vm.ControllerApresMidi)})").Bold().FontColor(QColors.White);
                                    table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Background(QColors.Grey.Darken1).Padding(2).AlignCenter().Text($"Nuit ({GetControllerName(vm.ControllerNuit)})").Bold().FontColor(QColors.White);

                                    table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Padding(2).Element(e => DrawShift(e, row.ReportMatin));
                                    table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Padding(2).Element(e => DrawShift(e, row.ReportApresMidi));
                                    table.Cell().Border(1).BorderColor(QColors.Grey.Medium).Padding(2).Element(e => DrawShift(e, row.ReportNuit));
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

                // ANTI-FICHIERS CORROMPUS : Si le rendu plante, on détruit le fichier 0 Ko.
                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); } catch { }
                }
            }
        }

        private static void DrawShift(IContainer container, ShiftReport report)
        {
            if (report == null) return;

            container.Column(col =>
            {
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

        // --- METHODES UTILITAIRES PARTAGEES (Communes aux deux moteurs) ---
        private static string GetControllerName(string name) => string.IsNullOrWhiteSpace(name) ? "?" : name;
        private static string GetDmsText(string hex) => hex == "#2ECC71" ? "À jour" : hex == "#F39C12" ? "Expire bientôt" : hex == "#E74C3C" ? "Expiré" : "N/A";
    }
}