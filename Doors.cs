using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.IO;

[Transaction(TransactionMode.ReadOnly)] // Режим "только чтение"
public class DoorFireRatingChecker : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        Document doc = commandData.Application.ActiveUIDocument.Document;
        List<string> errors = new List<string>();

        // 1. Попробуем найти параметр по разным именам
        string[] possibleParamNames = { "Огнестойкость", "FireRating", "Противопожарный" };

        using (Transaction trans = new Transaction(doc, "Check Doors"))
        {
            trans.Start();

            var doors = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType();

            foreach (var door in doors)
            {
                string fireRatingValue = null;
                foreach (var paramName in possibleParamNames)
                {
                    Parameter param = door.LookupParameter(paramName);
                    if (param != null && !string.IsNullOrEmpty(param.AsString()))
                    {
                        fireRatingValue = param.AsString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(fireRatingValue))
                {
                    string doorInfo = $"Дверь ID: {door.Id} | Уровень: {GetLevelName(doc, door)}";
                    errors.Add(doorInfo);
                }
            }

            trans.Commit();
        }

        // 2. Вывод результатов
        if (errors.Count > 0)
        {
            string reportPath = "C:/DoorFireRating_Report.csv";
            File.WriteAllLines(reportPath, new[] { "ID двери,Уровень" }.Concat(errors));
            TaskDialog.Show("Результат", $"Найдено {errors.Count} дверей без параметра. Отчет сохранен в {reportPath}");
        }
        else
        {
            TaskDialog.Show("Успех", "Все двери имеют параметр 'Огнестойкость'!");
        }

        return Result.Succeeded;
    }

    private string GetLevelName(Document doc, Element door)
    {
        Parameter levelParam = door.get_Parameter(BuiltInParameter.DOOR_LEVEL);
        return levelParam?.AsString() ?? "Нет уровня";
    }
}
