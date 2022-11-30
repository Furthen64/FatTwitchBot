using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;


///
///
/// Requires: AsyncAwaitBestPractices  from nuget
/// REquires: An account on Twitch, lets call it BotAccount
/// Requires: Login to your BotAccount on Twitch, then in same browser Go to https://twitchapps.com/tmi/ and get your OAuth 
/// 
/// Fat Twitch Bot A : started working on it 2022-11-10

/// (Redeems? https://www.twitch.tv/videos/806178794?collection=E1yJPFFiSBZBrQ)
/// 
/// Alextria wants a Poll, #a, #b, #c

namespace SimpleTwitchBot
{
  class Program
  { 

    static async Task Main(string[] args)
    {
      TwitchBott twBot = new TwitchBott();
      Console.WriteLine("program> runBotAsync");
      Task runbotTask = twBot.runBotAsync();    // Fire off and move on  
      
      await runbotTask;         // Thread.Delay with Wait for the Task to return

      Console.WriteLine("program> exiting");

      

        /* exempel med frukost:
        pourCoffee();
        console

        Task<Egg> eggTask  = fryEggsAsync(3); // async!
        Task<Bacon baconTask = fryBaconAsync(2); 
        Task<Bread> breadTask = toastBreadAsync(3);


        KLASSTYPEN

        Bread breadObj = await breadTask; // bread popped!
        spreadButter
        spreadJam
        console


        Egg eggObj = await eggTask; // sequential
        console egg done
        */



    }
  }
}