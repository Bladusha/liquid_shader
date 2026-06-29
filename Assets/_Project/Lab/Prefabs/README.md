# Prefabs / prefabs

Эта папка содержит prefab-ассеты, которые используются сценой `real` и связанными меню.

## Что здесь лежит
- `RealPauseMenu.prefab` - основное меню паузы для сцены `real`.
- `RealHotkeyHints.prefab` - runtime-подсказки хоткеев в углу экрана: компактная PNG-версия и анимированная открытая версия внутри одного prefab.
- `RealHotkeyHintsOpen.prefab` - отдельный готовый prefab открытой версии подсказок, если нужен статичный полный вариант без переключателя.
- `Lab01TableMenu.prefab` - меню таблицы лабораторной.
- `Lab01CalculationMenu.prefab` - меню расчётов лабораторной.

## Как использовать
- Не создавай новые копии вручную, если уже есть prefab с тем же назначением. Используй существующий asset и меняй его.
- Если нужно пересобрать runtime-версию, запусти соответствующий editor-menu item:
  - `Tools/LiquidShader/Create Real Pause Menu Prefab`
  - `Tools/LiquidShader/Create Real Hotkey Hint Prefabs`
  - `Tools/LiquidShader/Create Lab 01 Work Menu Prefabs`
- Скрипты в сцене грузят их через `Resources.Load("prefabs/<Name>")`.

## Как редактировать
- Открывай prefab напрямую из этой папки и меняй UI/тексты/размещение элементов.
- После правок проверь, что имена дочерних объектов и ссылки в контроллерах совпадают.
- Если меняешь структуру prefab, обнови соответствующий builder-скрипт, чтобы runtime-fallback создавал такую же иерархию.
