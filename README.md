# The FateGrandOrderAPI NuGet lib is a small lib that allows you to get information about servants from the game Fate/Grand Order

Usage: 
                StringBuilder Servant = new StringBuilder();
                var person = FateGrandOrderParsing.GetPerson("Jack_the_Ripper");
                Servant.AppendLine($"Name: {person.BasicInfomation.EnglishName}");
                Servant.AppendLine($"Jap name: {person.BasicInfomation.JapaneseName}");
                Servant.AppendLine($"Gender: {person.BasicInfomation.Gender}");
                Servant.AppendLine($"ATK: {person.BasicInfomation.ATK}");
                Servant.AppendLine($"Class: {person.BasicInfomation.Class}");
                Servant.AppendLine($"Cost: {person.BasicInfomation.Cost}");
                Console.WriteLine(Servant);
                
                //Keep in mind there are many more things it can do than just this. This is just a small example

Output:
                Will be added soon
