# Hydrodynamics Water UI Pack

Отдельный Unity UI-пак для лабораторной работы по гидродинамике. Основа визуального языка: белые лабораторные поверхности, голубая вода, волны, мягкая глубина и оранжевые акценты для важных действий.

## Что внутри
- `backgrounds/menu_background_main.svg`: фон главного меню.
- `backgrounds/menu_background_ingame.svg`: фон меню внутри игры.
- `backgrounds/menu_background_clean.svg`: чистый фон под текстовые или служебные сцены.
- `buttons/button_primary_water_frame.svg`: единственный утверждённый вариант кнопки без запеченного текста.
- `forms/input_*.svg`: состояния input field.
- `forms/dropdown_*.svg`: закрытый и открытый dropdown.
- `tables/table_input_2x1_*.svg`: компактные input field для таблиц, формат 160×80.
- `tables/table_row_header_1x1.svg`: квадратная шапка строки 80×80 под номер строки.
- `tables/table_column_header_2x1.svg`: шапка столбца 160×80.
- `mini_ui/hotkey_corner_menu.svg`: маленькое меню подсказок для угла экрана.
- `mini_ui/hotkey_k_show_hints.svg`: отдельное мини-меню `K - показать подсказки`.
- `mini_ui/hotkey_key_*.svg`: отдельные иконки клавиш K/T/C/E.
- `mini_ui/hotkey_wasd.svg`: иконка WASD для перемещения.
- `mini_ui/hotkey_mouse.svg`: иконка мыши для обзора/управления.
- `mini_ui/notification_*.svg`: мини-всплывающие уведомления и иконка сообщения.
- `examples/example_scene_menu.svg`: собранный пример отдельной сцены меню.
- `examples/example_ingame_menu.svg`: собранный пример меню внутри игры.
- `examples/example_overlay_menu.svg`: собранный пример overlay-меню поверх сцены.
- `examples/example_notifications_menu.svg`: собранный пример меню уведомлений.
- `examples/example_corner_hotkeys_overlay.svg`: пример подсказок в углу игрового экрана.
- `examples/example_mini_notifications_overlay.svg`: пример всплывающих уведомлений.
- `examples/example_table_5x7.svg`: тестовое меню таблицы по референсу: 9 столбцов, 2 строки данных.
- `examples/example_calculations_menu.svg`: пример меню расчётов в новом сером игровом стиле.
- `examples/example_passport_menu.svg`: пример паспорта лабораторного стенда в новом сером стиле.
- `preview.html`: быстрый просмотр ключевых элементов.

## Правила Unity
- Кнопки, input field, dropdown и панели использовать как `Image` с `9-slice`.
- Текст добавлять отдельным `TextMeshProUGUI`, не запекать в SVG.
- Для кнопок использовать `active scale = 0.96`; hover/pressed делать через overlay или tint.
- Для числовых значений в гидродинамике использовать tabular numbers.
- Для вложенных поверхностей сохранять концентрические радиусы: внешний радиус больше внутреннего на величину padding.

## Сборка меню по блокам

Базовая папка пака: `C:\Users\bodro\OneDrive\Рабочий стол\горбач\unity_main_menu_art\hydrodynamics_water_ui_pack`.

### Общая схема Canvas
- Создать `Canvas` в режиме `Screen Space - Overlay` или `Screen Space - Camera`.
- Добавить корневой `Panel/MenuRoot` на весь экран.
- Фон брать из `backgrounds` или использовать серый runtime-фон для игровых overlay-окон.
- Все SVG-элементы импортировать как `Sprite (2D and UI)`.
- Для растягиваемых элементов включать `Image Type = Sliced`.
- Весь текст меню делать через `TextMeshProUGUI`, чтобы другой агент мог менять язык, значения и размер без перерисовки SVG.
- Интерактивные элементы держать с hit area не меньше `44×44`.

### Главное меню
- Пример: `examples/example_scene_menu.svg`.
- Фон: `backgrounds/menu_background_main.svg`.
- Основа большого окна: `panels/panel_large_header.svg`.
- Кнопка: `buttons/button_primary_water_frame.svg`.
- Логотип и название заведения в примере уже показаны как композиция; в Unity лучше собрать их отдельными `Image` + `TextMeshProUGUI`.
- Кнопки `НАЧАТЬ`, `ТЕОРИЯ`, `ПРОТОКОЛ`, `НАСТРОЙКИ` собирать одним и тем же sprite кнопки, меняя только TMP-текст.

