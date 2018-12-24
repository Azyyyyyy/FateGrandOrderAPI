# The FateGrandOrderAPI NuGet lib is a small lib that allows you to get information about servants from the game Fate/Grand Order
[![Discord](https://discordapp.com/api/guilds/525688264250753025/widget.png)](https://discord.gg/F5RhrBs)

Usage: 
```cs
string name = "Jack_the_Ripper"; //It doesn't matter if the servant name has the _ for spaces but it's a good idea to have them
var persondata = await FateGrandOrderParsing.GetPerson(name, PresetsForInformation.BasicInformation);
StringBuilder servantInfo = new StringBuilder();
servantInfo.AppendLine($"Name: {persondata.BasicInformation.EnglishName}");
servantInfo.AppendLine($"Jap name: {persondata.BasicInformation.JapaneseName}");
servantInfo.AppendLine($"Gender: {persondata.BasicInformation.Gender}");
servantInfo.AppendLine($"ATK: {persondata.BasicInformation.ATK}");
servantInfo.AppendLine($"Class: {persondata.BasicInformation.Class}");
servantInfo.AppendLine($"Cost: {persondata.BasicInformation.Cost}");
Console.Write(servantInfo);
//Keep in mind there are many more things it can do than just this. This is just a small example
```

Output:
```
Name: Jack the Ripper
Jap name: ジャック・ザ・リッパー
Gender: Female
ATK: 1,786/11,557
Class: Assassin
Cost: 16
```
