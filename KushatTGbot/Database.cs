using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotExperiments
{
    public class DishRecorder
    {
        private string databasePath;
        public DishRecorder(string databasePath)
        {
            this.databasePath = databasePath;
        }
        public void RecordDishes(long userId, string firstName, string lastName, List<string> salads, List<string> hotDishes, List<string> mainDishes, List<string> drinks)
        {
            using (var package = new ExcelPackage(new FileInfo(databasePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "Dishes");
                if (worksheet == null)
                {
                    worksheet = package.Workbook.Worksheets.Add("Dishes");
                    worksheet.Cells[1, 1].Value = "Id Пользователя";
                    worksheet.Cells[1, 2].Value = "Имя";
                    worksheet.Cells[1, 3].Value = "Фамилия";
                    worksheet.Cells[1, 4].Value = "Салаты";
                    worksheet.Cells[1, 5].Value = "Горячие блюда";
                    worksheet.Cells[1, 6].Value = "Основные блюда";
                    worksheet.Cells[1, 7].Value = "Напиток";
                    worksheet.Cells[1, 8].Value = "Свое блюдо";
                }               
                int userIdRow = worksheet.Cells["A:A"].FirstOrDefault(cell => cell.Value?.ToString() == userId.ToString())?.Start.Row ?? worksheet.Dimension?.End.Row + 1 ?? 2;
                worksheet.Cells[userIdRow, 1].Value = userId;
                worksheet.Cells[userIdRow, 2].Value = firstName;
                worksheet.Cells[userIdRow, 3].Value = lastName;
                for (int i = 0; i < salads.Count; i++)
                {
                    worksheet.Cells[userIdRow, 4].Offset(0, i).Value = salads[i];
                }
                for (int i = 0; i < hotDishes.Count; i++)
                {
                    worksheet.Cells[userIdRow, 5].Offset(0, i).Value = hotDishes[i];
                }
                for (int i = 0; i < mainDishes.Count; i++)
                {
                    worksheet.Cells[userIdRow, 6].Offset(0, i).Value = mainDishes[i];
                }
                for (int i = 0; i < drinks.Count; i++)
                {
                    worksheet.Cells[userIdRow, 7].Offset(0, i).Value = drinks[i];
                }
                package.Save();
            }
        }
        public void DeleteSelectedDishes(long userId)
        {
            using (var package = new ExcelPackage(new FileInfo(databasePath)))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "Dishes");
                if (worksheet == null)
                {
                    return;
                }
                int? userIdRow = worksheet.Cells["A:A"].FirstOrDefault(cell => cell.Value?.ToString() == userId.ToString())?.Start.Row;
                if (userIdRow == null)
                {
                    return;
                }
                int row = userIdRow.Value;
                for (int i = 4; i <= 7; i++)
                {
                    var range = worksheet.Cells[row, i, row, worksheet.Dimension.End.Column];
                    range.Clear();
                }
                package.Save();
            }
        }
        public List<string> CheckTwoDishFields(long userId, ITelegramBotClient bot)
        {
            List<string> selectedDishes = new List<string>();

            using (var package = new ExcelPackage(new FileInfo(databasePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "Dishes");
                if (worksheet == null)
                {
                    return selectedDishes;
                }

                int rowCount = worksheet.Dimension.End.Row;
                for (int row = 2; row <= rowCount; row++)
                {
                    var userIdValue = worksheet.Cells[row, 1].Value?.ToString();
                    if (userIdValue != null && userIdValue == userId.ToString())
                    {
                        var salads = worksheet.Cells[row, 4].Value?.ToString();
                        var hotDishes = worksheet.Cells[row, 5].Value?.ToString();
                        var mainDishes = worksheet.Cells[row, 6].Value?.ToString();
                        var drinks = worksheet.Cells[row, 7].Value?.ToString();
                        if (!string.IsNullOrEmpty(salads) && !string.IsNullOrEmpty(hotDishes) && !string.IsNullOrEmpty(mainDishes))
                        {
                            DeleteSelectedDishes(userId);
                           
                             bot.SendTextMessageAsync(userId, "Вы выбрали обед из двух, а заказали три. Заказ сбросился! Все ваши выбранные блюда удалены! Перезаказывайте", replyMarkup: Program.GetInlineKeyboard(Inlines.RestartMenuOptions()));
                        }
                       
                        else if((!string.IsNullOrEmpty(salads) && !string.IsNullOrEmpty(hotDishes) && !string.IsNullOrEmpty(drinks))
                   || (!string.IsNullOrEmpty(salads) && !string.IsNullOrEmpty(mainDishes) && !string.IsNullOrEmpty(drinks))
                   || (!string.IsNullOrEmpty(hotDishes) && !string.IsNullOrEmpty(mainDishes) && !string.IsNullOrEmpty(drinks)))
                        {
                            selectedDishes.Add($"Салат: {salads} \nГорячее: {hotDishes} \nОсновное: {mainDishes} \nНапиток: {drinks}");
                        }
                        
                        
                    }
                }
            }

            return selectedDishes;
        }
        
        public List<string> CheckThreeDishFields(long userId)
        {
            List<string> selectedDishes = new List<string>();

            using (var package = new ExcelPackage(new FileInfo(databasePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "Dishes");
                if (worksheet == null)
                {
                    return selectedDishes;
                }

                int rowCount = worksheet.Dimension.End.Row;
                for (int row = 2; row <= rowCount; row++)
                {
                    var userIdValue = worksheet.Cells[row, 1].Value?.ToString();
                    if (userIdValue != null && userIdValue == userId.ToString())
                    {
                        var salads = worksheet.Cells[row, 4].Value?.ToString();
                        var hotDishes = worksheet.Cells[row, 5].Value?.ToString();
                        var mainDishes = worksheet.Cells[row, 6].Value?.ToString();
                        var drinks = worksheet.Cells[row, 7].Value?.ToString();
                        if (!string.IsNullOrEmpty(salads) && !string.IsNullOrEmpty(hotDishes) && !string.IsNullOrEmpty(mainDishes) )
                        {
                            selectedDishes.Add($"Салат: {salads} \nГорячее: {hotDishes} \nОсновное: {mainDishes} ");
                        }
                    }
                }
            }

            return selectedDishes;
        }
        //public void CountSelectedDishes()
        //{
        //    using (var package = new ExcelPackage(new FileInfo(databasePath)))
        //    {
        //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //        var worksheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == "Sheet1");
        //        if (worksheet == null)
        //        {
        //            Console.WriteLine("Dishes worksheet not found.");
        //            return;
        //        }

        //        int lastRow = 1;
        //        List<string> selectedDishes = new List<string>();

        //        for (int row = 2; row <= lastRow; row++)
        //        {
        //            string saladName = worksheet.Cells[row, 4].Value?.ToString();
        //            string hotDishName = worksheet.Cells[row, 5].Value?.ToString();
        //            string mainDishName = worksheet.Cells[row, 6].Value?.ToString();

        //            if (!string.IsNullOrEmpty(saladName))
        //            {
        //                selectedDishes.Add(saladName);
        //            }
        //            if (!string.IsNullOrEmpty(hotDishName))
        //            {
        //                selectedDishes.Add(hotDishName);
        //            }
        //            if (!string.IsNullOrEmpty(mainDishName))
        //            {
        //                selectedDishes.Add(mainDishName);
        //            }
        //        }

        //        var selectedDishesWorksheet = package.Workbook.Worksheets.Add("Sheet1");
        //        selectedDishesWorksheet.Cells[1, 1].Value = "Sheet1";
        //        int rowIndex = 2;

        //        foreach (var dish in selectedDishes)
        //        {
        //            selectedDishesWorksheet.Cells[rowIndex, 1].Value = dish;
        //            rowIndex++;
        //        }

        //        package.Save();
        //    }
        //}

    }
}
    



