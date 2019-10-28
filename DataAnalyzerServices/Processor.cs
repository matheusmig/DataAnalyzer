using DataAnalyzerConstants;
using DataAnalyzerModels;
using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataAnalyzerServices
{
    public class Processor : IProcessor
    {
        private readonly ILogger _logger;
        public Processor(ILogger<IProcessor> logger)
        {
            _logger = logger;
        }

        public async Task<IModel> ProcessLineAsync(string line)
        {
            if (!TryValidateLine(line, out var code))
                return default;

            switch (code)
            {
                case CodeIdentifier.Salesman: return ProcessSalesman(line);
                case CodeIdentifier.Client: return ProcessClient(line);
                case CodeIdentifier.Sale: return ProcessSale(line);
                default: 
                    _logger.LogWarning($"{nameof(ProcessLineAsync)} Invalid Code: {line}");
                    break;
            }

            return default;
        }

        private bool TryValidateLine(string line, out CodeIdentifier codeId)
        {
            codeId = CodeIdentifier.Unknown;

            var match = Regex.Match(line, ValidationRegexExpression.ValidLine, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!match.Success)
            {
                _logger.LogWarning($"{nameof(TryValidateLine)} : Invalid Line : {line}");
                return false;
            }

            var codeGroup = match.Groups[1];
            if (!int.TryParse(codeGroup.Value, out var code))
            {
                _logger.LogWarning($"{nameof(TryValidateLine)} : Cannot parse Code : {line}");
                return false;
            }

            if (!Enum.IsDefined(typeof(CodeIdentifier), code)) 
            {
                _logger.LogWarning($"{nameof(TryValidateLine)} : Code is not defined : {line}");
                return false;
            }

            codeId = (CodeIdentifier)code;
            return true;
        }

        private Salesman ProcessSalesman(string line) 
        {
            var match = Regex.Match(line, ValidationRegexExpression.ValidSalesman, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!match.Success)
            {
                _logger.LogWarning($"{nameof(ProcessSalesman)} : Invalid : {line}");
                return null;
            }

            var splittedLine = line.Split(ConfigurationConstants.LineSeparatorCharacter);
            if (splittedLine.Length < 4)
            {
                _logger.LogWarning($"{nameof(ProcessSalesman)} : Invalid split length : {line} : {splittedLine.Length}");
                return null;
            }

            if(!decimal.TryParse(splittedLine[3], out var salaryDecimal))
            {
                _logger.LogWarning($"{nameof(ProcessSalesman)} : Cannot parse salary : {line} : {splittedLine[3]}");
                return null;
            }

            return new Salesman()
            {
                Code = CodeIdentifier.Salesman,
                CPF = splittedLine[1],
                Name = splittedLine[2],
                Salary = salaryDecimal
            };
        }

        private Client ProcessClient(string line)
        {
            var match = Regex.Match(line, ValidationRegexExpression.ValidClient, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!match.Success)
            {
                _logger.LogWarning($"{nameof(ProcessClient)} : Invalid : {line}");
                return null;
            }

            var splittedLine = line.Split(ConfigurationConstants.LineSeparatorCharacter);
            if (splittedLine.Length < 4)
            {
                _logger.LogWarning($"{nameof(ProcessClient)} : Invalid split length : {line} : {splittedLine.Length}");
                return null;
            }

            return new Client()
            {
                Code = CodeIdentifier.Client,
                CNPJ = splittedLine[1],
                Name = splittedLine[2],
                BusinessArea = splittedLine[3]
            };
        }

        private Sale ProcessSale(string line)
        {
            var match = Regex.Match(line, ValidationRegexExpression.ValidSale, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!match.Success)
            {
                _logger.LogWarning($"{nameof(ProcessSale)} : Invalid : {line}");
                return null;
            }

            var splittedLine = line.Split(ConfigurationConstants.LineSeparatorCharacter);
            if (splittedLine.Length < 4)
            {
                _logger.LogWarning($"{nameof(ProcessSale)} : Invalid split length : {line} : {splittedLine.Length}");
                return null;
            }

            if(!int.TryParse(splittedLine[1], out var saleId))
            {
                _logger.LogWarning($"{nameof(ProcessSale)} : Invalid saleId : {line} : {splittedLine[1]}");
                return null;
            }

            var items = ProcessItems(splittedLine[2]);

            return new Sale()
            {
                Code = CodeIdentifier.Sale,
                SaleId = saleId,
                Items = items,
                SalesmanName = splittedLine[3]
            };
        }

        private IEnumerable<Item> ProcessItems(string itemsLine)
        {
            var result = new List<Item>();

            if (!itemsLine.StartsWith(ConfigurationConstants.ItemListStartMark) ||
                !itemsLine.EndsWith(ConfigurationConstants.ItemListEndMark))
            {
                _logger.LogWarning($"{nameof(ProcessItems)} : Invalid : {itemsLine}");
                return result;
            }

            var ItemsLineWithoutMarks = itemsLine.Remove(itemsLine.Length - 1).Remove(0, 1);    
            var itemsText = ItemsLineWithoutMarks.Split(ConfigurationConstants.ItemSeparatorCharacter);
            if (!itemsText.Any())
            {
                _logger.LogWarning($"{nameof(ProcessItems)} : Invalid item separator : {ItemsLineWithoutMarks}");
                return result;
            }

            foreach(var itemText in itemsText)
            {
                var itemFields = itemText.Split(ConfigurationConstants.ItemFieldsSeparatorCharacter);
                if (itemFields.Length != 3)
                {
                    _logger.LogWarning($"{nameof(ProcessItems)} : Invalid item fields : {itemText}");
                    continue;
                }

                if (!int.TryParse(itemFields[0], out var itemId))
                {
                    _logger.LogWarning($"{nameof(ProcessItems)} : Invalid itemId : {itemFields[0]}");
                    continue;
                }

                if (!int.TryParse(itemFields[1], out var quantity))
                {
                    _logger.LogWarning($"{nameof(ProcessItems)} : Invalid quantity : {itemFields[1]}");
                    continue;
                }

                if (!decimal.TryParse(itemFields[2], out var price))
                {
                    _logger.LogWarning($"{nameof(ProcessItems)} : Invalid price : {itemFields[2]}");
                    continue;
                }

                result.Add(new Item()
                {
                    ItemId = itemId,
                    Quantity = quantity,
                    Price = price
                });
            }

            return result;
        }
    }
}
