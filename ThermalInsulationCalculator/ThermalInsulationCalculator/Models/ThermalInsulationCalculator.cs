using Google.OrTools.LinearSolver;
using Google.OrTools.Init;
using System.Reflection;
using System.Linq.Expressions;

namespace ThermalInsulationCalculator.Models
{
    public class ThermalInsulationCalculator
    {
        public double Lambda0 { get; set; } = 24; // Коэффициент теплопроводности трубы
        public double Lambda1 { get; set; } = 1;  // Коэффициент теплопроводности 1-го материала
        public double Lambda2 { get; set; } = 0.2; // Коэффициент теплопроводности 2-го материала
        public double C1 { get; set; } = 1; // Стоимость 1-го материала
        public double C2 { get; set; } = 8; // Стоимость 2-го материала
        public double X0 { get; set; } = 200; // Суммарная толщина изоляции
        public double D1 { get; set; } = 200; // Внутренний диаметр трубы
        public double D2 { get; set; } = 230; // Внешний диаметр трубы
        public double TEnv { get; set; } = 20; // Температура окружающей среды
        public double TLiquid { get; set; } = 900; // Температура жидкости
        public double TMax { get; set; } = 40; // Максимальная температура
        public double AlphaInner { get; set; } = 500; // Коэффициент теплоотдачи внутренний
        public double AlphaOuter { get; set; } = 100; // Коэффициент теплоотдачи наружный
        public double x1 {  get; set; }
        public double x2 { get; set; }
        
        public double d3 { get => D2 * 0.001 + 2 * x1; }
        public double d4 { get => d3 + 2 * x2; }
        public double K { get => 1 / (1 / (AlphaInner * D1 * 0.001) + (1 / (2 * Lambda0)) * Math.Log(D2 * 0.001 / D1 * 0.001) + (1 / (2 * Lambda1)) * Math.Log(d3 / (D2 * 0.001)) + (1 / (2 * Lambda2)) * Math.Log(d4 / d3) + (1 / (AlphaOuter * d4))); }
        public double q { get => Math.PI * K * (TLiquid - TEnv); }
        public double TOut { get => TEnv + Math.PI / q * 1 / (AlphaOuter * d4); }
        public (double x1, double x2, double Cost, double K, double q, double TOut) CalculateOptimalThickness()
        {
            // Перевод размеров в метры
            double d1 = D1 / 1000;
            double d2 = D2 / 1000;
            double x0 = X0 / 1000;

            // Параметры
            double c1 = C1;
            double c2 = C2;
            double tMaxOuterSurface = TMax;

            // Создание линейного солвера
            Solver solver = Solver.CreateSolver("GLOP");
            if (solver == null)
            {
                throw new Exception("Solver not created.");
            }

            // Переменные x1 и x2 (толщины слоёв изоляции)
            Variable x1Var = solver.MakeNumVar(0.0000, double.PositiveInfinity, "x1");
            Variable x2Var = solver.MakeNumVar(0.0000, double.PositiveInfinity, "x2");

            // Ограничение: x1 + x2 == x0
            solver.Add(x1Var + x2Var == x0);

            // Ограничение на температуру наружной поверхности
            Variable tOut = solver.MakeNumVar(0.0000, double.PositiveInfinity, "tOut");
            solver.Add(tOut <= tMaxOuterSurface);

            // Целевая функция: минимизация стоимости изоляции
            Objective objective = solver.Objective();
            objective.SetCoefficient(x1Var, Math.PI * c1 * (d2 + x0));
            objective.SetCoefficient(x2Var, Math.PI * c2 * (d2 + x0 + 2 * x0));
            objective.SetMinimization();



            // Решение задачи
            Solver.ResultStatus resultStatus = solver.Solve();

            if (resultStatus == Solver.ResultStatus.OPTIMAL)
            {
                x1 = x1Var.SolutionValue();
                x2 = x2Var.SolutionValue();
                double minCost = objective.Value();
                return (Math.Round(x1, 4), Math.Round(x2, 4), Math.Round(minCost, 4), Math.Round(K, 4), Math.Round(q, 4), Math.Round(TOut, 4));
            }
            else
            {
                throw new Exception("The solver could not find an optimal solution.");
            }
        }

    }
}
