var doors = new FilteredElementCollector(doc) 
    .OfCategory(BuiltInCategory.OST_Doors) // 1. Собираем все двери
    .WhereElementIsNotElementType();  // 2. Исключаем типы (шаблоны) дверей
foreach (var door in doors)
{
    var fireRating = door.LookupParameter("Огнестойкость"); // 3. Ищем параметр
    if (fireRating == null || string.IsNullOrEmpty(fireRating.AsString())) // 4. Проверяем
        TaskDialog.Show("Ошибка", $"Дверь {door.Id} без параметра!"); // 5. Сообщаем
}