### Меню внутри игры
- Пример: `examples/example_ingame_menu.svg`.
- Фон/подложка: `backgrounds/menu_background_ingame.svg`.
- Левая панель паузы: `panels/panel_medium_side.svg` или локальная Image-панель по образцу примера.
- Кнопки действий: `buttons/button_primary_water_frame.svg`.
- Правый блок параметров собирать как обычную панель + TMP-значения, не запекать числа в картинку.

### Overlay паузы
- Пример: `examples/example_overlay_menu.svg`.
- Использовать затемнение сцены отдельным полупрозрачным `Image` поверх 3D-вида.
- Центральную панель делать из `panels/panel_large_header.svg` или из собственного `Image` по примеру.
- Для действий `Продолжить`, `Протокол`, `Настройки`, `Выйти` использовать только `buttons/button_primary_water_frame.svg`.
- Анимация появления: `CanvasGroup.alpha 0 -> 1`, `scale 0.98 -> 1`, `duration 0.20-0.30s`.

### Таблица измерений
- Пример: `examples/example_table_5x7.svg`.
- Табличные prefab-элементы: папка `tables`.
- Ячейка ввода: `tables/table_input_2x1_idle.svg`, `tables/table_input_2x1_focus.svg`, `tables/table_input_2x1_error.svg`.
- Шапка строки: `tables/table_row_header_1x1.svg`.
- Шапка столбца: `tables/table_column_header_2x1.svg`.
- Для текущего примера использовать структуру: 9 столбцов, 2 строки данных, общий внешний контур, минимальные зазоры.
- В Unity сетку лучше собирать через `GridLayoutGroup` или вручную: внутренние ячейки квадратные по углам, скругления только у крайних внешних углов.
- Подписи столбцов и номера строк делать TMP-текстом поверх соответствующих Image.

### Левый блок данных в таблице
- Использовать паттерн из `examples/example_table_5x7.svg`.
- Верх: TMP `Данные`.
- Середина: пустая область вывода на `Image`-панели.
- Низ: dropdown с номерами записей.
- Для dropdown можно использовать `forms/dropdown_closed.svg` и `forms/dropdown_open.svg`, если нужен отдельный prefab, или собрать как в примере из простых `Image` + TMP.

### Меню расчётов
- Пример: `examples/example_calculations_menu.svg`.
- Input fields расчётов собирать на основе `forms/input_idle.svg`, `forms/input_focus.svg`, `forms/input_error.svg`, либо использовать увеличенную композицию из примера.
- Подписи `d`, `f`, `V`, `w`, `t`, `ν`, `Re` держать TMP-текстом.
- Кнопки проверки собирать из `buttons/button_primary_water_frame.svg` или по форме кнопок из примера, но текст всегда отдельным TMP.
- Правый блок `Данные` повторяет блок таблицы: пустая область вывода + dropdown `Запись 1/2/3`.
- Если текст кнопки длинный, сначала увеличить ширину кнопки, а не уменьшать шрифт ниже читаемого размера.

### Паспорт лабораторного стенда
- Пример: `examples/example_passport_menu.svg`.
- Использовать как модальное окно поверх сцены после выбора строки или расчёта.
- Значения `d`, `f`, `V`, `w`, `t`, `ν`, `Re`, `Режим` выводить TMP-текстом в карточках.
- Кнопки: `Записать данные`, `Перейти к расчётам`, `Закрыть`.
- Окно должно открываться как modal: затемнение сцены, `CanvasGroup.alpha 0 -> 1`, лёгкий `scale 0.98 -> 1`.

### Подсказки и хоткеи
- Полное меню: `mini_ui/hotkey_corner_menu.svg`.
- Постоянная короткая подсказка: `mini_ui/hotkey_k_show_hints.svg`.
- Отдельные клавиши: `mini_ui/hotkey_key_k.svg`, `mini_ui/hotkey_key_t.svg`, `mini_ui/hotkey_key_c.svg`, `mini_ui/hotkey_key_e.svg`.
- Движение: `mini_ui/hotkey_wasd.svg`.
- Мышь: `mini_ui/hotkey_mouse.svg`.
- Пример overlay: `examples/example_corner_hotkeys_overlay.svg`.
- Логика: короткий hint `K` виден постоянно, полное меню открывается по `K`.

### Уведомления
- Иконка сообщений: `mini_ui/notification_message_icon.svg`.
- Одиночный toast: `mini_ui/notification_toast_compact.svg`.
- Стек уведомлений: `mini_ui/notification_stack_menu.svg`.
- Примеры: `examples/example_notifications_menu.svg`, `examples/example_mini_notifications_overlay.svg`.
- Для всплывания использовать `alpha 0 -> 1`, `anchoredPosition.y -12 -> 0`, скрытие быстрее появления.

