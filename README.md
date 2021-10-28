# Localization.WPF

### Установка
Install-Package Localization.WPF -Version 1.0.0

```C#
Text="{Loc Hello_world}"
```
### Настройка

1. Создать файл параметров и добавить в него свойство 'Culture' - это будет язык по умолчанию, и тут будет сохраняться последний выбранный язык

![image](https://user-images.githubusercontent.com/44946855/139259284-24a60c98-2a91-406f-898d-6269976f6bd9.png)

2. Создать файл ресурсов - тут ресурсы локалью по умолчанию

![image](https://user-images.githubusercontent.com/44946855/139259632-12851c6c-9be1-4d14-831c-0e0577927bb2.png)

3. Создать файл ресурсов с альтернативной локалью

![image](https://user-images.githubusercontent.com/44946855/139259900-33c9bf74-0ef5-4f07-aa73-aad327dbd1c6.png)

![image](https://user-images.githubusercontent.com/44946855/139260024-78b29f39-d3e5-4825-a95e-941e7a5b0517.png)

`Имя файла должно включать название локали которую будет содержать`

4. Добавить перевод ля всех ресурсов в файл альтернативной локали

![image](https://user-images.githubusercontent.com/44946855/139260281-376649a8-0ae0-44c9-ac22-a968b70ea505.png)

5. Настроить смену языка при смене локализации

Это можно сделать при старте приложения в App.xaml.cs

![image](https://user-images.githubusercontent.com/44946855/139260546-b9528dc3-eb34-4633-b5e0-c03aee896144.png)

В 19 строчке указан текущий проект, если лужно кроме проекта сменить локаль в другой библиотеке - то и ее указать.
В 25-26 строке - сохраняем настройки при смене локализации.

###  Использование `Text="{Loc Hello_world}"`

```C#
  <TextBlock Text="{Loc Hello_world}" VerticalAlignment="Center" TextAlignment="Center"/>
```

### Смена языка

Для смены языка - используйте команду `LocalizationManager.ChangeCultureCommand`, параметр комманды - локаль на которую надо перейти (например en-US)
```Xaml
  <Button Content="En"
          Command="LocalizationManager.ChangeCultureCommand"
          CommandParameter="en-US"/>
```

Если надо изменить язык из кода:
```C#
  LocalizationManager.ChangeCulture(new_culture);
```

