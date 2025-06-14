var doors = new FilteredElementCollector(doc)
    .OfCategory(BuiltInCategory.OST_Doors)
    .WhereElementIsNotElementType();
foreach (var door in doors)
{
    var fireRating = door.LookupParameter("Огнестойкость");
    if (fireRating == null || string.IsNullOrEmpty(fireRating.AsString()))
        TaskDialog.Show("Ошибка", $"Дверь {door.Id} без параметра!");
}
