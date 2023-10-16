using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBotExperiments
{
    public class DatabasesMenu
    {

        public static List<string> SaladMenuOptions(out string salad1, out string salad2, out string salad3)
        {
            List<string> optionssalads = new List<string>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string filePath = "Database\\database.xlsx";
            string sheetName = "Sheet1";
            int rowNumber = 1;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                salad1 = worksheet.Cells[2, rowNumber].Value?.ToString();
                salad2 = worksheet.Cells[3, rowNumber].Value?.ToString();
                salad3 = worksheet.Cells[4, rowNumber].Value?.ToString();
                if (!string.IsNullOrEmpty(salad1))
                    optionssalads.Add(salad1);
                if (!string.IsNullOrEmpty(salad2))
                    optionssalads.Add(salad2);
                if (!string.IsNullOrEmpty(salad3))
                    optionssalads.Add(salad3);
            }

            optionssalads.Add("Назад");

            return optionssalads;
        }
        public static List<string> HotDishMenuOptions(out string hotdish1, out string hotdish2, out string hotdish3)
        {
            List<string> optionshotdish = new List<string>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string filePath = "Database\\database.xlsx";
            string sheetName = "Sheet1";
            int rowNumber = 2;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                hotdish1 = worksheet.Cells[2, rowNumber].Value?.ToString();
                hotdish2 = worksheet.Cells[3, rowNumber].Value?.ToString();
                hotdish3 = worksheet.Cells[4, rowNumber].Value?.ToString();

                if (!string.IsNullOrEmpty(hotdish1))
                    optionshotdish.Add(hotdish1);
                if (!string.IsNullOrEmpty(hotdish2))
                    optionshotdish.Add(hotdish2);
                if (!string.IsNullOrEmpty(hotdish3))
                    optionshotdish.Add(hotdish3);
            }
            optionshotdish.Add("Назад");
            return optionshotdish;
        }

        public static List<string> MainDishMenuOptions(out string maindish1, out string maindish2)
        {
            List<string> optionsmaindish = new List<string>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string filePath = "Database\\database.xlsx";
            string sheetName = "Sheet1";
            int rowNumber = 3;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];

                maindish1 = worksheet.Cells[2, rowNumber].Value?.ToString();
                maindish2 = worksheet.Cells[3, rowNumber].Value?.ToString();

                if (!string.IsNullOrEmpty(maindish1))
                    optionsmaindish.Add(maindish1);
                if (!string.IsNullOrEmpty(maindish2))
                    optionsmaindish.Add(maindish2);

            }

            optionsmaindish.Add("Назад");
            return optionsmaindish;
        }
        public static List<string> DrinkMenuOptions(out string drink1, out string drink2, out string drink3)
        {
            List<string> optionsdrink = new List<string>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string filePath = "Database\\database.xlsx";
            string sheetName = "Sheet1";
            int rowNumber = 4;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                drink1 = worksheet.Cells[2, rowNumber].Value?.ToString();
                drink2 = worksheet.Cells[3, rowNumber].Value?.ToString();
                drink3 = worksheet.Cells[4, rowNumber].Value?.ToString();
                if (!string.IsNullOrEmpty(drink1))
                    optionsdrink.Add(drink1);
                if (!string.IsNullOrEmpty(drink2))
                    optionsdrink.Add(drink2);
                if (!string.IsNullOrEmpty(drink3))
                    optionsdrink.Add(drink3);
            }

            optionsdrink.Add("Назад");

            return optionsdrink;
        }
    }
}
