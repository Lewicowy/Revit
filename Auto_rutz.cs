using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RenameViewsPlugin
{
    [Transaction(TransactionMode.Manual)]
    public class RenameViewsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Получаем доступ к документу и UI
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1. Получаем все виды в проекте (можно добавить фильтр по типу)
                List<View> views = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(v => !v.IsTemplate && v.ViewType == ViewType.FloorPlan) // Только планы этажей
                    .ToList();

                if (views.Count == 0)
                {
                    TaskDialog.Show("Ошибка", "Не найдено подходящих видов.");
                    return Result.Cancelled;
                }

                // 2. Запрашиваем шаблон имени у пользователя (например: "Этаж {level}_План_{discipline}")
                string template = TaskDialog.Show(
                    "Шаблон имени",
                    "Введите шаблон (используйте {level}, {discipline}):",
                    TaskDialogCommonButtons.OkCancel) as string;

                if (string.IsNullOrEmpty(template))
                {
                    return Result.Cancelled;
                }

                // 3. Переименовываем виды
                using (Transaction trans = new Transaction(doc, "Rename Views"))
                {
                    trans.Start();

                    foreach (View view in views)
                    {
                        // Получаем уровень вида
                        string levelName = "01"; // По умолчанию
                        Parameter levelParam = view.get_Parameter(BuiltInParameter.PLAN_VIEW_LEVEL);
                        if (levelParam != null && levelParam.AsString() != "")
                        {
                            levelName = levelParam.AsString();
                        }

                        // Заменяем плейсхолдеры в шаблоне
                        string newName = template
                            .Replace("{level}", levelName)
                            .Replace("{discipline}", "АР"); // Можно брать из параметра вида

                        // Присваиваем новое имя
                        view.Name = newName;
                    }

                    trans.Commit();
                }

                TaskDialog.Show("Успех", $"Переименовано {views.Count} видов!");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
