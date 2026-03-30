using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Top5.Utils
{
    public class AlphanumericComparer : IComparer<string>
    {
        // Regex précompilée pour des performances optimales sur de longues listes
        private static readonly Regex _numericRegex = new Regex(@"\d+", RegexOptions.Compiled);

        public int Compare(string? x, string? y)
        {
            if (x == null || y == null)
                return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);

            // Astuce : On remplace "MCH-2" par "MCH-0000000002" en mémoire uniquement pour le tri
            string paddedX = _numericRegex.Replace(x, match => match.Value.PadLeft(10, '0'));
            string paddedY = _numericRegex.Replace(y, match => match.Value.PadLeft(10, '0'));

            return string.Compare(paddedX, paddedY, StringComparison.OrdinalIgnoreCase);
        }
    }
}