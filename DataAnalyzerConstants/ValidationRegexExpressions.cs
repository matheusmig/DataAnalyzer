using System;

namespace DataAnalyzerConstants
{
    public static class ValidationRegexExpression
    {
        public const string ValidLine = @"^(\d{3})ç\d+ç.+ç.+";

        public const string ValidSalesman = @"^\d+ç\d+ç(\w+(\s+\w+)*)+ç\d+(\.\d+)?$";

        public const string ValidClient = @"^\d+ç\d+ç(\w+(\s+\w+)*)+ç(\w+(\s+\w+)*)$";

        public const string ValidSale = @"^\d+ç\d+ç\[(\d+\-\d+\-\d+(\.\d+)?)(,\d+\-\d+\-\d+(\.\d+)?)*\]ç(\w+(\s+\w+)*)$";
    }
}
