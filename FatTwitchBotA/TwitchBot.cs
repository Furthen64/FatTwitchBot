using AsyncAwaitBestPractices;
using FatTwitchBotA;
using FatTwitchBotA.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq.Expressions;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SimpleTwitchBot
{

  
  /// <summary>
  /// TwitchBott wraps the TwitchBotCore for easy use.
  /// </summary>
  public class TwitchBott
  {
    private TwitchConfig? cfg = null;
    private Poll? poll = null;

    private System.Timers.Timer? pollPollTimer;
    private static int POLLPOLL_TIMER_SECONDS = 10;


    public TwitchBott()
    {
      initialize();
    }


    // (-+)
    public void initialize()
    {
      poll = new Poll("alextriaPoll", 1, 1);

      cfg = new TwitchConfig("appsettings.json"); // Bot settings are in this file, user and OAUTH password      
      cfg.channelname = "fayt64";

      var twBotCore = TwitchBotCore.Instance;
      twBotCore.initialize(cfg); 
    }

    // (--+)     
    // Main bot logic is here:
    //
    // > Join Channel
    // > loop
    //   {
    //      > Handle each type of !command
    //      > Poll: Count votes 
    //      > Poll: Issue Poll Timers
    //   } 

    public async Task runBotAsync()  // Does not return value
    {
      // TODO: this code can get stuck forever waiting!
      var twBotCore = TwitchBotCore.Instance; // singleton

      Console.WriteLine("runBotAsync> running SafeFireAndForget:");
      await Task.Delay(500);
      
      
      
      twBotCore.Start().SafeFireAndForget(); 

      // .... but because of the way .NET handles Tasks, the Task is run synchronously until the first await keyword is reached.
      // https://github.com/brminnick/AsyncAwaitBestPractices/issues/65




      Console.WriteLine("runBotAsync> done");

      // sanity checks
      if (cfg.channelname == "")
      {
        cfg.channelname = "swol_69";// DEFAULT 
      }
      await twBotCore.JoinChannel(cfg.channelname); // Sequential 
      await twBotCore.SendMessage(cfg.channelname, "sup nerds"); // Sequential 

      // 
      // TwitchBotCore creates OnMessage events and then we end up here:
      //

      twBotCore.OnMessage += async (sender, twitchChatMessage)
      =>
      {
        // Sanitize input!
        string chatMsgSafe = UserInput.cleanString(twitchChatMessage.Message.ToLower());
        string username = twitchChatMessage.Sender;

        Log($"TwitchBott onMessage> {twitchChatMessage.Sender} said '{twitchChatMessage.Message}'");


        // Poll

        if (poll.isRunning && chatMsgSafe.StartsWith("#"))
        {
          poll.handleVote(username, chatMsgSafe);
        }


        if (twitchChatMessage.Message.ToLower().StartsWith("!startpoll"))
        {          
          if(poll.isRunning) {
            await twBotCore.SendMessage(cfg.channelname, "Poll is on cooldown or already running.");
          } else {
          
            await poll.tryStart(cfg.channelname);         
          
            // Keep checking on the poll 
            pollPollTimer = new System.Timers.Timer(POLLPOLL_TIMER_SECONDS * 1000.0);           
            pollPollTimer.Elapsed += async (sender, e) =>
            {
              if (poll.hasPollFinished)
              {
                if(poll.resultKey != "") {
                  
                  // Activate PHOTOSHOP INTEGRATION!!
                  if(poll.resultKey == "a") {
                    await twBotCore.SendMessage(cfg.channelname, "TODO A");
                  }


                  
                  


                  

                }
                await twBotCore.SendMessage(cfg.channelname, poll.resultMessage);
                pollPollTimer.Stop();
              }
              else
              {
                Console.WriteLine("pollpolltimer anon> Not done yet");
              }
            };

            pollPollTimer.AutoReset = true;
            pollPollTimer.Enabled = true; 
          } 
          
        }
         
        //Listen for !hey command
        if (twitchChatMessage.Message.StartsWith("!hey"))
        {
          await twBotCore.SendMessage(twitchChatMessage.Channel, $"Hey there {twitchChatMessage.Sender}");
        }

        if (twitchChatMessage.Sender == "fayt64")
        {
          if (twitchChatMessage.Message.StartsWith("!quit"))
          {
            await twBotCore.SendMessage(cfg.channelname, "byeeeeeeee");
            await Task.Delay(500);
            twBotCore.quit(); ;
          }

        }
      }; // </anonymous function>



      // Check once every second 
      while (!twBotCore.isQuitting())
      {
        await Task.Delay(1000);
      }

      return;
    }

    public static void Log(string msg)
    {
      Console.WriteLine(msg);
    }

  }

  // 2022.11.27 trying to make it Singleton and Threadsafe
  // https://www.c-sharpcorner.com/UploadFile/8911c4/singleton-design-pattern-in-C-Sharp/


  // Used by TwitchBott to handle all Twitch IRC traffic
  // (---)
  public sealed class TwitchBotCore
  {
    const string ip = "irc.chat.twitch.tv";
    const int port = 6697;

    private TwitchConfig? twSettings = null;

    private bool quitting = false;
    private StreamReader streamReader;
    private StreamWriter streamWriter;
    private TaskCompletionSource<int> connected = new TaskCompletionSource<int>();

    public event TwitchChatEventHandler OnMessage = delegate { };
    public delegate void TwitchChatEventHandler(object sender, TwitchChatMessage e);

    private DateTime startTime;



    // (---) 
    // Authenticates
    // Handles PING/PONG
    // Loop until quitting flag:
    // {
    //   handle message
    //   parse !commands
    //   create OnMessage(..TwitchChatMessage) events that TwitchBott reacts to 
    // }

    public async Task Start()
    {
      await Task.Yield(); // Trick to start the concurrency in .NET 

     
      // Sanity check
      if(twSettings == null)  
      {
           Console.WriteLine("TwitchBotCore> Start failed, twSettings is null.");
        return ;
      }

      try
      {
        // Connect to Twitch
        var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(ip, port);
        SslStream sslStream = new SslStream(
            tcpClient.GetStream(),
            false,
            ValidateServerCertificate,
            null
        );


        await sslStream.AuthenticateAsClientAsync(ip);

        streamReader = new StreamReader(sslStream);
        streamWriter = new StreamWriter(sslStream) { NewLine = "\r\n", AutoFlush = true };

        await streamWriter.WriteLineAsync($"PASS {twSettings.password}");
        await streamWriter.WriteLineAsync($"NICK {twSettings.botUsername}");

        connected.SetResult(0);
        startTime = DateTime.Now;

        while (!quitting)
        {

          String line = await streamReader.ReadLineAsync();

          // Parse message:
          if (line != null)
          {
            Console.WriteLine(line);
            string[] split = line.Split(" ");

            if (line.StartsWith("PING"))
            {
              Console.WriteLine($"TwitchBotCore {DateTime.Now}> PING? PONG!");
              await streamWriter.WriteLineAsync($"PONG {split[1]}");
            }

            if (split.Length > 2 && split[1] == "PRIVMSG")
            {
              //:mytwitchchannel!mytwitchchannel@mytwitchchannel.tmi.twitch.tv 
              // ^^^^^^^^
              //Grab this name here
              int exclamationPointPosition = split[0].IndexOf("!");
              string username = split[0].Substring(1, exclamationPointPosition - 1);
              //Skip the first character, the first colon, then find the next colon
              int secondColonPosition = line.IndexOf(':', 1);//the 1 here is what skips the first character
              string message = line.Substring(secondColonPosition + 1);//Everything past the second colon
              string channel = split[2].TrimStart('#');

              OnMessage(this, new TwitchChatMessage
              {
                Message = message,
                Sender = username,
                Channel = channel
              });
            }
          }

        }

        Console.WriteLine($"TwitchBotCore  {DateTime.Now}> Quitting.");

      }
      catch (Exception ex)
      {
        Console.WriteLine($"ERROR {DateTime.Now}> Exception ahoy! ");
        Console.WriteLine(ex.Message);
      }
    }

    public class TwitchChatMessage : EventArgs
    {
      public string? Sender { get; set; }
      public string? Message { get; set; }
      public string? Channel { get; set; }
    }


    public void initialize(TwitchConfig? twSettings)
    {
      Console.WriteLine("TwitchBotCore> Initialize! only once i hope");
      if (twSettings == null)
      {
        throw new Exception("TwitchBotCore> twSettings is null!");
      }
      this.twSettings = twSettings;
    }
    
    // untested
    public string getUptime()
    {
      return (DateTime.Now.TimeOfDay - this.startTime.TimeOfDay).ToString();
    }


    public void quit()
    {
      quitting = true;
    }

    // Outside of start we need to define ValidateServerCertificate
    private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
      return sslPolicyErrors == SslPolicyErrors.None;
    }


    public async Task SendMessage(string channel, string message)
    {
      await connected.Task;
      await streamWriter.WriteLineAsync($"PRIVMSG #{channel} :{message}");
    }

    public async Task JoinChannel(string channel)
    {
      await connected.Task;
      await streamWriter.WriteLineAsync($"JOIN #{channel}");
    }


    public bool isQuitting()
    {
      return quitting;
    }



    TwitchBotCore()
    {
      Console.WriteLine("TwitchBotCore> Constructor!");
    }
    private static readonly object lockObj = new object();
    private static TwitchBotCore instance = null;
    public static TwitchBotCore Instance
    {
      get
      {
        if (instance == null)
        {
          lock (lockObj)
          {
            if (instance == null)
            {
              instance = new TwitchBotCore();
            }
          }
        }
        return instance;
      }
    }
  }




  public class Poll
  {
    public enum PollState { POLL_INIT, POLL_RUNNING, POLL_FINISHED };
    private PollState currState = PollState.POLL_INIT;
    private TimeSpan pollSpan;

    private TimeSpan cooldownSpan;


    private string pollName = "unnamed";
    public string resultMessage = "";
    public string resultKey = "";

    private System.Timers.Timer? aTimer;
    private System.Timers.Timer? bTimer;
    
    private DateTime pollStartedTime;
    private DateTime timeElapsed;
    private static int POLL_TIMER_MINUTES = 0; // 1
    private static int POLL_TIMER_SECONDS = 30;  // 30
    private static int POLL_TIMER_TOTAL_SECONDS = (POLL_TIMER_MINUTES * 60) + (POLL_TIMER_SECONDS);

    private List<string> options;
    private Dictionary<string, int> votingResults;
    
    private List<string> usersAlreadyVoted;





    // (-+)
    public Poll(string pollName, uint pollSpanSec, uint cooldownSpanSec)
    {
      aTimer = new System.Timers.Timer();
      this.pollName = pollName;
      this.pollSpan = TimeSpan.FromSeconds(pollSpanSec);
      this.cooldownSpan = TimeSpan.FromSeconds(cooldownSpanSec);
      currState = PollState.POLL_INIT;



      options = new List<string>();
      {
        options.Add("#A = Clear current layer");
        options.Add("#B = Circles");
        options.Add("#C = Lines");
      }
      votingResults = new Dictionary<string, int>();
      votingResults.Add("a", 0);
      votingResults.Add("b", 0);
      votingResults.Add("c", 0);


       

      usersAlreadyVoted = new List<string>();
    


    }



    public bool isRunning { get { return currState == PollState.POLL_RUNNING; } }

    public bool hasPollFinished { get { return currState == PollState.POLL_FINISHED; } }

    // (--+)
    public async Task tryStart(string channelname)
    {
      var twBotCore = TwitchBotCore.Instance;

      if (isRunning)
      {
        await twBotCore.SendMessage(channelname, "Poll is on cooldown or already running.");
      }
      else
      {

        if (activate(twBotCore))
        {          
          await twBotCore.SendMessage(channelname, "A Photoshop poll has started! DinkDonk #A = Clear current layer, #B = Circles, #C = Lines");
        }
        else
        {
          await twBotCore.SendMessage(channelname, "Something went wrong with startPoll ");
        }
      }

    }

    // (-+)
    public bool handleVote(string username, string msgLowSafe)
    {
      bool result = false;
      if (usersAlreadyVoted.Contains(username))
      {
        Console.WriteLine("FIXME undo this comment:");
        //  return false;
      }
      string voteMsg = "";

      if (msgLowSafe.Length > 1)
      {
        voteMsg = msgLowSafe.Trim().Substring(0, 2).Replace('#', ' ').Trim();
      }
      else
      {
        voteMsg = msgLowSafe;
      }

      if (voteMsg == "a" || voteMsg == "b" || voteMsg == "c")
      {
        if (votingResults.ContainsKey(voteMsg))
        {
          votingResults[voteMsg]++;
        }
        result = true;
      }



      if (result)
      { 
        usersAlreadyVoted.Add(username);
      }
      return result;
    }




    // (untested)
    public void doReset()
    {
      timeElapsed = DateTime.UtcNow;
      Console.WriteLine("Reset timer for poll");
      usersAlreadyVoted.Clear();
      votingResults.Clear();

      votingResults.Add("a", 0);
      votingResults.Add("b", 0);
      votingResults.Add("c", 0);

    }




    // Returns true if activation is successful 
    // (untested)
    public bool activate(TwitchBotCore twBot)
    {
      if (!isRunning)
      {
        this.currState = PollState.POLL_RUNNING;
        pollStartedTime = DateTime.UtcNow;


        // Timer 
        if (POLL_TIMER_TOTAL_SECONDS > 30)
        {
          // Also create one that tells us how much time is left
          bTimer = new System.Timers.Timer(POLL_TIMER_SECONDS * 1000.0); // every ten seconds tells us how much time is left          
          bTimer.Elapsed += OnTimedEventB;   // Hook up the Elapsed event for the timer. 
          bTimer.AutoReset = true;
          bTimer.Enabled = true;
        }

        Console.WriteLine("Timer started for " + POLL_TIMER_TOTAL_SECONDS + " seconds");
        aTimer = new System.Timers.Timer(POLL_TIMER_TOTAL_SECONDS * 1000.0);
        aTimer.Elapsed += OnTimedEventA;   // Hook up the Elapsed event for the timer. 
        aTimer.AutoReset = true;
        aTimer.Enabled = true;

        return true;
      }

      return false;
    }



    // (--) Poll ended timer, setup in "Activate" 
    private void OnTimedEventA(Object? source, ElapsedEventArgs e)
    {
      if (aTimer == null)
      {
        return;
      }



      int maxVal = 0;
      string votedOption = "nothing";
      foreach (var x in votingResults)
      {
        if (x.Value > maxVal)
        {
          maxVal = x.Value;
          votedOption = x.Key;
        }
      }

      // Check if it tied with something else
      int nrOfMaxers = 0;
      List<string> tieResult = new List<string>();
      foreach (var x in votingResults)
      {
        if (x.Value == maxVal)
        {
          nrOfMaxers++;
          tieResult.Add(x.Key);
        }
      }

      if (nrOfMaxers > 1)
      {
        resultMessage = "Poll ended, but it was a tie between ";
        foreach (string str in tieResult)
        {
          resultMessage += str.ToUpper() + " and ";
        }
        resultMessage = String.Empty + resultMessage.Substring(0, resultMessage.Length - 4); // remove trailing and
        resultMessage += ". Try again!";

        resultKey = "";
      }
      else
      {

        // Normal result:
        resultKey = votedOption.ToUpper();
        resultMessage = "Poll ended! " + votedOption.ToUpper() + " won with " + maxVal + " votes.";
      }


      Console.WriteLine("Poll ended!");

      aTimer.Enabled = false;
      if (bTimer != null)
      {
        bTimer.Enabled = false;
      }
      doReset();
      currState = PollState.POLL_FINISHED;

    }

    private void OnTimedEventB(Object? source, ElapsedEventArgs e)
    {
      TimeSpan tspan = TimeSpan.FromSeconds(POLL_TIMER_TOTAL_SECONDS) - (DateTime.UtcNow - pollStartedTime);
      //int remainingTimeSecs = POLL_TIMER_TOTAL_SECONDS - (int)Math.Round(tspan.TotalSeconds);
      Console.WriteLine("Time left: " + (int)Math.Round(tspan.TotalSeconds) + " seconds ");

    }
  }


  
  // Java:
  public interface Trigger
  {

    /**
		 *
		 * @param nick The user who trigged the trigger
		 * @param hostmask The hostmask of the user
		 * @param channel Trigged in this channel
		 * @param trigger The trigger used and all arguments
		 */
    public void use(String nick, String hostmask, String channel, String[] trigger);

    /**
		 *
		 * @return The description of the trigger
		 */
    public String getDescription();
  }



  // Java versionen: 
  public class TriggerService
  {
    private static TriggerService instance;
    private Dictionary<String, Trigger> triggerDict;
    //private IrcSender sender;

    private TriggerService()
    {
      triggerDict = new Dictionary<String, Trigger>();
    }

    public static TriggerService getInstance()
    {
      if (instance == null)
        instance = new TriggerService();
      return instance;
    }

    public void registerTrigger(Trigger trig, String triggerName)
    {
      triggerDict.Add(triggerName, trig);
    }

    public void useTrigger(String nick, String hostmask, String channel, String args)
    {
      String[] trigger = args.Split("!")[1].Split(" ");
      if (triggerDict.ContainsKey(trigger[0]))
      {
        // java: triggerDict.get(trigger[0]).use(nick, hostmask,channel,trigger);
        Trigger trig;

        triggerDict.TryGetValue(trigger[0], out trig);

        if (trig != null)
        {
          trig.use(nick, hostmask, channel, trigger);
        }

      }

      if (trigger[0].Equals("triggers"))
        showTriggers(nick);
    }


    // Sends the descriptions of all triggers to user
    private void showTriggers(String who)
    {
      foreach (KeyValuePair<string, Trigger> kvp in triggerDict)
      {
        if (triggerDict.ContainsKey(kvp.Key) == true)
        {
          Console.WriteLine("showTriggers> Found key " + kvp.Key);
          // JOEN TODO sender.sendNotice(who, "!" + kvp.Key.ToString() + " - " + triggerDict[kvp.Key].getDescription());
        }

      }
    }
  }





}