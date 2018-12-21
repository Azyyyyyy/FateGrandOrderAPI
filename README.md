# The FateGrandOrderAPI NuGet lib is a small lib that allows you to get information about servants from the game Fate/Grand Order
[![Discord](https://discordapp.com/api/guilds/525688264250753025/widget.png)](https://discord.gg/F5RhrBs)

Usage: 
```cs
StringBuilder Servant = new StringBuilder();
var person = FateGrandOrderParsing.GetPerson("Jack_the_Ripper");
Servant.AppendLine($"Name: {person.BasicInfomation.EnglishName}");
Servant.AppendLine($"Gender: {person.BasicInfomation.Gender}");
Servant.AppendLine($"ATK: {person.BasicInfomation.ATK}");
Servant.AppendLine($"Class: {person.BasicInfomation.Class}");
Servant.AppendLine($"Cost: {person.BasicInfomation.Cost}");
Console.WriteLine(Servant);
//Keep in mind there are many more things it can do than just this. This is just a small example
```

Output:
```
Name: Jack the Ripper
Gender: Female
ATK: 1,786/11,557
Class: Assassin
Cost: 16
```
