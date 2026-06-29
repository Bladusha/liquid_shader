# Unity Element List

Базовая папка: `Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack`

PNG экспорт для Unity лежит в `UnityPng`.

## Какой формат использовать в Unity
- Основной формат для Unity UI: `UnityPng/**/*.png`.
- SVG из исходных папок оставить как editable source/reference.
- Все PNG импортировать как `Texture Type = Sprite (2D and UI)`.
- Для кнопок, input field, dropdown, panels включить `Image Type = Sliced`.
- Текст не брать из PNG. В Unity поверх sprite добавлять `TextMeshProUGUI`.
- Для интерактива использовать `Button`, `TMP_InputField`, `TMP_Dropdown`, `CanvasGroup`.

## Prefab Elements
| Назначение | SVG source | Unity PNG | Как использовать |
| --- | --- | --- | --- |
| Главный фон меню | `backgrounds/menu_background_main.svg` | `UnityPng/backgrounds/menu_background_main.png` | Fullscreen `Image` на Canvas. |
| Фон игрового меню | `backgrounds/menu_background_ingame.svg` | `UnityPng/backgrounds/menu_background_ingame.png` | Подложка для pause/overlay меню. |
| Чистый фон | `backgrounds/menu_background_clean.svg` | `UnityPng/backgrounds/menu_background_clean.png` | Служебные экраны без лишних деталей. |
| Большая панель | `panels/panel_large_header.svg` | `UnityPng/panels/panel_large_header.png` | Основа главного окна, 9-slice. |
| Средняя боковая панель | `panels/panel_medium_side.svg` | `UnityPng/panels/panel_medium_side.png` | Боковые блоки, 9-slice. |
| Основная кнопка | `buttons/button_primary_water_frame.svg` | `UnityPng/buttons/button_primary_water_frame.png` | Единый prefab кнопки. TMP-текст сверху. |
| Input idle | `forms/input_idle.svg` | `UnityPng/forms/input_idle.png` | Обычное поле ввода. |
| Input focus | `forms/input_focus.svg` | `UnityPng/forms/input_focus.png` | Активное поле ввода. |
| Input error | `forms/input_error.svg` | `UnityPng/forms/input_error.png` | Поле с ошибкой валидации. |
| Dropdown closed | `forms/dropdown_closed.svg` | `UnityPng/forms/dropdown_closed.png` | Закрытый TMP_Dropdown. |
| Dropdown open | `forms/dropdown_open.svg` | `UnityPng/forms/dropdown_open.png` | Раскрытое состояние/list background. |
| Табличный input idle | `tables/table_input_2x1_idle.svg` | `UnityPng/tables/table_input_2x1_idle.png` | Ячейка таблицы 2:1. |
| Табличный input focus | `tables/table_input_2x1_focus.svg` | `UnityPng/tables/table_input_2x1_focus.png` | Активная ячейка таблицы. |
| Табличный input error | `tables/table_input_2x1_error.svg` | `UnityPng/tables/table_input_2x1_error.png` | Ошибка в ячейке таблицы. |
| Шапка строки | `tables/table_row_header_1x1.svg` | `UnityPng/tables/table_row_header_1x1.png` | Номер строки 1:1. |
| Шапка столбца | `tables/table_column_header_2x1.svg` | `UnityPng/tables/table_column_header_2x1.png` | Название столбца 2:1. |
| Hint K | `mini_ui/hotkey_k_show_hints.svg` | `UnityPng/mini_ui/hotkey_k_show_hints.png` | Маленький постоянный hint для открытия подсказок. |
| Полное меню хоткеев | `mini_ui/hotkey_corner_menu.svg` | `UnityPng/mini_ui/hotkey_corner_menu.png` | Оверлей в углу, открывается по K. |
| Клавиша K | `mini_ui/hotkey_key_k.svg` | `UnityPng/mini_ui/hotkey_key_k.png` | Отдельная иконка клавиши. |
| Клавиша T | `mini_ui/hotkey_key_t.svg` | `UnityPng/mini_ui/hotkey_key_t.png` | Открытие таблицы. |
| Клавиша C | `mini_ui/hotkey_key_c.svg` | `UnityPng/mini_ui/hotkey_key_c.png` | Открытие расчётов. |
| Клавиша E | `mini_ui/hotkey_key_e.svg` | `UnityPng/mini_ui/hotkey_key_e.png` | Взаимодействие. |
| WASD | `mini_ui/hotkey_wasd.svg` | `UnityPng/mini_ui/hotkey_wasd.png` | Подсказка перемещения. |
| Мышь | `mini_ui/hotkey_mouse.svg` | `UnityPng/mini_ui/hotkey_mouse.png` | Подсказка обзора/управления. |
| Иконка сообщений | `mini_ui/notification_message_icon.svg` | `UnityPng/mini_ui/notification_message_icon.png` | Маленькая кнопка/индикатор сообщений. |
| Toast | `mini_ui/notification_toast_compact.svg` | `UnityPng/mini_ui/notification_toast_compact.png` | Всплывающее уведомление. |
| Stack уведомлений | `mini_ui/notification_stack_menu.svg` | `UnityPng/mini_ui/notification_stack_menu.png` | Мини-меню уведомлений. |

