using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Top5.Models;
using Top5.ViewModels;

namespace Top5.Services
{
    /// <summary>
    /// Service de génération de rapport PDF (FlowDocument).
    /// Version Ultra-Compacte avec système de "Badges" visuels pour les états DMS et Défauts.
    /// </summary>
    public static class PdfReportService
    {
        public static FlowDocument CreateDocument(MainViewModel vm)
        {
            FlowDocument doc = new FlowDocument { FontFamily = new FontFamily("Segoe UI"), FontSize = 12 };
            doc.PagePadding = new Thickness(15);
            doc.PageWidth = 793.7;
            doc.PageHeight = 1122.5;
            doc.ColumnWidth = doc.PageWidth - doc.PagePadding.Left - doc.PagePadding.Right;

            // --- TITRE DU RAPPORT ---
            Paragraph title = new Paragraph();
            title.TextAlignment = TextAlignment.Center;
            title.Margin = new Thickness(0, 0, 0, 10);

            title.Inlines.Add(new Run($"Rapport de Production Top 5") { FontSize = 20, FontWeight = FontWeights.Bold });
            title.Inlines.Add(new LineBreak());
            title.Inlines.Add(new Run($"{vm.ViewingDate:dd/MM/yyyy} — Jour : {vm.ViewingDayOfYear}") { FontSize = 16, Foreground = Brushes.DimGray });
            doc.Blocks.Add(title);

            // --- SECTION CONSIGNES D'ÉQUIPES ---
            if (!string.IsNullOrWhiteSpace(vm.TeamCommentMatin) ||
                !string.IsNullOrWhiteSpace(vm.TeamCommentApresMidi) ||
                !string.IsNullOrWhiteSpace(vm.TeamCommentNuit))
            {
                Table commentTable = new Table { Margin = new Thickness(0, 0, 0, 8), BorderBrush = Brushes.DarkRed, BorderThickness = new Thickness(1.5) };
                commentTable.Columns.Add(new TableColumn());
                TableRowGroup crg = new TableRowGroup();

                TableRow trTitle = new TableRow { Background = new SolidColorBrush(Color.FromRgb(250, 235, 235)) };
                trTitle.Cells.Add(new TableCell(new Paragraph(new Run("⚠️ CONSIGNES D'ÉQUIPES DU JOUR")) { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkRed, Padding = new Thickness(4), FontSize = 13 }));
                crg.Rows.Add(trTitle);

                if (!string.IsNullOrWhiteSpace(vm.TeamCommentMatin)) crg.Rows.Add(CreateCommentRow("Matin", vm.TeamCommentMatin));
                if (!string.IsNullOrWhiteSpace(vm.TeamCommentApresMidi)) crg.Rows.Add(CreateCommentRow("Après-Midi", vm.TeamCommentApresMidi));
                if (!string.IsNullOrWhiteSpace(vm.TeamCommentNuit)) crg.Rows.Add(CreateCommentRow("Nuit", vm.TeamCommentNuit));

                commentTable.RowGroups.Add(crg);
                doc.Blocks.Add(commentTable);
            }

            // --- BOUCLE SUR LES MACHINES ---
            foreach (var row in vm.ProductionRows)
            {
                if (row.Production.Piece == "---" && row.Production.Moule == "---") continue;

                Paragraph machineHeader = new Paragraph { Background = new SolidColorBrush(Color.FromRgb(235, 237, 239)), Padding = new Thickness(3), Margin = new Thickness(0, 6, 0, 0) };
                machineHeader.Inlines.Add(new Run($" {row.Production.Machine} | Pièce: {row.Production.Piece} | Moule: {row.Production.Moule}   ") { FontWeight = FontWeights.Bold, FontSize = 14 });

                string dmsStatus = GetDmsText(row.Production.DMSColor);
                Brush dmsBgBrush = GetBrushFromHex(row.Production.DMSColor);
                Brush dmsFgBrush = GetForegroundBrush(row.Production.DMSColor);

                machineHeader.Inlines.Add(new Run(" DMS : ") { Foreground = Brushes.DarkSlateGray, FontWeight = FontWeights.Bold, FontSize = 12 });
                // BADGE DMS : Remplacement de la pastille par un bloc de fond coloré
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

                    // BADGE DÉFAUT : Bloc de fond coloré avec le texte de l'état
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

            if (state == "B") bgBrush = new SolidColorBrush(Color.FromRgb(46, 204, 113));
            else if (state == "AA") bgBrush = new SolidColorBrush(Color.FromRgb(243, 156, 18));
            else if (state == "NC") bgBrush = new SolidColorBrush(Color.FromRgb(231, 76, 60));
            else bgBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));

            Paragraph p = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0) };
            p.Inlines.Add(new Run($"{label}") { FontWeight = FontWeights.Bold, FontSize = 10.5 });

            return new TableCell(p) { Background = bgBrush, BorderBrush = Brushes.DarkGray, BorderThickness = new Thickness(0.5), Padding = new Thickness(1) };
        }

        private static string GetControllerName(string name) => string.IsNullOrWhiteSpace(name) ? "?" : name;

        private static string GetDmsText(string hex)
        {
            if (hex == "#2ECC71") return "À jour";
            if (hex == "#F39C12") return "Expire bientôt";
            if (hex == "#E74C3C") return "Expiré";
            return "N/A";
        }

        private static SolidColorBrush GetBrushFromHex(string hex)
        {
            try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)); }
            catch { return Brushes.Black; }
        }

        // Utilitaire pour s'assurer que le texte dans le badge est lisible (Blanc sur couleur, Noir sur gris clair)
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
    }
}