# The FateGrandOrderAPI NuGet lib is a small lib that allows you to get information about servants from the game Fate/Grand Order
[![Discord](https://discordapp.com/api/guilds/525688264250753025/widget.png)](https://discord.gg/F5RhrBs)

Usage: 
```cs
string name = "Jack_the_Ripper";
StringBuilder Servant = new StringBuilder();
var servant = await FateGrandOrderParsing.GetPerson(name, PresetsForInformation.BasicInformation);
Servant.AppendLine($"Name: {servant.BasicInformation.EnglishName}");
Servant.AppendLine($"Gender: {servant.BasicInformation.Gender}");
Servant.AppendLine($"ATK: {servant.BasicInformation.ATK}");
Servant.AppendLine($"Class: {servant.BasicInformation.Class}");
Servant.AppendLine($"Cost: {servant.BasicInformation.Cost}");
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