## Example Screens
| Экран | SVG reference | Unity PNG reference | Что брать для production |
| --- | --- | --- | --- |
| Главное меню | `examples/example_scene_menu.svg` | `UnityPng/examples/example_scene_menu.png` | Reference layout + `backgrounds`, `panels`, `buttons`. |
| Меню внутри игры | `examples/example_ingame_menu.svg` | `UnityPng/examples/example_ingame_menu.png` | Reference layout + panels/buttons/TMP. |
| Overlay паузы | `examples/example_overlay_menu.svg` | `UnityPng/examples/example_overlay_menu.png` | Reference layout + затемнение + panels/buttons. |
| Уведомления | `examples/example_notifications_menu.svg` | `UnityPng/examples/example_notifications_menu.png` | Reference layout + `mini_ui/notification_*`. |
| Hotkeys overlay | `examples/example_corner_hotkeys_overlay.svg` | `UnityPng/examples/example_corner_hotkeys_overlay.png` | Reference layout + `mini_ui/hotkey_*`. |
| Mini notifications overlay | `examples/example_mini_notifications_overlay.svg` | `UnityPng/examples/example_mini_notifications_overlay.png` | Reference layout + toast/stack. |
| Таблица | `examples/example_table_5x7.svg` | `UnityPng/examples/example_table_5x7.png` | Reference layout + `tables/*`, TMP, GridLayoutGroup. |
| Расчёты | `examples/example_calculations_menu.svg` | `UnityPng/examples/example_calculations_menu.png` | Reference layout + `forms`, `buttons`, right `Данные` block. |
| Паспорт стенда | `examples/example_passport_menu.svg` | `UnityPng/examples/example_passport_menu.png` | Reference modal + TMP values + buttons. |

## Какие prefab создать в Unity
1. `UI_Button_WaterPrimary`: `button_primary_water_frame.png` + child `TextMeshProUGUI`.
2. `UI_Input_Water`: `input_idle/focus/error.png` + `TMP_InputField`.
3. `UI_Dropdown_Water`: `dropdown_closed/open.png` + `TMP_Dropdown`.
4. `UI_TableCell_Input`: `table_input_2x1_*` + `TMP_InputField`.
5. `UI_TableHeader_Row`: `table_row_header_1x1.png` + TMP number.
6. `UI_TableHeader_Column`: `table_column_header_2x1.png` + TMP title.
7. `UI_DataBlock`: panel image + title `Данные` + output area + dropdown records.
8. `UI_HotkeyHint_K`: `hotkey_k_show_hints.png`.
9. `UI_HotkeyMenu`: `hotkey_corner_menu.png`.
10. `UI_NotificationToast`: `notification_toast_compact.png`.
11. `UI_NotificationStack`: `notification_stack_menu.png`.
12. `UI_ModalPassport`: layout from `example_passport_menu.png`, but values/buttons must be real UI children.

## Import Settings
- `Texture Type`: `Sprite (2D and UI)`.
- `Sprite Mode`: `Single`.
- `Mesh Type`: `Full Rect` for UI.
- `Pixels Per Unit`: keep default unless project has a UI scale convention.
- `Compression`: `None` for crisp UI or `High Quality` if build size matters.
- `Filter Mode`: `Bilinear` for scaled panels, `Point` only if sharp pixel edges are required.
- For sliced elements set borders in Sprite Editor so corners and water decoration do not stretch.

## Не использовать как production prefab
- `examples/*.png` are reference/mockup screens. They are useful as visual guides, not as final interactive UI.
- Production menus should be assembled from the reusable PNG elements above plus TMP text.

## PNG Exported Files
- `UnityPng/backgrounds/menu_background_clean.png`
- `UnityPng/backgrounds/menu_background_ingame.png`
- `UnityPng/backgrounds/menu_background_main.png`
- `UnityPng/buttons/button_primary_water_frame.png`
- `UnityPng/examples/example_calculations_menu.png`
- `UnityPng/examples/example_corner_hotkeys_overlay.png`
- `UnityPng/examples/example_ingame_menu.png`
- `UnityPng/examples/example_mini_notifications_overlay.png`
- `UnityPng/examples/example_notifications_menu.png`
- `UnityPng/examples/example_overlay_menu.png`
- `UnityPng/examples/example_passport_menu.png`
- `UnityPng/examples/example_scene_menu.png`
- `UnityPng/examples/example_table_5x7.png`
- `UnityPng/forms/dropdown_closed.png`
- `UnityPng/forms/dropdown_open.png`
- `UnityPng/forms/input_error.png`
- `UnityPng/forms/input_focus.png`
- `UnityPng/forms/input_idle.png`
- `UnityPng/mini_ui/hotkey_corner_menu.png`
- `UnityPng/mini_ui/hotkey_k_show_hints.png`
- `UnityPng/mini_ui/hotkey_key_c.png`
- `UnityPng/mini_ui/hotkey_key_e.png`
- `UnityPng/mini_ui/hotkey_key_k.png`
- `UnityPng/mini_ui/hotkey_key_t.png`
- `UnityPng/mini_ui/hotkey_mouse.png`
- `UnityPng/mini_ui/hotkey_wasd.png`
- `UnityPng/mini_ui/notification_message_icon.png`
- `UnityPng/mini_ui/notification_stack_menu.png`
- `UnityPng/mini_ui/notification_toast_compact.png`
- `UnityPng/panels/panel_large_header.png`
- `UnityPng/panels/panel_medium_side.png`
- `UnityPng/tables/table_column_header_2x1.png`
- `UnityPng/tables/table_input_2x1_error.png`
- `UnityPng/tables/table_input_2x1_focus.png`
- `UnityPng/tables/table_input_2x1_idle.png`
- `UnityPng/tables/table_row_header_1x1.png`


