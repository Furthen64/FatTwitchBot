using System;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;


///
///
/// Requires: AsyncAwaitBestPractices  from nuget
/// Requires: An account on Twitch, lets call it BotAccount
/// Requires: Login to your BotAccount on Twitch, then in same browser Go to https://twitchapps.com/tmi/ and get your OAuth 
/// 
/// Fat Twitch Bot A : started working on it 2022-11-10

/// (Redeems? https://www.twitch.tv/videos/806178794?collection=E1yJPFFiSBZBrQ)
/// 
/// Alextria wants a Poll, #A, #B, #C

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
    }
  }
}



// Understanding async and await and task and concurrent programming with this simple example:

    //public static async Task method1()
    //{


    //  for (int i = 0; i < 100; i++)
    //  {
    //    Console.WriteLine("method1> " + i);

    //    if (i == 50)
    //    {
    //      await Task.Yield();    // ??? if we dont use the "await" somehow... this will run asynchronously from now on
    //    }

    //    Task.Delay(100).Wait();

    //  }
    //}


    //public static void method2()
    //{
    //  for (int i = 0; i < 100; i++)
    //  {
    //    Console.WriteLine("method2> " + i);
    //    Task.Delay(100).Wait();
    //  }
    //}

    //static async Task Main(string[] args)
    //{


    //  method1().SafeFireAndForget();
    //  method2();

      