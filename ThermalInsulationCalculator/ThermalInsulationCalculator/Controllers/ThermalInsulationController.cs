using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Reflection;
using ThermalInsulationCalculator.Models;
using Excel = Microsoft.Office.Interop.Excel;

namespace ThermalInsulationCalculator.Controllers
{
    public class ThermalInsulationController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return base.View(new Models.ThermalInsulationCalculator());
        }

        [HttpPost]
        [HttpPost]
        public IActionResult Calculate(Models.ThermalInsulationCalculator model)
        {
            if (ModelState.IsValid)
            {
                var (x1, x2, Cost, K, q, TOut) = model.CalculateOptimalThickness();
                model.x1 = x1; // Устанавливаем x1 модели
                model.x2 = x2; // Устанавливаем x2 модели

                ViewBag.OptimalX1 = x1;
                ViewBag.OptimalX2 = x2;
                ViewBag.MinCost = Cost;
                ViewBag.K = K;
                ViewBag.Q = q;
                ViewBag.TOut = TOut;;

                ViewBag.X1 = x1;
                ViewBag.X2 = x2;

            }
            return View("Index", model);
        }

        private MemoryStream ExportToExcel(Models.ThermalInsulationCalculator model, double x1, double x2, double cost, double k, double q, double tOut)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Thermal Insulation Data");

            // Заголовки
            worksheet.Cells["A1"].Value = "Parameter";
            worksheet.Cells["B1"].Value = "Value";

            // Исходные данные
            worksheet.Cells["A2"].Value = "Lambda0";
            worksheet.Cells["B2"].Value = model.Lambda0;
            worksheet.Cells["A3"].Value = "Lambda1";
            worksheet.Cells["B3"].Value = model.Lambda1;
            worksheet.Cells["A4"].Value = "Lambda2";
            worksheet.Cells["B4"].Value = model.Lambda2;
            worksheet.Cells["A5"].Value = "C1";
            worksheet.Cells["B5"].Value = model.C1;
            worksheet.Cells["A6"].Value = "C2";
            worksheet.Cells["B6"].Value = model.C2;
            worksheet.Cells["A7"].Value = "X0";
            worksheet.Cells["B7"].Value = model.X0;
            worksheet.Cells["A8"].Value = "D1";
            worksheet.Cells["B8"].Value = model.D1;
            worksheet.Cells["A9"].Value = "D2";
            worksheet.Cells["B9"].Value = model.D2;

            // Результаты расчёта
            worksheet.Cells["A11"].Value = "Optimal x1";
            worksheet.Cells["B11"].Value = x1;
            worksheet.Cells["A12"].Value = "Optimal x2";
            worksheet.Cells["B12"].Value = x2;
            worksheet.Cells["A13"].Value = "Minimum Cost";
            worksheet.Cells["B13"].Value = cost;
            worksheet.Cells["A14"].Value = "K";
            worksheet.Cells["B14"].Value = k;
            worksheet.Cells["A15"].Value = "q";
            worksheet.Cells["B15"].Value = q;
            worksheet.Cells["A16"].Value = "TOut";
            worksheet.Cells["B16"].Value = tOut;

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }


    }

}
