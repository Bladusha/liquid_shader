# Menu Script Export

Оптимизированные и переименованные копии меню-скриптов собраны в этой папке специально вне `Assets`, чтобы не создавать дубли классов в текущем Unity-проекте.

## Что уже сделано

- Экспортные `.cs` приведены к `UTF-8`.
- Скрипты переименованы так, чтобы по имени было понятнее назначение.
- Убрана лишняя прослойка `MenuCursorController`: её стартовая логика перенесена в `MenuPanelManager`.
- В ряде файлов убраны лишние `Find*`, повторные подписки и небезопасные `RemoveAllListeners()`.
- Добавлен генератор сцена-пака `Tools/LiquidShader/Build Menu Scene Pack`.
- Сценарий меню теперь разбит на отдельные сцены: `MenuEntry`, `MenuPlayerData`, `MenuProtocol`, `MenuHub`, `MenuHelp`.
- В `MenuEntry` добавлен выпадающий список выбора лабораторной работы, пока с одной опцией.

## Проверка исходного проекта

- Базовый Unity-проект компилировался в batchmode на `6000.0.62f1` без script errors.
- В логах были только сетевые сообщения Unity cloud, не связанные со скриптами.

## Новая структура

### MenuSceneScripts

- `MenuPanelManager.cs`
  Управляет набором панелей в меню-сцене: скрывает все панели, показывает нужную по имени или индексу, может сразу открыть стартовую панель и настроить курсор.
- `MenuPanelSwitchButton.cs`
  Вешается на UI-кнопку и переключает панель через `MenuPanelManager`.
- `PlayerDataMenuController.cs`
  Собирает данные игрока из `TMP_InputField`, валидирует обязательные поля, сохраняет их в `PlayerPrefs` и переводит пользователя на следующую панель.
- `SceneLoadButton.cs`
  Кнопка загрузки сцены. Проверяет, что сцена есть в `Build Settings`, и загружает её синхронно или асинхронно.

### UI_scripts_for_UI

- `PanelInputAutoSave.cs`
  Автосохранение и автозагрузка `TMP_InputField` внутри панели через `PlayerPrefs`.
- `PanelPrefabOpenButton.cs`
  Открывает новый UI-префаб по кнопке, при необходимости скрывая текущую панель.
- `PauseMenuRuntimeController.cs`
  Открывает и закрывает pause menu во время игры, ставит игру на паузу через `Time.timeScale`, находит кнопку продолжения и восстанавливает состояние курсора.
- `PdfMenuOpenButton.cs`
  Открывает PDF viewer поверх текущего `Canvas`, скрывает предыдущее меню и восстанавливает его после закрытия viewer.
- `PdfViewerPanelController.cs`
  Управляет окном просмотра PDF: создаёт страницы из массива `Sprite`, обрабатывает закрытие и связь с кнопкой-открывателем.
- `SceneRestartButton.cs`
  Перезапускает текущую сцену, при необходимости чистит сохранённые данные панели или полностью сбрасывает `PlayerPrefs`.
- `SlitWidthInteractionController.cs`
  Управляет интерактивной настройкой ширины щели: блокирует обычное управление, переводит камеру в точку просмотра, меняет значение мышью и сохраняет его.

### scripts

- `PlayerInteractionMenuController.cs`
  Открывает игровое меню по клавише, создаёт его из префаба, временно отключает управление игроком и пробрасывает нажатия кнопок в привязанные `MonoBehaviour`.
- `PositionSliderMenuController.cs`
  Универсальное окно со слайдером для настройки значения: показывает текущее число, подтверждает или отменяет изменение и временно отключает управление игроком.
- `WorkzoneSelectionController.cs`
  Основная система рабочей зоны: включает специальный режим, ограничивает перемещение игрока, делает raycast по объектам, подсвечивает их и открывает меню взаимодействия.

## Что важно при переносе

- Нужны зависимости `UnityEngine.UI` и местами `TextMeshPro`.
- `PlayerInteractionMenuController`, `WorkzoneSelectionController` и `SlitWidthInteractionController` завязаны на `EasyPeasyFirstPersonController`.
- `WorkzoneSelectionController` и `SlitWidthInteractionController` используют `Outline`.
- Для полного переноса логики одних `.cs` может быть недостаточно: могут понадобиться связанные `prefab`, `scene`, `Canvas`, `Button`, `TMP_InputField` и другие UI-объекты.
