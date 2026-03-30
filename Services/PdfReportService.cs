using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Top5.Models;
using Top5.ViewModels;

namespace Top5.Services
{
    public static class PdfReportService
    {
        public static FlowDocument CreateDocument(MainViewModel vm)
        {
            FlowDocument doc = new FlowDocument { FontFamily = new FontFamily("Segoe UI"), FontSize = 10 };
            doc.PagePadding = new Thickness(20);
            doc.PageWidth = 793.7;
            doc.PageHeight = 1122.5;
            doc.ColumnWidth = doc.PageWidth - doc.PagePadding.Left - doc.PagePadding.Right;

            Paragraph title = new Paragraph(new Run($"Rapport de Production Top 5 - {vm.ViewingDate:dd/MM/yyyy}"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            doc.Blocks.Add(title);

            // NOUVEAU : Encadré global des consignes d'équipes tout en haut du PDF
            if (!string.IsNullOrWhiteSpace(vm.TeamCommentMatin) ||
                !string.IsNullOrWhiteSpace(vm.TeamCommentApresMidi) ||
                !string.IsNullOrWhiteSpace(vm.TeamCommentNuit))
            {
                Table commentTable = new Table { Margin = new Thickness(0, 0, 0, 15), BorderBrush = Brushes.DarkRed, BorderThickness = new Thickness(1) };
                commentTable.Columns.Add(new TableColumn());
                TableRowGroup crg = new TableRowGroup();

                TableRow trTitle = new TableRow { Background = new SolidColorBrush(Color.FromRgb(250, 235, 235)) };
                trTitle.Cells.Add(new TableCell(new Paragraph(new Run("⚠️ CONSIGNES D'ÉQUIPES DU JOUR")) { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkRed, Padding = new Thickness(5) }));
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

                Paragraph machineHeader = new Paragraph { Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)), Padding = new Thickness(2), Margin = new Thickness(0, 5, 0, 0) };
                machineHeader.Inlines.Add(new Run($" {row.Production.Machine} | Pièce: {row.Production.Piece} | Moule: {row.Production.Moule} ") { FontWeight = FontWeights.Bold, FontSize = 11 });

                string dmsStatus = GetDmsText(row.Production.DMSColor);
                machineHeader.Inlines.Add(new Run($"(DMS: {dmsStatus})") { Foreground = GetBrushFromHex(row.Production.DMSColor), FontWeight = FontWeights.Bold, FontSize = 10 });
                doc.Blocks.Add(machineHeader);

                Table table = new Table { CellSpacing = 0, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 5) };
                table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

                TableRowGroup rg = new TableRowGroup();
                table.RowGroups.Add(rg);

                // Les en-têtes redeviennent normaux, sans les commentaires !
                TableRow headerRow = new TableRow { Background = Brushes.LightGray };
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
            Paragraph p = new Paragraph { Padding = new Thickness(5, 2, 5, 2) };
            p.Inlines.Add(new Run($"{shift} : ") { FontWeight = FontWeights.Bold, Foreground = Brushes.DarkSlateGray });
            p.Inlines.Add(new Run(comment));
            tr.Cells.Add(new TableCell(p));
            return tr;
        }

        private static TableCell CreateShiftCell(ShiftReport report)
        {
            TableCell cell = new TableCell { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Padding = new Thickness(2) };

            Table controlsTable = new Table { CellSpacing = 1, Margin = new Thickness(0, 0, 0, 2) };
            controlsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            controlsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            controlsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup trg = new TableRowGroup();
            TableRow tr = new TableRow();

            tr.Cells.Add(CreateStatusCell("RX", report.RXState.ToString()));
            tr.Cells.Add(CreateStatusCell("3D", report.DimensionalState.ToString()));
            tr.Cells.Add(CreateStatusCell("AC", report.AspectState.ToString()));

            trg.Rows.Add(tr);
            controlsTable.RowGroups.Add(trg);
            cell.Blocks.Add(controlsTable);

            cell.Blocks.Add(new Paragraph(new Run($"Avis NC : {report.AncCount}")) { Margin = new Thickness(0, 0, 0, 2), FontSize = 9, FontWeight = FontWeights.SemiBold });

            if (report.Defects.Count > 0)
            {
                cell.Blocks.Add(new Paragraph(new Run("Défauts :")) { FontWeight = FontWeights.Bold, Margin = new Thickness(0), FontSize = 9, Foreground = Brushes.DarkRed });
                foreach (var d in report.Defects)
                {
                    string noyau = string.IsNullOrEmpty(d.CoreNumber) ? "" : $" [Nº {d.CoreNumber}]";
                    cell.Blocks.Add(new Paragraph(new Run($"- {d.DefectType} ({d.State}){noyau}")) { Margin = new Thickness(2, 0, 0, 0), FontSize = 9 });
                }
            }

            if (!string.IsNullOrWhiteSpace(report.GeneralComment))
            {
                cell.Blocks.Add(new Paragraph(new Run($"Com: {report.GeneralComment}")) { FontStyle = FontStyles.Italic, Margin = new Thickness(0, 2, 0, 0), FontSize = 9, Foreground = Brushes.DarkSlateGray });
            }

            return cell;
        }

        private static TableCell CreateStatusCell(string label, string state)
        {
            Brush bgBrush = Brushes.LightGray;
            string text = state;

            if (state == "B") { bgBrush = new SolidColorBrush(Color.FromRgb(46, 204, 113)); text = "Conforme"; }
            else if (state == "AA") { bgBrush = new SolidColorBrush(Color.FromRgb(243, 156, 18)); text = "À Amél."; }
            else if (state == "NC") { bgBrush = new SolidColorBrush(Color.FromRgb(231, 76, 60)); text = "Non Conf."; }
            else { bgBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)); text = "Non Rens."; }

            Paragraph p = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0) };
            p.Inlines.Add(new Run($"{label}: ") { FontWeight = FontWeights.Bold, FontSize = 8 });
            p.Inlines.Add(new Run(text) { FontSize = 8 });

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

        private static TableCell CreateHeaderCell(string text)
        {
            Paragraph p = new Paragraph { Margin = new Thickness(2), TextAlignment = TextAlignment.Center };
            p.Inlines.Add(new Run(text) { FontWeight = FontWeights.Bold, FontSize = 10 });
            return new TableCell(p) { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1) };
        }
    }
}