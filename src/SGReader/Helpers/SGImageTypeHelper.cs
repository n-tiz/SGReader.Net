using System.Collections.Generic;
using System.Linq;

namespace SGReader.Helpers
{
    internal static class SGImageTypeHelper
    {
        public static string FormatType(byte type) => type switch
        {
            0 => "Sprite (0)",
            1 => "Plain (1)",
            10 => "Plain (10)",
            12 => "Plain (12)",
            13 => "Plain (13)",
            20 => "Plain (20)",
            30 => "Isometric (30)",
            _ => $"Type {type}"
        };

        public static string FormatTypes(IEnumerable<byte> types)
        {
            var labels = types
                .Distinct()
                .OrderBy(t => t)
                .Select(FormatType)
                .ToList();

            return labels.Count == 0 ? "—" : string.Join(" · ", labels);
        }
    }
}