### Формы и dropdown
- Обычное поле: `forms/input_idle.svg`.
- Активное поле: `forms/input_focus.svg`.
- Ошибка: `forms/input_error.svg`.
- Закрытый список: `forms/dropdown_closed.svg`.
- Открытый список: `forms/dropdown_open.svg`.
- Ошибки валидации показывать заменой sprite на `input_error.svg` и TMP-сообщением рядом, а не изменением запечённого текста.

### Импорт в Unity
- Для SVG через Vector Graphics package: импортировать как `Vector Sprite`, затем использовать в `Image`.
- Для 9-slice задать border вручную в Sprite Editor: оставить нерастягиваемыми скругления и декоративную воду.
- Для крупных меню лучше держать примеры из `examples` как визуальный reference, а production UI собирать из отдельных `buttons`, `forms`, `tables`, `mini_ui`, `panels`.
- Если агенту нужно быстро собрать экран, он должен сначала открыть соответствующий `examples/*.svg`, затем заменить baked layout на реальные Unity prefabs по путям выше.

## Назначение кнопок
- `button_primary_water_frame.svg`: единый стиль кнопки для всех действий. Отличать действия нужно TMP-текстом, размером, состоянием hover/pressed/disabled или дополнительным overlay, а не другим дизайном кнопки.

## Назначение форм
- `input_idle.svg`: обычное поле.
- `input_focus.svg`: активное поле ввода.
- `input_error.svg`: ошибка валидации.
- `dropdown_closed.svg`: закрытый список.
- `dropdown_open.svg`: раскрытый список.

## Мини-меню подсказок
- `hotkey_corner_menu.svg`: готовая компактная панель для угла экрана. Использовать как overlay-панель, например справа снизу или справа сверху.
- `hotkey_k_show_hints.svg`: компактный постоянно видимый hint для клавиши `K`. Использовать, когда полное меню подсказок скрыто.
- `hotkey_key_k.svg`: клавиша `K`, открытие меню подсказок.
- `hotkey_key_t.svg`: клавиша `T`, открытие таблицы измерений.
- `hotkey_key_c.svg`: клавиша `C`, открытие расчётов. Использована латинская `C`, чтобы Unity Input System не путал её с кириллической `С`.
- `hotkey_key_e.svg`: клавиша `E`, взаимодействие с установкой.
- `hotkey_wasd.svg`: движение игрока/камеры.
- `hotkey_mouse.svg`: управление обзором мышью.

## Анимация появления подсказок
- Скрытое состояние `hotkey_corner_menu`: `CanvasGroup.alpha = 0`, `RectTransform.anchoredPosition.y = -12`, `localScale = 0.98`.
- Появление: за `0.30s` довести `alpha` до `1`, `anchoredPosition.y` до `0`, `localScale` до `1`, easing `cubic-bezier(0.2, 0, 0, 1)` или Unity `Ease.OutCubic`.
- Контент показывать stagger-группами: заголовок `0ms`, строки клавиш `80ms`, блоки `WASD/мышь` `160ms`.
- Скрытие: `0.15s`, `alpha = 0`, `anchoredPosition.y = -12`, без сильного scale. Это мягче и не отвлекает от сцены.
- Для клика по клавише/иконке использовать press-scale `0.96`; не уменьшать сильнее.

## Мини-уведомления
- `notification_message_icon.svg`: маленькая иконка сообщения для кнопки/индикатора уведомлений.
- `notification_toast_compact.svg`: одиночный всплывающий toast. Текст держать отдельным TMP, если нужна локализация.
- `notification_stack_menu.svg`: компактная стопка сообщений как всплывающее мини-меню.

## Таблицы
- Все табличные input field имеют соотношение 2:1 (`160×80`) и рассчитаны на плотную таблицу с большим количеством значений.
- `table_input_2x1_idle.svg`: обычная ячейка ввода.
- `table_input_2x1_focus.svg`: активная ячейка ввода.
- `table_input_2x1_error.svg`: ошибка валидации значения.
- `table_row_header_1x1.svg`: квадратная подпись строки, использовать только под номер строки.
- `table_column_header_2x1.svg`: подпись столбца, размер совпадает с input field.
- `example_table_5x7.svg`: пример плотной сетки по референсу: 9 столбцов и 2 строки данных. Зазор между input field минимальный: ячейки стоят вплотную, разделяются только линиями сетки. Скругления нужны только на внешних углах таблицы; внутренние ячейки должны быть квадратными.
- `example_calculations_menu.svg`: пример меню расчётов. Правый блок повторяет паттерн таблицы: `Данные`, пустая область вывода и dropdown с номерами записей.
- `example_passport_menu.svg`: пример паспорта лабораторного стенда. Значения разнесены по карточкам в две колонки, кнопки увеличены и не пересекаются с текстом.
